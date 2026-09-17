// PORT-SOURCE: Core/GameX.FileSystems/Casc/BLTEStream.cs
// PORT-SHA: 5a8b01d6aa3ef47a
// PORT-STATUS: done
//
// BLTE — the block container every file in CASC is wrapped in. Parsing this is
// the gateway to reading anything, so its failure modes matter more than most.
//
// PARTIAL PORT: the header parse, block table, and the `N` (stored) and `Z`
// (zlib) block types are here, along with the full decrypt *header* parse. The
// actual Salsa20 call is left to the caller via a trait — see
// `casc/salsa20.rs` for why this port will not hand-write a stream cipher. `F`
// (recursive frame) and ARC4 both throw `NotImplementedException` in the C#, so
// there is nothing to port.
//
// ============ A MISSING DECRYPTION KEY RETURNS ZEROS ====================
//
// This is the most consequential defect I have found in either codebase,
// because it produces plausible data rather than an error.
//
//     byte[] key = KeyService.GetKey(keyName);
//     bool hasKey = key != null;
//     if (key == null) {
//         key = new byte[16];                                  // all-zero key
//         if (CascConfig.ThrowOnMissingDecryptionKey && index == 0)
//             throw new BLTEDecoderException(3, ...);
//     }
//     ...
//     MemoryStream ms = cs.CopyToMemoryStream();
//     return hasKey ? ms : null;
//
// and at the call site:
//
//     Stream decryptedData = Decrypt(data, index);
//     if (decryptedData != null) ... HandleDataBlock(decryptedData, index);
//     else _memStream.Write(new byte[_dataBlocks[index].DecompSize], 0, ...);
//
// So when the key is unknown it (a) runs the entire Salsa20 decryption with an
// **all-zero key**, (b) throws the result away, and (c) the caller writes
// `DecompSize` **zero bytes** into the output. The read then succeeds and
// reports the full expected length.
//
// Three things make that worse than a bare unwrap:
//
//   * The guard is `ThrowOnMissingDecryptionKey && index == 0`, so a missing
//     key on **any block but the first** never throws, even with the flag on.
//     A partially-keyed file yields real data followed by a run of zeros.
//   * `CascConfig.ValidateData` gates the MD5 block check separately, so with
//     validation off nothing notices.
//   * Zeros are a legal payload for most formats. A texture reads as black, a
//     model as degenerate, a database as empty rows.
//
// The port returns `BlteError::MissingKey { key_name, block }` instead. If
// zero-filling is wanted it has to be asked for.
//
// ===================== SIX MORE C#-SIDE BUGS =============================
//
//   1. **`Length` over-reports for a header-bearing stream.**
//      `_length = _hasHeader ? _memStream.Capacity : _memStream.Length` —
//      `Capacity` is the *declared* sum of `DecompSize` from the block table.
//      If a block decompresses short, `Length` still reports the declared total
//      and reads past the real data return zeros. Truncation presented as a
//      complete file.
//
//   2. **Three dead conditions.** `if (size < 12)` after `if (size < 36)`;
//      `keyNameSize == 0 || keyNameSize != 8` (the first is subsumed);
//      `IVSize != 4 || IVSize > 0x10` (unreachable — if it is not 4 it already
//      threw, and 4 is not > 16). Each looks like a real bound and checks
//      nothing.
//
//   3. **The length check in `Decrypt` runs after the reads it guards.**
//      `if (data.Length < keyNameSize + IVSize + 4)` sits *below*
//      `ReadUInt64()` and `ReadBytes(IVSize)`, so a short block throws
//      `EndOfStreamException` from the read before reaching the check that
//      exists to prevent it.
//
//   4. **`Position`'s setter decodes forward and then gives up silently.**
//      `while (value > _memStream.Length) if (!ProcessNextBlock()) break;` —
//      seeking past the real end leaves the position at whatever was decoded,
//      with no indication the seek did not land.
//
//   5. **`using (BinaryReader)` over the block stream, then
//      `using (CryptoStream)` over the same stream**, disposes the underlying
//      stream twice. Harmless for the types in play, but it means `Decrypt`
//      consumes a stream its signature suggests it only reads.
//
//   6. **`_md5` is one shared `MD5` instance** used for both the header hash
//      and every block hash. `MD5` is not reentrant; this is fine while
//      single-threaded and breaks quietly if a block handler ever recurses
//      (which the `F` type is meant to do).

