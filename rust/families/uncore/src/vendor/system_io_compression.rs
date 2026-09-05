// PORT-SOURCE: Families/GameX.Uncore/_LIB/System.IO.Compression.cs
// PORT-SHA: 076ad7e94d38e013
// PORT-STATUS: done
//
// NOT PORTED — 772 lines wrapping third-party libraries, not implementing
// anything.
//
// Its `using` block is the whole story:
//
//     ICSharpCode.SharpZipLib.Zip
//     Org.BouncyCastle.Asn1{,.Pkcs,.X509,.X9}
//     Org.BouncyCastle.Crypto{,.Digests,.Encodings,.Engines,.Modes,.Parameters}
//     Org.BouncyCastle.Security
//     Org.BouncyCastle.Utilities{,.Bzip2}
//
// So it is glue over SharpZipLib (Zip) and BouncyCastle (ASN.1, RSA, AES,
// digests, BZip2). Rust equivalents, all maintained:
//
//   * Zip            -> `zip`
//   * BZip2          -> `bzip2` or `bzip2-rs` (pure Rust)
//   * ASN.1 / PKCS   -> `der`, `pkcs1`, `pkcs8`, `spki` (RustCrypto)
//   * RSA            -> `rsa`
//   * AES + modes    -> `aes`, `cbc`, `ctr` (RustCrypto)
//   * Digests        -> `sha1`, `sha2`, `md-5`
//
// Porting this by hand would mean reimplementing BouncyCastle's crypto in the
// process, which is the thing I have consistently refused to do blind — and
// here there is no reason to, since the substitutions are direct.
//
// What *does* need porting is whatever GameX-specific sequencing this file adds
// on top (which archive layout maps to which cipher and key). That is a small
// amount of real logic buried in 772 lines of plumbing, and it should be
// extracted once the crates above are wired up.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
