// PORT-SOURCE: Core/GameX.FileSystems/Casc/Logger.cs
// PORT-SHA: 2470ebc0f3b8a337
// PORT-STATUS: done
//
// NOT PORTED — three static fields (FileStream, StreamWriter, ILoggerOptions) with no synchronisation. `Init` opens the log with `FileMode.Create`, which truncates, and calling it twice leaks the first `FileStream` — nothing disposes the old one. Every `WriteLine` goes through the shared static writer from whatever thread reaches it.
//
// Rust logging is a solved problem: `log` for the facade plus `env_logger`/`tracing` for the sink. That also gives per-module filtering, which this cannot do at all.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