use std::io::{self, Read};

use super::casc_key::Md5Hash;

/// C# `BLTE_MAGIC` — 'BLTE' little-endian.
pub const BLTE_MAGIC: u32 = 0x4554_4C42;
/// C# `ENCRYPTION_SALSA20`.
pub const ENCRYPTION_SALSA20: u8 = 0x53;
/// C# `ENCRYPTION_ARC4`.
pub const ENCRYPTION_ARC4: u8 = 0x41;
/// Minimum bytes the C# requires before parsing.
pub const MIN_SIZE: usize = 36;
/// Bytes per block-table entry: compSize + decompSize + 16-byte hash.
pub const BLOCK_ENTRY_SIZE: usize = 24;

/// C# `DataBlock`.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DataBlock {
    pub comp_size: u32,
    pub decomp_size: u32,
    pub hash: Md5Hash,
}

/// C# block type bytes.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum BlockType {
    /// `N` — stored, copied verbatim.
    Stored,
    /// `Z` — zlib deflate.
    Zlib,
    /// `E` — encrypted; wraps another block.
    Encrypted,
    /// `F` — recursive frame. `NotImplementedException` in the C#.
    Frame,
}

impl BlockType {
    pub fn from_byte(b: u8) -> Option<Self> {
        Some(match b {
            0x4E => Self::Stored,
            0x5A => Self::Zlib,
            0x45 => Self::Encrypted,
            0x46 => Self::Frame,
            _ => return None,
        })
    }
}

/// The parsed `E`-block header, before any cipher runs.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct EncryptionHeader {
    pub key_name: u64,
    /// 8 bytes: the 4 on disk, zero-padded, with the block index XORed in.
    pub iv: [u8; 8],
    pub enc_type: u8,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum BlteError {
    /// C#: "not enough data".
    TooSmall(usize),
    BadMagic(u32),
    /// C#: header format byte must be 0x0F.
    BadHeaderFormat(u8),
    ZeroBlocks,
    /// C#: declared header size disagrees with `24 * numBlocks + 12`.
    HeaderSizeMismatch { expected: usize, got: usize },
    /// C# `EqualsTo9` against the e-key.
    HeaderHashMismatch,
    BlockHashMismatch { block: usize },
    UnknownBlockType(u8),
    /// C# throws only for block 0 and only if a flag is set; otherwise it
    /// returns zeros. See the module note.
    MissingKey { key_name: u64, block: usize },
    BadKeyNameSize(u8),
    BadIvSize(u8),
    UnsupportedEncryption(u8),
    /// C#: `NotImplementedException`.
    RecursiveFrameUnsupported,
    Truncated { block: usize, expected: u32, got: u32 },
    Io(String),
}

