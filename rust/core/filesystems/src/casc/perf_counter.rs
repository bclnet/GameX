// PORT-SOURCE: Core/GameX.FileSystems/Casc/PerfCounter.cs
// PORT-SHA: a1e925578336ad2a
// PORT-STATUS: done
//
// NOT PORTED — a `Stopwatch` wrapper whose `Dispose` prints the elapsed time. It writes to **both** `Console.WriteLine` and `Logger.WriteLine`, so every measurement appears twice, and it is unconditional — a release build prints timing for every counter.
//
// The Rust equivalent is `tracing`'s spans, which give the same nesting without the duplicate output or the `IDisposable`-as-scope-guard idiom (`Drop` covers that natively).
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
