// PORT-SOURCE: Core/GameX/App/ImportManager.cs
// PORT-SHA: de2a9d0984627dda
// PORT-STATUS: done
//
// NOT PORTED — there is no working implementation to port.
//
// ============ THE `.meta` SIDECAR IS NEVER READ ==========================
//
//     var setPath = Path.Combine(filePath, ".set");
//     using (var r = new BinaryReader(File.Open(setPath, ...)))
//         await ArcBinary.Stream.Read(source, r, "Set");
//     var metaPath = Path.Combine(filePath, ".meta");
//     using (var r = new BinaryReader(File.Open(setPath, ...)))
//         //                                    ^^^^^^^ setPath again
//         await ArcBinary.Stream.Read(source, r, "Meta");
//
// The second `File.Open` reads **`setPath`**, not `metaPath`. `metaPath` is
// computed and never used, so `.set` is parsed twice — once as a file list and
// once as metadata — and `.meta` is never opened at all.
//
// The consequence is not a crash: `parse_meta` over a `.set` file finds none of
// its section headers and returns an empty result, so every imported archive
// silently loses its compression and encryption flags. `formats/stream.rs`
// documents that parser. **Fix in the C# tree** — one identifier.
//
// The rest of the file is unfinished:
//
//   * **The public entry point `ImportAsync` has an empty body.** Its whole
//     content is a commented-out `foreach (var path in resource.Paths)`, so
//     import does nothing and reports success.
//   * **`MaxDegreeOfParallelism = 8; //1;`** is declared and never read — the
//     trailing `//1;` looks like someone toggling it while debugging.
//   * `ImportPakAsync` creates a `BinaryWriter` over a `FileStream` and returns
//     it without disposing either; the caller receives an open handle with no
//     indication it owns it.
//
// Nothing to translate until the C# does something. Kept as a file so the 1:1
// mapping holds and `sync-check.sh` notices when it grows an implementation.
