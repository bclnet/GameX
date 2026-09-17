// PORT-SOURCE: Core/GameX.FileSystems/Casc/BackgroundWorkerEx.cs
// PORT-SHA: 26c7cb5602d947b8
// PORT-STATUS: done
//
// NOT PORTED — a `BackgroundWorker` subclass. Two things about it are worth carrying forward rather than translating. It declares `public new void ReportProgress(int)` — `new` hiding over a non-virtual method, the same pattern as `MultiDictionary`, so a base-class reference silently gets different behaviour. And that method **throws `OperationCanceledException` from a progress report** when `CancellationPending` is set, making cancellation a side effect of reporting progress. It also only reports when `percentProgress > lastProgressPercentage`, so progress can never go backwards and a restarted task reports nothing.
//
// The Rust shape is a channel or a callback plus a `CancellationToken`-style flag the worker polls — cancellation checked where it happens, not raised out of an unrelated call.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
