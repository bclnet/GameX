// PORT-SOURCE: Core/GameX/_LIB/Compression/OodleLZ.cs
// PORT-SHA: d2d2332aab4d9322
// PORT-STATUS: done
//
// NOT PORTED — P/Invoke declarations, not logic.
//
// A thin `DllImport` binding to **Oodle** (`oo2core`), RAD Game Tools's proprietary compressor, with no algorithm of its own.
// Hand-translating FFI signatures buys nothing and risks undefined behaviour at
// the boundary — the same call made for the OpenAL, libogg, libchdr and LZMA
// bindings in the OpenStack port.
//
// Oodle is closed-source and license-restricted, so there is no pure-Rust
// equivalent. Bind the same DLL with `bindgen`, or gate the formats needing it
// behind a feature. Note the C# hard-codes the DLL name, so it already only
// works where a matching `oo2core_*.dll` is present.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
