// PORT-SOURCE: Core/GameX.FileSystems/Casc/Salsa20.cs
// PORT-SHA: e72b9873376631e1
// PORT-STATUS: done
//
// NOT PORTED — and this one is worse than the first refusal, because it is the
// **second** Salsa20 implementation in GameX.
//
// Two independent implementations of the same stream cipher live in this
// repository:
//
//     Core/GameX/_LIB/Compression/Salsa20.cs         168 live lines
//     Core/GameX.FileSystems/Casc/Salsa20.cs         235 live lines
//
// They are not copies — different line counts and different content. Three call
// sites choose between them by which namespace they imported:
// `Casc/KeyService.cs`, `Families/GameX.IW/Formats/FastFile.cs`, and CASC's
// own BLTE 'E' (encrypted) block path.
//
// That is the same pattern as the three disagreeing binary16 implementations in
// the OpenStack port, except here it is cryptography. If the two differ
// anywhere — a rotation constant, a counter width, the block-boundary handling
// — then whether a file decrypts correctly depends on which file the caller
// happened to `using`. Neither has tests.
//
// **Use RustCrypto's `salsa20` for both**, and delete one from the C# side.
// It is audited, fuzzed, and tested against the published ECRYPT/eSTREAM
// vectors:
//
//     use salsa20::{Salsa20, Key, Nonce};
//     use salsa20::cipher::{KeyIvInit, StreamCipher};
//     let mut c = Salsa20::new(&Key::from(key), &Nonce::from(nonce));
//     c.apply_keystream(&mut buf);
//
// **Before deleting either, diff them.** If they disagree, one of them has been
// producing wrong plaintext for whichever games route through it, and that is
// worth knowing independently of this port.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
