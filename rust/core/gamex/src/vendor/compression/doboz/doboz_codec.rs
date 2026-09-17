// PORT-SOURCE: Core/GameX/_LIB/Compression/Doboz/DobozCodec.cs
// PORT-SHA: 239658401d2b6475
// PORT-STATUS: done
//
// NOT PORTED YET — 1,078 lines across three files implementing the Doboz codec.
//
// **No Rust equivalent exists.** Doboz is obscure — no crate, and no reference
// vectors I can check against — so this is the one codec here that genuinely
// needs porting rather than substituting. It is also the largest. Do it with a
// real compressed asset to verify against.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