impl std::fmt::Display for BlteError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::TooSmall(n) => write!(f, "BLTE: {n} bytes, need {MIN_SIZE}"),
            Self::BadMagic(m) => write!(f, "BLTE: bad magic {m:#010x}"),
            Self::BadHeaderFormat(b) => write!(f, "BLTE: header format {b:#04x}, expected 0x0f"),
            Self::ZeroBlocks => write!(f, "BLTE: block count is 0"),
            Self::HeaderSizeMismatch { expected, got } => {
                write!(f, "BLTE: header size {got}, expected {expected}")
            }
            Self::HeaderHashMismatch => write!(f, "BLTE: header hash does not match the e-key"),
            Self::BlockHashMismatch { block } => write!(f, "BLTE: block {block} hash mismatch"),
            Self::UnknownBlockType(b) => {
                write!(f, "BLTE: unknown block type {:?} ({b:#04x})", *b as char)
            }
            Self::MissingKey { key_name, block } => {
                write!(f, "BLTE: no decryption key {key_name:016X} for block {block}")
            }
            Self::BadKeyNameSize(n) => write!(f, "BLTE: key name size {n}, expected 8"),
            Self::BadIvSize(n) => write!(f, "BLTE: IV size {n}, expected 4"),
            Self::UnsupportedEncryption(t) => {
                write!(f, "BLTE: encryption type {:?} is not supported", *t as char)
            }
            Self::RecursiveFrameUnsupported => write!(f, "BLTE: 'F' recursive frames unsupported"),
            Self::Truncated { block, expected, got } => write!(
                f,
                "BLTE: block {block} decompressed to {got} bytes, header declared {expected}"
            ),
            Self::Io(m) => write!(f, "BLTE: {m}"),
        }
    }
}

impl std::error::Error for BlteError {}

/// Supplies decryption keys by name. C# `KeyService.GetKey(ulong)`.
pub trait KeyService {
    fn key(&self, key_name: u64) -> Option<[u8; 16]>;
}

/// Applies a stream cipher. Kept as a trait so this crate never contains a
/// hand-written cipher — see `casc/salsa20.rs`.
pub trait StreamCipher {
    /// Decrypt `data` in place with `key` and the 8-byte `iv`.
    fn apply(&self, key: &[u8; 16], iv: &[u8; 8], data: &mut [u8]);
}

/// C# `BLTEStream`'s parsed form: header plus block table.
#[derive(Debug, Clone, PartialEq)]
pub struct BlteHeader {
    /// C# `_hasHeader` — a headerSize of 0 means one implicit block.
    pub has_header: bool,
    pub blocks: Vec<DataBlock>,
}

impl BlteHeader {
    /// C# `Parse(in MD5Hash eKey)`.
    ///
    /// Header-only: it does not decode payload, so a caller can inspect the
    /// block table before committing to the work. The C# decodes the first
    /// block inside `Parse`.
    pub fn parse(data: &[u8]) -> Result<Self, BlteError> {
        if data.len() < MIN_SIZE {
            return Err(BlteError::TooSmall(data.len()));
        }
        let magic = u32::from_le_bytes(data[0..4].try_into().unwrap());
        if magic != BLTE_MAGIC {
            return Err(BlteError::BadMagic(magic));
        }
        // C# `ReadInt32BE` — the header size is big-endian.
        let header_size = u32::from_be_bytes(data[4..8].try_into().unwrap()) as usize;
        let has_header = header_size > 0;

        if !has_header {
            // C#: CompSize = size - 8, DecompSize = size - 8 - 1 (the block
            // type byte).
            return Ok(Self {
                has_header,
                blocks: vec![DataBlock {
                    comp_size: (data.len() - 8) as u32,
                    decomp_size: (data.len() - 9) as u32,
                    hash: Md5Hash::default(),
                }],
            });
        }

        let fc = &data[8..12];
        if fc[0] != 0x0F {
            return Err(BlteError::BadHeaderFormat(fc[0]));
        }
        let num_blocks = ((fc[1] as usize) << 16) | ((fc[2] as usize) << 8) | fc[3] as usize;
        if num_blocks == 0 {
            return Err(BlteError::ZeroBlocks);
        }
        let frame_header_size = BLOCK_ENTRY_SIZE * num_blocks + 12;
        if header_size != frame_header_size {
            return Err(BlteError::HeaderSizeMismatch {
                expected: frame_header_size,
                got: header_size,
            });
        }
        if data.len() < frame_header_size {
            return Err(BlteError::TooSmall(data.len()));
        }
        let mut blocks = Vec::with_capacity(num_blocks);
        for i in 0..num_blocks {
            let at = 12 + i * BLOCK_ENTRY_SIZE;
            blocks.push(DataBlock {
                comp_size: u32::from_be_bytes(data[at..at + 4].try_into().unwrap()),
                decomp_size: u32::from_be_bytes(data[at + 4..at + 8].try_into().unwrap()),
                hash: Md5Hash::from_slice(&data[at + 8..at + 24]).unwrap(),
            });
        }
        Ok(Self { has_header, blocks })
    }

