// PORT-SOURCE: Core/GameX/_LIB/Compression/Salsa20.cs
// PORT-SHA: e78b3e1000ac0577
// PORT-STATUS: done
//
// NOT PORTED — hand-writing a stream cipher is the one thing I will not do
// blind.
//
// Salsa20 is 168 lines here, and it is exactly the shape of code where a wrong
// rotation constant or a mis-ordered quarter-round produces a keystream that
// looks random, encrypts and decrypts self-consistently, and is
// cryptographically worthless. A round-trip test catches none of that. Same
// judgement as `AsnKeyParser.cs` and the NCCH crypto in the OpenStack port.
//
// Use RustCrypto's `salsa20` crate — audited, fuzzed, and tested against the
// published ECRYPT/eSTREAM vectors:
//
//     use salsa20::{Salsa20, Key, Nonce};
//     use salsa20::cipher::{KeyIvInit, StreamCipher};
//     let mut c = Salsa20::new(&Key::from(key), &Nonce::from(nonce));
//     c.apply_keystream(&mut buf);
//
// Worth noting separately: this is the only cryptography in GameX, and it
// decrypts game assets rather than protecting anything — so the risk here is
// corrupt output, not a security hole. That makes the substitution easy rather
// than optional.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
