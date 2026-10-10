using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks;

/// <summary>
/// ParallelX - Parallel.ForEachAsync (.NET 6) and Parallel.ForAsync (.NET 8) for netstandard2.1.
/// Same signatures and behaviour, so a call site moves to the framework by renaming ParallelX to Parallel.
/// </summary>
/// <remarks>
/// Use these instead of Parallel.For/ForEach with an async lambda: Parallel.For takes an Action, so an async
/// lambda becomes async void - Parallel.For returns at each body's first await, does not wait for the work,
/// does not limit how much runs at once, and loses its exceptions.
/// </remarks>
public static class ParallelX {
    #region ForEachAsync

    /// <summary>
    /// Runs body for each item, at most Environment.ProcessorCount at a time, and completes when all have finished.
    /// </summary>
    public static Task ForEachAsync<TSource>(IEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask> body)
        => ForEachAsync(source, new ParallelOptions(), body);

    /// <summary>
    /// Runs body for each item, at most Environment.ProcessorCount at a time, and completes when all have finished.
    /// </summary>
    public static Task ForEachAsync<TSource>(IEnumerable<TSource> source, CancellationToken cancellationToken, Func<TSource, CancellationToken, ValueTask> body)
        => ForEachAsync(source, new ParallelOptions { CancellationToken = cancellationToken }, body);

    /// <summary>
    /// Runs body for each item, at most parallelOptions.MaxDegreeOfParallelism at a time (-1 means Environment.ProcessorCount),
    /// and completes when all have finished. The first failure stops new items from starting; the returned task then
    /// faults with every exception thrown. Cancelling parallelOptions.CancellationToken stops new items and cancels the task.
    /// </summary>
    public static Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TSource, CancellationToken, ValueTask> body) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (parallelOptions == null) throw new ArgumentNullException(nameof(parallelOptions));
        if (body == null) throw new ArgumentNullException(nameof(body));
        var outer = parallelOptions.CancellationToken;
        if (outer.IsCancellationRequested) return Task.FromCanceled(outer);
        var dop = parallelOptions.MaxDegreeOfParallelism == -1 ? Environment.ProcessorCount : parallelOptions.MaxDegreeOfParallelism;

        CancellationTokenSource cts; IEnumerator<TSource> e;
        try { e = source.GetEnumerator(); }
        catch (Exception ex) { return Task.FromException(ex); }
        cts = CancellationTokenSource.CreateLinkedTokenSource(outer);
        var gate = new object();
        var errors = new ConcurrentQueue<Exception>();
        var done = false;

        // the next item, or false when the source is exhausted, has failed, or work was cancelled
        bool TryNext(out TSource item) {
            lock (gate) {
                if (!done && !cts.IsCancellationRequested) {
                    try { if (e.MoveNext()) { item = e.Current; return true; } }
                    catch (Exception ex) { errors.Enqueue(ex); cts.Cancel(); }
                }
                done = true; item = default; return false;
            }
        }

        // each worker pulls items until there are none left; a failure stops every worker from starting more
        async Task Worker() {
            while (TryNext(out var item))
                try { await body(item, cts.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) when (cts.IsCancellationRequested && !outer.IsCancellationRequested && !errors.IsEmpty) { return; } // caused by another worker's failure
                catch (Exception ex) { errors.Enqueue(ex); cts.Cancel(); return; }
        }

        var scheduler = parallelOptions.TaskScheduler ?? TaskScheduler.Default;
        var workers = new Task[dop];
        for (var i = 0; i < dop; i++) workers[i] = Task.Factory.StartNew(Worker, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task.WhenAll(workers).ContinueWith(_ => {
            lock (gate) e.Dispose();
            cts.Dispose();
            if (!errors.IsEmpty) tcs.TrySetException(errors);
            else if (outer.IsCancellationRequested) tcs.TrySetCanceled(outer);
            else tcs.TrySetResult(true);
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        return tcs.Task;
    }

    #endregion

    #region ForAsync

    /// <summary>
    /// Runs body for each index in [fromInclusive, toExclusive), at most Environment.ProcessorCount at a time.
    /// </summary>
    public static Task ForAsync(int fromInclusive, int toExclusive, Func<int, CancellationToken, ValueTask> body)
        => ForAsync(fromInclusive, toExclusive, new ParallelOptions(), body);

    /// <summary>
    /// Runs body for each index in [fromInclusive, toExclusive), at most Environment.ProcessorCount at a time.
    /// </summary>
    public static Task ForAsync(int fromInclusive, int toExclusive, CancellationToken cancellationToken, Func<int, CancellationToken, ValueTask> body)
        => ForAsync(fromInclusive, toExclusive, new ParallelOptions { CancellationToken = cancellationToken }, body);

    /// <summary>
    /// Runs body for each index in [fromInclusive, toExclusive), with ForEachAsync's limits, failure and cancellation behaviour.
    /// An empty or reversed range completes immediately.
    /// </summary>
    public static Task ForAsync(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Func<int, CancellationToken, ValueTask> body) {
        if (parallelOptions == null) throw new ArgumentNullException(nameof(parallelOptions));
        if (body == null) throw new ArgumentNullException(nameof(body));
        return ForEachAsync(fromInclusive < toExclusive ? Enumerable.Range(fromInclusive, toExclusive - fromInclusive) : Enumerable.Empty<int>(), parallelOptions, body);
    }

    #endregion
}
