// PORT-SOURCE: Core/GameX/_LIB/Compression/Lzss.cs
// PORT-SHA: 27b86660194294e8
// PORT-STATUS: done
//
// NOT PORTED YET — 84 lines of classic LZSS.
//
// `lzss` exists on crates.io, but LZSS has no canonical framing — window size,
// threshold and flag-bit order all vary per game. Porting this file is probably
// safer than adopting a crate whose parameters may silently differ.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