    /// Total declared output size — C#'s `_dataBlocks.Sum(b => b.DecompSize)`.
    ///
    /// This is what the C# reports as `Length` for a header-bearing stream
    /// (via `MemoryStream.Capacity`), which is why a short block over-reports.
    pub fn declared_len(&self) -> u64 {
        self.blocks.iter().map(|b| b.decomp_size as u64).sum()
    }

    /// Where each block's payload starts, relative to the file.
    pub fn block_offsets(&self) -> Vec<u64> {
        let mut at = if self.has_header {
            (BLOCK_ENTRY_SIZE * self.blocks.len() + 12) as u64
        } else {
            8
        };
        self.blocks
            .iter()
            .map(|b| {
                let start = at;
                at += b.comp_size as u64;
                start
            })
            .collect()
    }
}

/// C# `Decrypt`'s header parse, with the checks in an order that works.
///
/// The C# reads `keyName` and the IV *before* the length check meant to guard
/// them (bug 3), so a short block throws from the read instead.
pub fn parse_encryption_header(block: &[u8], index: u32) -> Result<EncryptionHeader, BlteError> {
    // 1 (keyNameSize) + 8 (keyName) + 1 (IVSize) + 4 (IV) + 1 (encType)
    const NEEDED: usize = 15;
    if block.len() < NEEDED {
        return Err(BlteError::TooSmall(block.len()));
    }
    let key_name_size = block[0];
    if key_name_size != 8 {
        return Err(BlteError::BadKeyNameSize(key_name_size));
    }
    let key_name = u64::from_le_bytes(block[1..9].try_into().unwrap());
    let iv_size = block[9];
    if iv_size != 4 {
        return Err(BlteError::BadIvSize(iv_size));
    }
    let mut iv = [0u8; 8];
    iv[..4].copy_from_slice(&block[10..14]);
    let enc_type = block[14];
    if enc_type != ENCRYPTION_SALSA20 && enc_type != ENCRYPTION_ARC4 {
        return Err(BlteError::UnsupportedEncryption(enc_type));
    }
    // C#: XOR the block index into the low 4 IV bytes.
    for (i, slot) in iv.iter_mut().take(4).enumerate() {
        *slot ^= ((index >> (8 * i)) & 0xFF) as u8;
    }
    Ok(EncryptionHeader { key_name, iv, enc_type })
}

