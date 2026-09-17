// PORT-SOURCE: Core/GameX/_LIB/Compression/XCompress.cs
// PORT-SHA: 29a67366328fbe1f
// PORT-STATUS: done
//
// NOT PORTED — P/Invoke declarations, not logic.
//
// A thin `DllImport` binding to Microsoft's **XCompress** (Xbox LZX), with no algorithm of its own.
// Hand-translating FFI signatures buys nothing and risks undefined behaviour at
// the boundary — the same call made for the OpenAL, libogg, libchdr and LZMA
// bindings in the OpenStack port.
//
// `lzxd` is a pure-Rust LZX decompressor and covers the decode path, which is all
// this file uses — that would also drop a Windows-only native dependency.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
