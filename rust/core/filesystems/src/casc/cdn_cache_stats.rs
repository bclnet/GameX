// PORT-SOURCE: Core/GameX.FileSystems/Casc/CdnCacheStats.cs
// PORT-SHA: 440726421e88caee
// PORT-STATUS: done
//
// NOT PORTED — three public mutable statics — `timeSpentDownloading`, `numFilesOpened`, `numFilesDownloaded` — with no synchronisation, incremented from concurrent CDN downloads. `TimeSpan` is 8 bytes so `+=` on it is not even atomic on 32-bit; the counters are `int` so they tear less visibly but still race.
//
// Either `AtomicU64` in a struct the cache owns, or drop it in favour of `tracing` metrics.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