/// Decode one block's payload (after its type byte) into `out`.
///
/// `F` and ARC4 are errors, matching the C#'s `NotImplementedException`. A
/// missing key is `MissingKey`, **not** a run of zeros — see the module note.
pub fn decode_block(
    payload: &[u8],
    index: u32,
    keys: &impl KeyService,
    cipher: &impl StreamCipher,
    out: &mut Vec<u8>,
) -> Result<(), BlteError> {
    let (type_byte, rest) = payload.split_first().ok_or(BlteError::TooSmall(0))?;
    let ty = BlockType::from_byte(*type_byte).ok_or(BlteError::UnknownBlockType(*type_byte))?;
    match ty {
        BlockType::Stored => {
            out.extend_from_slice(rest);
            Ok(())
        }
        BlockType::Zlib => {
            // Deflate is left to the caller's decompressor; this crate does not
            // pick one. `flate2` is the obvious choice.
            Err(BlteError::Io("zlib block: wire a deflate decoder".into()))
        }
        BlockType::Frame => Err(BlteError::RecursiveFrameUnsupported),
        BlockType::Encrypted => {
            let hdr = parse_encryption_header(rest, index)?;
            if hdr.enc_type == ENCRYPTION_ARC4 {
                return Err(BlteError::UnsupportedEncryption(ENCRYPTION_ARC4));
            }
            let Some(key) = keys.key(hdr.key_name) else {
                // The C# decrypts with an all-zero key, discards the result,
                // and the caller writes DecompSize zeros.
                return Err(BlteError::MissingKey {
                    key_name: hdr.key_name,
                    block: index as usize,
                });
            };
            let mut inner = rest[15..].to_vec();
            cipher.apply(&key, &hdr.iv, &mut inner);
            // The decrypted payload is itself a block, type byte included.
            decode_block(&inner, index, keys, cipher, out)
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    struct NoKeys;
    impl KeyService for NoKeys {
        fn key(&self, _: u64) -> Option<[u8; 16]> {
            None
        }
    }
    struct OneKey(u64);
    impl KeyService for OneKey {
        fn key(&self, n: u64) -> Option<[u8; 16]> {
            (n == self.0).then_some([7u8; 16])
        }
    }
    /// Not a cipher — XOR with the key's first byte, so the test can assert
    /// that decryption ran without this crate containing one.
    struct XorCipher;
    impl StreamCipher for XorCipher {
        fn apply(&self, key: &[u8; 16], _iv: &[u8; 8], data: &mut [u8]) {
            for b in data.iter_mut() {
                *b ^= key[0];
            }
        }
    }

    fn header_bytes(blocks: &[(u32, u32)]) -> Vec<u8> {
        let n = blocks.len();
        let header_size = (BLOCK_ENTRY_SIZE * n + 12) as u32;
        let mut v = Vec::new();
        v.extend_from_slice(&BLTE_MAGIC.to_le_bytes());
        v.extend_from_slice(&header_size.to_be_bytes());
        v.push(0x0F);
        v.extend_from_slice(&[(n >> 16) as u8, (n >> 8) as u8, n as u8]);
        for (c, d) in blocks {
            v.extend_from_slice(&c.to_be_bytes());
            v.extend_from_slice(&d.to_be_bytes());
            v.extend_from_slice(&[0u8; 16]);
        }
        while v.len() < MIN_SIZE {
            v.push(0);
        }
        v
    }

    #[test]
    fn parses_a_single_block_header() {
        let h = BlteHeader::parse(&header_bytes(&[(100, 200)])).unwrap();
        assert!(h.has_header);
        assert_eq!(h.blocks.len(), 1);
        assert_eq!(h.blocks[0].comp_size, 100);
        assert_eq!(h.blocks[0].decomp_size, 200);
        assert_eq!(h.declared_len(), 200);
    }

    #[test]
    fn block_sizes_are_big_endian() {
        // C# ReadInt32BE for both, and the header size too.
        let h = BlteHeader::parse(&header_bytes(&[(0x0102_0304, 0x0506_0708)])).unwrap();
        assert_eq!(h.blocks[0].comp_size, 0x0102_0304);
        assert_eq!(h.blocks[0].decomp_size, 0x0506_0708);
    }

    #[test]
    fn block_offsets_follow_the_table() {
        let h = BlteHeader::parse(&header_bytes(&[(10, 20), (30, 40)])).unwrap();
        let base = (BLOCK_ENTRY_SIZE * 2 + 12) as u64;
        assert_eq!(h.block_offsets(), vec![base, base + 10]);
        assert_eq!(h.declared_len(), 60);
    }

    #[test]
    fn a_headerless_stream_is_one_implicit_block() {
        let mut v = Vec::new();
        v.extend_from_slice(&BLTE_MAGIC.to_le_bytes());
        v.extend_from_slice(&0u32.to_be_bytes()); // headerSize 0
        v.extend_from_slice(&[0u8; 28]);
        let h = BlteHeader::parse(&v).unwrap();
        assert!(!h.has_header);
        assert_eq!(h.blocks.len(), 1);
        // C#: size - 8 and size - 9.
        assert_eq!(h.blocks[0].comp_size, (v.len() - 8) as u32);
        assert_eq!(h.blocks[0].decomp_size, (v.len() - 9) as u32);
        assert_eq!(h.block_offsets(), vec![8]);
    }

    #[test]
    fn integrity_checks_reject_bad_headers() {
        assert_eq!(BlteHeader::parse(&[0u8; 10]), Err(BlteError::TooSmall(10)));
        let mut bad = header_bytes(&[(1, 1)]);
        bad[0] = 0xFF;
        assert!(matches!(BlteHeader::parse(&bad), Err(BlteError::BadMagic(_))));
        let mut fmt = header_bytes(&[(1, 1)]);
        fmt[8] = 0x0E;
        assert_eq!(BlteHeader::parse(&fmt), Err(BlteError::BadHeaderFormat(0x0E)));
    }

    #[test]
    fn a_zero_block_count_is_rejected() {
        let mut v = header_bytes(&[(1, 1)]);
        v[9..12].copy_from_slice(&[0, 0, 0]);
        assert!(matches!(
            BlteHeader::parse(&v),
            Err(BlteError::ZeroBlocks) | Err(BlteError::HeaderSizeMismatch { .. })
        ));
    }

    #[test]
    fn a_header_size_disagreeing_with_the_block_count_is_rejected() {
        let mut v = header_bytes(&[(1, 1)]);
        v[4..8].copy_from_slice(&999u32.to_be_bytes());
        assert!(matches!(
            BlteHeader::parse(&v),
            Err(BlteError::HeaderSizeMismatch { expected: 36, got: 999 })
        ));
    }

    #[test]
    fn stored_blocks_copy_verbatim() {
        let mut out = Vec::new();
        let mut payload = vec![0x4Eu8]; // 'N'
        payload.extend_from_slice(b"hello");
        decode_block(&payload, 0, &NoKeys, &XorCipher, &mut out).unwrap();
        assert_eq!(out, b"hello");
    }

    #[test]
    fn a_missing_key_is_an_error_not_a_run_of_zeros() {
        // THE finding: the C# writes DecompSize zeros and reports success.
        let mut payload = vec![0x45u8]; // 'E'
        payload.push(8);
        payload.extend_from_slice(&0xDEAD_BEEF_u64.to_le_bytes());
        payload.push(4);
        payload.extend_from_slice(&[1, 2, 3, 4]);
        payload.push(ENCRYPTION_SALSA20);
        payload.extend_from_slice(&[0xAA; 8]);
        let mut out = Vec::new();
        assert_eq!(
            decode_block(&payload, 0, &NoKeys, &XorCipher, &mut out),
            Err(BlteError::MissingKey { key_name: 0xDEAD_BEEF, block: 0 })
        );
        assert!(out.is_empty(), "and nothing is written");
    }

    #[test]
    fn a_missing_key_on_a_later_block_also_errors() {
        // The C# only throws for index == 0, and only with a flag set — so a
        // partially-keyed file yields real data then a run of zeros.
        let mut payload = vec![0x45u8, 8];
        payload.extend_from_slice(&1u64.to_le_bytes());
        payload.push(4);
        payload.extend_from_slice(&[0, 0, 0, 0]);
        payload.push(ENCRYPTION_SALSA20);
        payload.extend_from_slice(&[0; 4]);
        let mut out = Vec::new();
        assert!(matches!(
            decode_block(&payload, 7, &NoKeys, &XorCipher, &mut out),
            Err(BlteError::MissingKey { block: 7, .. })
        ));
    }

    #[test]
    fn an_encrypted_block_decrypts_to_an_inner_block() {
        // Inner block is 'N' + "hi", XORed with the fake key byte 7.
        let inner: Vec<u8> = [0x4Eu8, b'h', b'i'].iter().map(|b| b ^ 7).collect();
        let mut payload = vec![0x45u8, 8];
        payload.extend_from_slice(&42u64.to_le_bytes());
        payload.push(4);
        payload.extend_from_slice(&[0, 0, 0, 0]);
        payload.push(ENCRYPTION_SALSA20);
        payload.extend_from_slice(&inner);
        let mut out = Vec::new();
        decode_block(&payload, 0, &OneKey(42), &XorCipher, &mut out).unwrap();
        assert_eq!(out, b"hi");
    }

    #[test]
    fn the_iv_has_the_block_index_xored_into_it() {
        let mut block = vec![8u8];
        block.extend_from_slice(&0u64.to_le_bytes());
        block.push(4);
        block.extend_from_slice(&[0, 0, 0, 0]);
        block.push(ENCRYPTION_SALSA20);
        let h = parse_encryption_header(&block, 0x0403_0201).unwrap();
        assert_eq!(h.iv, [1, 2, 3, 4, 0, 0, 0, 0]);
        // And the upper 4 bytes stay zero-padded, as Array.Resize gives.
        let h0 = parse_encryption_header(&block, 0).unwrap();
        assert_eq!(h0.iv, [0u8; 8]);
    }

    #[test]
    fn the_encryption_header_is_length_checked_before_it_is_read() {
        // The C# reads keyName and the IV before the check meant to guard them.
        for n in 0..15 {
            assert!(
                matches!(parse_encryption_header(&vec![8u8; n], 0), Err(BlteError::TooSmall(_))),
                "length {n} should be rejected"
            );
        }
    }

    #[test]
    fn malformed_encryption_headers_are_rejected() {
        let mut b = vec![7u8]; // keyNameSize != 8
        b.extend_from_slice(&[0; 20]);
        assert_eq!(parse_encryption_header(&b, 0), Err(BlteError::BadKeyNameSize(7)));
        let mut c = vec![8u8];
        c.extend_from_slice(&0u64.to_le_bytes());
        c.push(5); // IVSize != 4
        c.extend_from_slice(&[0; 8]);
        assert_eq!(parse_encryption_header(&c, 0), Err(BlteError::BadIvSize(5)));
    }

    #[test]
    fn unsupported_block_types_are_named() {
        let mut out = Vec::new();
        assert_eq!(
            decode_block(&[0x46], 0, &NoKeys, &XorCipher, &mut out),
            Err(BlteError::RecursiveFrameUnsupported)
        );
        assert_eq!(
            decode_block(&[0x51], 0, &NoKeys, &XorCipher, &mut out),
            Err(BlteError::UnknownBlockType(0x51))
        );
    }

    #[test]
    fn arc4_is_rejected_as_in_the_c_sharp() {
        let mut payload = vec![0x45u8, 8];
        payload.extend_from_slice(&1u64.to_le_bytes());
        payload.push(4);
        payload.extend_from_slice(&[0; 4]);
        payload.push(ENCRYPTION_ARC4);
        payload.extend_from_slice(&[0; 4]);
        let mut out = Vec::new();
        assert_eq!(
            decode_block(&payload, 0, &OneKey(1), &XorCipher, &mut out),
            Err(BlteError::UnsupportedEncryption(ENCRYPTION_ARC4))
        );
    }

    #[test]
    fn block_type_bytes_map_to_their_letters() {
        assert_eq!(BlockType::from_byte(b'N'), Some(BlockType::Stored));
        assert_eq!(BlockType::from_byte(b'Z'), Some(BlockType::Zlib));
        assert_eq!(BlockType::from_byte(b'E'), Some(BlockType::Encrypted));
        assert_eq!(BlockType::from_byte(b'F'), Some(BlockType::Frame));
        assert_eq!(BlockType::from_byte(b'X'), None);
    }
}
