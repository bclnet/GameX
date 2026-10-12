// PORT-SOURCE: Core/GameX/_LIB/Compression/Blast.cs
// PORT-SHA: e3b1caaba88a400a
// PORT-STATUS: done
//
// Mark Adler's blast.c (PKWARE DCL implode decompressor), as C# has it, in safe Rust:
// - Pointers are indices: `inx` indexes `inxs`, the buffer C#'s `inx` points into (the
//   caller's `hold`); the copy pointers in `decomp` index `outx`.
// - `blast_in` fills `inxs` and returns its length (C# sets `ref byte* buf` to its `hold`).
//   `blast_out` takes the bytes to write. Their opaque `how` is whatever the closure captures.
// - C# stores `infun`/`outfun`/`inhow`/`outhow` as fields; closures that borrow the caller's
//   buffers can't live in `self`, so `blast` passes `infun`/`outfun` down to `decomp`,
//   `bits` and `decode` instead.
// - The "EOF" exception (blast.c's longjmp) is an `Err`, caught in `blast` as error 2.
// - The static constructor is three `LazyLock`s; `litcnt`/`litsym` etc. are the
//   `count`/`symbol` arrays inside each `Huffman`.
// - `Decompress(byte[], byte[])` is `decompress`; `Decompress(byte[], Stream)` is `decompressStream`.
//   A failed write in `decompressStream` returns 1 (blast.c's output error) where C#'s
//   `Stream.Write` throws; past the end of `outputBuffer`, `decompress` panics where C#'s
//   `Memcpy` overruns it.

use std::io::{self, Write};
use std::sync::LazyLock;

pub type blast_in<'a> = dyn FnMut(&mut Vec<u8>) -> i32 + 'a;
pub type blast_out<'a> = dyn FnMut(&[u8]) -> i32 + 'a;

const MAXBITS: usize = 13;             // maximum code length
const MAXWIN: usize = 4096;            // maximum window size

static litcode: LazyLock<Huffman> = LazyLock::new(|| Huffman::new(256, &litlen));      // length code
static lencode: LazyLock<Huffman> = LazyLock::new(|| Huffman::new(16, &lenlen));       // length code
static distcode: LazyLock<Huffman> = LazyLock::new(|| Huffman::new(64, &distlen));     // distance code
static litlen: [u8; 98] = [
    11, 124, 8, 7, 28, 7, 188, 13, 76, 4, 10, 8, 12, 10, 12, 10, 8, 23, 8,
    9, 7, 6, 7, 8, 7, 6, 55, 8, 23, 24, 12, 11, 7, 9, 11, 12, 6, 7, 22, 5,
    7, 24, 6, 11, 9, 6, 7, 22, 7, 11, 38, 7, 9, 8, 25, 11, 8, 11, 9, 12,
    8, 12, 5, 38, 5, 38, 5, 11, 7, 5, 6, 21, 6, 10, 53, 8, 7, 24, 10, 27,
    44, 253, 253, 253, 252, 252, 252, 13, 12, 45, 12, 45, 12, 61, 12, 45,
    44, 173]; // bit lengths of literal codes
static lenlen: [u8; 6] = [2, 35, 36, 53, 38, 23]; // bit lengths of length codes 0..15
static distlen: [u8; 7] = [2, 20, 53, 230, 247, 151, 248]; // bit lengths of distance codes 0..63
static basex: [i16; 16] = [3, 2, 4, 5, 6, 7, 8, 9, 10, 12, 16, 24, 40, 72, 136, 264]; // base for length codes
static extra: [u8; 16] = [0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8]; // extra bits for length codes

pub struct Blast {
    // input state
    inxs: Vec<u8>,                  // buffer the next input location points into
    inx: usize,                     // next input location
    left: i32,                      // available input at in
    bitbuf: i32,                    // bit buffer
    bitcnt: i32,                    // number of bits in bit buffer

    // output state
    next: usize,                    // index of next write location in out[]
    first: i32,                     // true to check distances (for first 4K)
    outx: Vec<u8>,                  // output buffer and sliding window
}

