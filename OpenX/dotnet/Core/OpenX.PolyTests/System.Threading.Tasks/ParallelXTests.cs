using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks;

[TestClass]
public class ParallelXTests {
    [TestMethod]
    public async Task ForAsync_RunsEveryIndexAndWaitsForThem() {
        var seen = new ConcurrentBag<int>();
        await ParallelX.ForAsync(3, 13, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (i, ct) => {
            await Task.Delay(5, ct);
            seen.Add(i);
        });
        CollectionAssert.AreEquivalent(Enumerable.Range(3, 10).ToArray(), seen.ToArray(), "every index ran, and the call did not return until they had");
    }

    [TestMethod]
    public async Task ForAsync_EmptyAndReversedRangesComplete() {
        var n = 0;
        await ParallelX.ForAsync(5, 5, (i, ct) => { n++; return default; });
        await ParallelX.ForAsync(5, 2, (i, ct) => { n++; return default; });
        Assert.AreEqual(0, n);
    }

    [TestMethod]
    public async Task ForEachAsync_HonoursMaxDegreeOfParallelism() {
        int running = 0, peak = 0;
        await ParallelX.ForEachAsync(Enumerable.Range(0, 40), new ParallelOptions { MaxDegreeOfParallelism = 3 }, async (i, ct) => {
            var now = Interlocked.Increment(ref running);
            InterlockedMax(ref peak, now);
            await Task.Delay(2, ct);
            Interlocked.Decrement(ref running);
        });
        Assert.IsTrue(peak <= 3, $"peak {peak}");
        Assert.IsTrue(peak >= 2, $"peak {peak}: work should overlap");
    }

    [TestMethod]
    public async Task ForEachAsync_DegreeOneRunsInOrder() {
        var order = new List<int>();
        await ParallelX.ForEachAsync(Enumerable.Range(0, 20), new ParallelOptions { MaxDegreeOfParallelism = 1 }, async (i, ct) => {
            await Task.Yield();
            order.Add(i);
        });
        CollectionAssert.AreEqual(Enumerable.Range(0, 20).ToArray(), order.ToArray());
    }

    [TestMethod]
    public async Task ForEachAsync_FailureStopsNewWorkAndSurfacesTheException() {
        var started = 0;
        var task = ParallelX.ForEachAsync(Enumerable.Range(0, 1000), new ParallelOptions { MaxDegreeOfParallelism = 2 }, async (i, ct) => {
            Interlocked.Increment(ref started);
            await Task.Delay(1, ct);
            if (i == 5) throw new InvalidOperationException("boom");
        });
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => task);
        Assert.AreEqual("boom", ex.Message);
        Assert.IsTrue(started < 1000, $"started {started}: a failure must stop new items");
        Assert.IsTrue(task.IsFaulted);
        Assert.AreEqual(1, task.Exception.InnerExceptions.Count, "cancellations caused by the failure are not reported");
    }

    [TestMethod]
    public async Task ForEachAsync_KeepsEveryConcurrentFailure() {
        using var barrier = new Barrier(2);
        var task = ParallelX.ForEachAsync(new[] { 1, 2 }, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (i, ct) => {
            barrier.SignalAndWait(); // both are running before either throws
            throw new InvalidOperationException($"fail {i}");
        });
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => task);
        Assert.AreEqual(2, task.Exception.InnerExceptions.Count);
    }

    [TestMethod]
    public async Task ForEachAsync_SynchronousThrowIsCaught() {
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => ParallelX.ForEachAsync(new[] { 1 }, (i, ct) => throw new ArgumentException("sync")));
    }

    [TestMethod]
    public async Task ForEachAsync_OuterCancellationCancelsTheTask() {
        using var cts = new CancellationTokenSource();
        var started = 0;
        var task = ParallelX.ForEachAsync(Enumerable.Range(0, 1000), new ParallelOptions { MaxDegreeOfParallelism = 1, CancellationToken = cts.Token }, async (i, ct) => {
            if (Interlocked.Increment(ref started) == 3) cts.Cancel();
            await Task.Yield();
        });
        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
        Assert.IsTrue(task.IsCanceled);
        Assert.AreEqual(3, started);
    }

    [TestMethod]
    public async Task ForEachAsync_AlreadyCancelledNeverRunsTheBody() {
        var n = 0;
        var task = ParallelX.ForEachAsync(new[] { 1, 2, 3 }, new CancellationToken(true), (i, ct) => { n++; return default; });
        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
        Assert.AreEqual(0, n);
    }

    [TestMethod]
    public async Task ForEachAsync_SourceThatThrowsFaultsTheTask() {
        static IEnumerable<int> Bad() { yield return 1; throw new FormatException("bad source"); }
        var seen = new ConcurrentBag<int>();
        await Assert.ThrowsExactlyAsync<FormatException>(() => ParallelX.ForEachAsync(Bad(), (i, ct) => { seen.Add(i); return default; }));
        CollectionAssert.AreEqual(new[] { 1 }, seen.ToArray());
    }

    [TestMethod]
    public void ForEachAsync_ValidatesArguments() {
        Assert.ThrowsExactly<ArgumentNullException>(() => ParallelX.ForEachAsync<int>(null, (i, ct) => default));
        Assert.ThrowsExactly<ArgumentNullException>(() => ParallelX.ForEachAsync(new[] { 1 }, (ParallelOptions)null, (i, ct) => default));
        Assert.ThrowsExactly<ArgumentNullException>(() => ParallelX.ForEachAsync(new[] { 1 }, null));
    }

    [TestMethod]
    public async Task ForEachAsync_MatchesTheFrameworkOnTheSameWork() {
        // the polyfill and .NET's own Parallel.ForEachAsync should agree on results and on the failure they surface
        async Task<(int[] seen, string error)> Run(Func<IEnumerable<int>, ParallelOptions, Func<int, CancellationToken, ValueTask>, Task> impl) {
            var seen = new ConcurrentBag<int>();
            string error = null;
            try {
                await impl(Enumerable.Range(0, 50), new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (i, ct) => {
                    await Task.Delay(1, ct);
                    if (i == 49) throw new InvalidOperationException("last");
                    seen.Add(i);
                });
            }
            catch (Exception ex) { error = $"{ex.GetType().Name}:{ex.Message}"; }
            return (seen.OrderBy(x => x).ToArray(), error);
        }
        var mine = await Run(ParallelX.ForEachAsync);
        var theirs = await Run(Parallel.ForEachAsync);
        Assert.AreEqual(theirs.error, mine.error);
        CollectionAssert.IsSubsetOf(mine.seen, Enumerable.Range(0, 49).ToArray());
    }

    static void InterlockedMax(ref int target, int value) {
        int current;
        while ((current = Volatile.Read(ref target)) < value && Interlocked.CompareExchange(ref target, value, current) != current) { }
    }
}
