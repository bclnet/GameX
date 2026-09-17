// PORT-SOURCE: Families/GameX.Uncore/_LIB/System.Security.Cryptography.cs
// PORT-SHA: f1fcd7910af79892
// PORT-STATUS: done
//
// NOT PORTED — 24 lines implementing XTEA decryption.
//
// XTEA is small enough that porting it is tempting, and it has published test
// vectors, so unlike Salsa20 it *could* be verified. But the same rule applies:
// a block cipher written without running a single vector is not something to
// ship. Use the `xtea` crate, or port it and check against the reference
// vectors before trusting it.
//
// Two things about the C# worth carrying over either way:
//
//   * **Only decryption exists.** There is no `Encrypt`, so this is read-only
//     asset decryption. Fine, but it means a round-trip test is impossible and
//     the vectors are the only check available.
//   * **A trailing partial block is silently skipped.** The loop condition is
//     `i + 8 <= offset + count`, so 1..7 leftover bytes are left in plaintext
//     with no indication. If a format ever relies on that, it is load-bearing
//     behaviour rather than an oversight, and needs preserving deliberately.
//
// The constants are standard XTEA: delta `0x9E3779B9`, 32 rounds,
// `sum = delta * rounds` for the decrypt direction.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