impl Blast {
    pub fn new() -> Self { Self { inxs: Vec::new(), inx: 0, left: 0, bitbuf: 0, bitcnt: 0, next: 0, first: 0, outx: vec![0u8; MAXWIN] } }

    // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L66
    /// Return need bits from the input stream. This always leaves less than eight bits in the buffer. bits() works properly for need == 0.
    ///
    /// Format notes:
    /// - Bits are stored in bytes from the least significant bit to the most significant bit. Therefore bits are dropped from the bottom of the bit
    ///   buffer, using shift right, and new bytes are appended to the top of the bit buffer, using shift left.
    fn bits(&mut self, need: i32, infun: &mut blast_in) -> io::Result<i32> {
        let mut val: i32; // bit accumulator

        // load at least need bits into val
        val = self.bitbuf;
        while self.bitcnt < need {
            if self.left == 0 {
                self.left = infun(&mut self.inxs); self.inx = 0;
                if self.left == 0 { return Err(io::Error::new(io::ErrorKind::UnexpectedEof, "EOF")); } // out of input
            }
            val |= (self.inxs[self.inx] as i32) << self.bitcnt; self.inx += 1; // load eight bits
            self.left -= 1;
            self.bitcnt += 8;
        }

        // drop need bits and update buffer, always zero to seven bits left
        self.bitbuf = val >> need;
        self.bitcnt -= need;

        // return need bits, zeroing the bits above that
        Ok(val & ((1 << need) - 1))
    }

    // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L123
    /// Decode a code from the stream s using huffman table h. Return the symbol or a negative value if there is an error. If all of the lengths are zero, i.e.
    /// an empty code, or if the code is incomplete and an invalid code is received, then -9 is returned after reading MAXBITS bits.
    ///
    /// Format notes:
    /// - The codes as stored in the compressed data are bit-reversed relative to a simple integer ordering of codes of the same lengths. Hence below the
    ///   bits are pulled from the compressed data one at a time and used to build the code value reversed from what is in the stream in order to
    ///   permit simple integer comparisons for decoding.
    ///
    /// - The first code for the shortest length is all ones. Subsequent codes of the same length are simply integer decrements of the previous code.  When
    ///   moving up a length, a one bit is appended to the code. For a complete code, the last code of the longest length will be all zeros.  To support
    ///   this ordering, the bits pulled during decoding are inverted to apply the more "natural" ordering starting with all zeros and incrementing.
    fn decode(&mut self, h: &Huffman, infun: &mut blast_in) -> io::Result<i32> {
        let mut len: i32;       // current number of bits in code
        let mut code: i32;      // len bits being decoded
        let mut first: i32;     // first code of length len
        let mut count: i32;     // number of codes of length len
        let mut index: i32;     // index of first code of length len in symbol table
        let mut bitbuf: i32;    // bits from stream
        let mut left: i32;      // bits left in next or left to process
        let mut next: usize;    // next number of codes

        bitbuf = self.bitbuf;
        left = self.bitcnt;
        code = 0; first = 0; index = 0;
        len = 1;

        next = 1;
        loop {
            while left != 0 {
                left -= 1;
                code |= (bitbuf & 1) ^ 1; // invert code
                bitbuf >>= 1;
                count = h.count[next] as i32; next += 1;
                if code < first + count {
                    // if length len, return symbol
                    self.bitbuf = bitbuf;
                    self.bitcnt = (self.bitcnt - len) & 7;
                    return Ok(h.symbol[(index + (code - first)) as usize] as i32);
                }
                index += count; // else update for next length
                first += count;
                first <<= 1;
                code <<= 1;
                len += 1;
            }
            left = MAXBITS as i32 + 1 - len;
            if left == 0 { break; }
            if self.left == 0 {
                self.left = infun(&mut self.inxs); self.inx = 0;
                if self.left == 0 { return Err(io::Error::new(io::ErrorKind::UnexpectedEof, "EOF")); } // out of input
            }
            bitbuf = self.inxs[self.inx] as i32; self.inx += 1;
            self.left -= 1;
            if left > 8 { left = 8; }
        }
        Ok(-9) // ran out of codes
    }

    // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L276
    /// Decode PKWare Compression Library stream.
    ///
    /// Format notes:
    ///
    /// - First byte is 0 if literals are uncoded or 1 if they are coded. Second byte is 4, 5, or 6 for the number of extra bits in the distance code.
    ///   This is the base-2 logarithm of the dictionary size minus six.
    ///
    /// - Compressed data is a combination of literals and length/distance pairs terminated by an end code. Literals are either Huffman coded or
    ///   uncoded bytes. A length/distance pair is a coded length followed by a coded distance to represent a string that occurs earlier in the
    ///   uncompressed data that occurs again at the current location.
    ///
    /// - A bit preceding a literal or length/distance pair indicates which comes next, 0 for literals, 1 for length/distance.
    ///
    /// - If literals are uncoded, then the next eight bits are the literal, in the normal bit order in th stream, i.e. no bit-reversal is needed. Similarly,
    ///   no bit reversal is needed for either the length extra bits or the distance extra bits.
    ///
    /// - Literal bytes are simply written to the output. A length/distance pair is an instruction to copy previously uncompressed bytes to the output. The
    ///   copy is from distance bytes back in the output stream, copying for length bytes.
    ///
    /// - Distances pointing before the beginning of the output data are not permitted.
    ///
    /// - Overlapped copies, where the length is greater than the distance, are allowed and common. For example, a distance of one and a length of 518
    ///   simply copies the last byte 518 times. A distance of four and a length of twelve copies the last four bytes three times. A simple forward copy
    ///   ignoring whether the length is greater than the distance or not implements this correctly.
    pub fn decomp(&mut self, infun: &mut blast_in, outfun: &mut blast_out) -> io::Result<i32> {
        let lit: i32;           // true if literals are coded
        let dict: i32;          // log2(dictionary size) - 6
        let mut symbol: i32;    // decoded symbol, extra bits for distance
        let mut len: i32;       // length for copy
        let mut dist: i32;      // distance for copy
        let mut copy: i32;      // copy counter
        let (mut from, mut to): (usize, usize); // copy pointers

        // read header
        lit = self.bits(8, infun)?;
        if lit > 1 { return Ok(-1); }
        dict = self.bits(8, infun)?;
        if dict < 4 || dict > 6 { return Ok(-2); }

        // decode literals and length/distance pairs
        loop {
            if self.bits(1, infun)? != 0 {
                // get length
                symbol = self.decode(&lencode, infun)?;
                len = basex[symbol as usize] as i32 + self.bits(extra[symbol as usize] as i32, infun)?;
                if len == 519 { break; } // end code

                // get distance
                symbol = if len == 2 { 2 } else { dict };
                dist = self.decode(&distcode, infun)? << symbol;
                dist += self.bits(symbol, infun)?;
                dist += 1;
                if self.first != 0 && dist > self.next as i32 { return Ok(-3); }    // distance too far back

                // copy length bytes from distance bytes back
                loop {
                    to = self.next;
                    from = to.wrapping_sub(dist as usize);
                    copy = MAXWIN as i32;
                    if (self.next as i32) < dist {
                        from = from.wrapping_add(copy as usize);
                        copy = dist;
                    }
                    copy -= self.next as i32;
                    if copy > len { copy = len; }
                    len -= copy;
                    self.next += copy as usize;
                    loop {
                        self.outx[to] = self.outx[from]; to += 1; from += 1;
                        copy -= 1;
                        if copy == 0 { break; }
                    }
                    if self.next == MAXWIN {
                        if outfun(&self.outx[..self.next]) != 0 { return Ok(1); }
                        self.next = 0;
                        self.first = 0;
                    }
                    if len == 0 { break; }
                }
            }
            else {
                // get literal and write it
                symbol = if lit != 0 { self.decode(&litcode, infun)? } else { self.bits(8, infun)? };
                self.outx[self.next] = symbol as u8; self.next += 1;
                if self.next == MAXWIN {
                    if outfun(&self.outx[..self.next]) != 0 { return Ok(1); }
                    self.next = 0;
                    self.first = 0;
                }
            }
        }
        Ok(0)
    }

    // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L377
    pub fn blast(&mut self, infun: &mut blast_in, outfun: &mut blast_out) -> i32 {
        // initialize input state
        self.left = 0;
        self.bitbuf = 0;
        self.bitcnt = 0;

        // initialize output state
        self.next = 0;
        self.first = 1;

        // return if bits() or decode() tries to read past available input
        let mut err = match self.decomp(infun, outfun) { // decompress
            Ok(err) => err,
            Err(_) => 2, // if came back here via longjmp(), then skip decomp(), return error
        };

        // write any leftover output and update the error code if needed
        if err != 1 && self.next != 0 && outfun(&self.outx[..self.next]) != 0 && err == 0 { err = 1; }
        err
    }

    // Decompress a PKWare Compression Library stream from stdin to stdout
    // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L428
    pub fn decompress(&mut self, inputBuffer: &[u8], outputBuffer: &mut [u8]) -> io::Result<i32> {
        const CHUNK: usize = 16384;
        let (mut inputLen, mut outputLen) = (inputBuffer.len() as i32, outputBuffer.len() as i32);
        let (mut inputPtr, mut outputPtr) = (0usize, 0usize);
        let mut inf = |buf: &mut Vec<u8>| -> i32 {
            if inputLen <= 0 { return 0; }
            let len = (inputLen as usize).min(CHUNK);
            buf.clear(); buf.extend_from_slice(&inputBuffer[inputPtr..inputPtr + len]);
            inputPtr += len;
            inputLen -= len as i32;
            len as i32
        };
        let mut outf = |buf: &[u8]| -> i32 {
            if outputLen <= 0 { return 0; }
            outputBuffer[outputPtr..outputPtr + buf.len()].copy_from_slice(buf);
            outputPtr += buf.len();
            outputLen -= buf.len() as i32;
            0
        };

        // decompress to stdout
        let ret = self.blast(&mut inf, &mut outf);
        if ret != 0 { return Err(io::Error::new(io::ErrorKind::InvalidData, format!("blast error: {ret}"))); }
        Ok(0)
    }

    pub fn decompressStream<W: Write>(&mut self, inputBuffer: &[u8], outputBuffer: &mut W) -> io::Result<i32> {
        const CHUNK: usize = 16384;
        let mut inputLen = inputBuffer.len() as i32;
        let mut inputPtr = 0usize;
        let mut inf = |buf: &mut Vec<u8>| -> i32 {
            if inputLen <= 0 { return 0; }
            let len = (inputLen as usize).min(CHUNK);
            buf.clear(); buf.extend_from_slice(&inputBuffer[inputPtr..inputPtr + len]);
            inputPtr += len;
            inputLen -= len as i32;
            len as i32
        };
        let mut outf = |buf: &[u8]| -> i32 {
            if outputBuffer.write_all(buf).is_err() { return 1; }
            //if (outputLen <= 0) return 0;
            //UnsafeX.Memcpy(outputPtr, (IntPtr)buf, (uint)length);
            //outputPtr += length;
            //outputLen -= length;
            0
        };
        // decompress
        let ret = self.blast(&mut inf, &mut outf);
        if ret != 0 { return Err(io::Error::new(io::ErrorKind::InvalidData, format!("blast error: {ret}"))); }
        Ok(0)
    }
}

/// Huffman code decoding tables. count[1..MAXBITS] is the number of symbols of each length, which for a canonical code are stepped through in order.
/// symbol[] are the symbol values in canonical order, where the number of entries is the sum of the counts in count[]. The decoding process can be
/// seen in the function decode() below.
#[derive(Clone, Default)]
struct Huffman {
    pub count: Vec<i16>,        // number of symbols of each length
    pub symbol: Vec<i16>,       // canonically ordered symbols
}

impl Huffman {
    fn new(symbols: usize, rep: &[u8]) -> Self { let mut h = Self { count: vec![0; MAXBITS + 1], symbol: vec![0; symbols] }; h.construct(rep, rep.len() as i32); h }

    // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L185
    /// Given a list of repeated code lengths rep[0..n - 1], where each byte is a count(high four bits + 1) and a code length(low four bits), generate the
    /// list of code lengths.This compaction reduces the size of the object code. Then given the list of code lengths length[0..n-1] representing a canonical
    /// Huffman code for n symbols, construct the tables required to decode those codes. Those tables are the number of codes of each length, and the symbols
    /// sorted by length, retaining their original order within each length.The return value is zero for a complete code set, negative for an over-
    /// subscribed code set, and positive for an incomplete code set. The tables can be used if the return value is zero or positive, but they cannot be used
    /// if the return value is negative. If the return value is zero, it is not possible for decode() using that table to return an error--any stream of
    /// enough bits will resolve to a symbol.If the return value is positive, then it is possible for decode() using that table to return an error for received
    /// codes past the end of the incomplete lengths.
    pub fn construct(&mut self, rep: &[u8], mut n: i32) -> i32 {
        let mut symbol: i16;                            // current symbol when stepping through length[]
        let mut len: i16;                               // current length when stepping through h.count[]
        let mut left: i32;                              // number of possible codes left of current length
        let mut offs = [0i16; MAXBITS + 1];             // offsets in symbol table for each length
        let mut length = [0i16; 256];                   // code lengths
        let mut repi = 0usize;

        // convert compact repeat counts into symbol bit length list
        symbol = 0;
        loop {
            len = rep[repi] as i16; repi += 1;
            left = (len as i32 >> 4) + 1;
            len &= 15;
            loop { length[symbol as usize] = len; symbol += 1; left -= 1; if left == 0 { break; } }
            n -= 1;
            if n == 0 { break; }
        }
        n = symbol as i32;

        // count number of codes of each length
        len = 0; while len as usize <= MAXBITS { self.count[len as usize] = 0; len += 1; }
        symbol = 0; while (symbol as i32) < n { self.count[length[symbol as usize] as usize] += 1; symbol += 1; }    // assumes lengths are within bounds
        if self.count[0] as i32 == n { return 0; }                                                                  // no codes! complete, but decode() will fail

        // check for an over-subscribed or incomplete set of lengths
        left = 1;                                                                   // one possible code of zero length
        len = 1; while len as usize <= MAXBITS {
            left <<= 1;                             // one more bit, double codes left
            left -= self.count[len as usize] as i32; // deduct count from possible codes
            if left < 0 { return left; }            // over-subscribed--return negative
            len += 1;
        }                                           // left > 0 means incomplete

        // generate offsets into symbol table for each length for sorting
        offs[1] = 0;
        len = 1; while (len as usize) < MAXBITS { offs[len as usize + 1] = offs[len as usize].wrapping_add(self.count[len as usize]); len += 1; }

        // put symbols in table sorted by length, by symbol order within each length
        symbol = 0; while (symbol as i32) < n { if length[symbol as usize] != 0 { let l = length[symbol as usize] as usize; self.symbol[offs[l] as usize] = symbol; offs[l] += 1; } symbol += 1; }

        // return zero for complete set, positive for incomplete set
        left
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    // blast.c's published test vector
    #[test]
    fn decompressTestVector() {
        let input = [0x00u8, 0x04, 0x82, 0x24, 0x25, 0x8f, 0x80, 0x7f];
        let mut os = Vec::new();
        Blast::new().decompressStream(&input, &mut os).unwrap();
        assert_eq!(os, b"AIAIAIAIAIAIA");
        let mut buf = [0u8; 13];
        Blast::new().decompress(&input, &mut buf).unwrap();
        assert_eq!(&buf, b"AIAIAIAIAIAIA");
    }
}
