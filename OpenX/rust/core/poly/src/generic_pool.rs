// PORT-SOURCE: Core/OpenX.Polyfills/System.Collections.Generic/GenericPoolX.cs
// PORT-SHA: 0539af1ef504d469
// PORT-STATUS: done
//
// PARTIAL PORT: `SinglePool<T>` is not ported. Its `Release` calls `Dispose`
// on the only instance, so the next `Get` hands back a disposed object.
// `ActionAsync` and `FuncAsync` are not separate methods. C# runs `Release`
// when the task is created, not when it finishes, so the item goes back into
// the pool while the work is still using it. A caller here holds the value
// until it drops, including across `.await`.
//
// Home: `openx_poly::pool` (OpenX/Polyfills.md). This module is the type;
// `pool` re-exports it.

use std::sync::Mutex;

/// C# `GenericPoolX<T>`.
pub struct GenericPool<T> {
    items: Mutex<Vec<T>>,
    factory: Box<dyn Fn() -> T + Send + Sync>,
    reset: Option<Box<dyn Fn(&mut T) + Send + Sync>>,
    retainInPool: usize,
}

impl<T> GenericPool<T> {
    /// C# `GenericPoolX(Func<T> factory, Action<T> reset = null, int retainInPool = 10)`.
    pub fn new(factory: impl Fn() -> T + Send + Sync + 'static) -> Self {
        Self { items: Mutex::new(Vec::new()), factory: Box::new(factory), reset: None, retainInPool: 10 }
    }

    pub fn withReset(mut self, reset: impl Fn(&mut T) + Send + Sync + 'static) -> Self {
        self.reset = Some(Box::new(reset));
        self
    }

    pub fn withRetain(mut self, retainInPool: usize) -> Self {
        self.retainInPool = retainInPool;
        self
    }

    /// C# `Get()` — take one from the pool, or build a new one.
    pub fn get(&self) -> T {
        self.tryTake().unwrap_or_else(|| (self.factory)())
    }

    /// Take an idle item without calling the factory. `None` when the pool is empty.
    pub fn tryTake(&self) -> Option<T> {
        self.items.lock().unwrap_or_else(|p| p.into_inner()).pop()
    }

    /// C# `Release(T item)` — keep it if there is room, else drop it.
    pub fn release(&self, mut item: T) {
        let mut g = self.items.lock().unwrap_or_else(|p| p.into_inner());
        if g.len() < self.retainInPool {
            if let Some(r) = &self.reset { r(&mut item); }
            g.push(item);
        }
    }

    /// C# `Action(Action<T>)`.
    pub fn action(&self, action: impl FnOnce(&mut T)) {
        let mut item = self.get();
        action(&mut item);
        self.release(item);
    }

    /// C# `Func(Func<T, TResult>)`.
    pub fn func<R>(&self, func: impl FnOnce(&mut T) -> R) -> R {
        let mut item = self.get();
        let r = func(&mut item);
        self.release(item);
        r
    }

    /// How many items are idle.
    pub fn idle(&self) -> usize {
        self.items.lock().unwrap_or_else(|p| p.into_inner()).len()
    }
}

/// C# `StaticPool<T>` — one shared instance. `Release` resets it and does not dispose it.
pub struct StaticPool<T> {
    value: Mutex<T>,
    reset: Option<Box<dyn Fn(&mut T) + Send + Sync>>,
}

impl<T> StaticPool<T> {
    pub fn new(value: T) -> Self {
        Self { value: Mutex::new(value), reset: None }
    }

    pub fn withReset(mut self, reset: impl Fn(&mut T) + Send + Sync + 'static) -> Self {
        self.reset = Some(Box::new(reset));
        self
    }

    /// C# `Get` / `Release` as one critical section. The C# handed the same
    /// instance to every caller at once; a single value cannot do that.
    pub fn with<R>(&self, f: impl FnOnce(&mut T) -> R) -> R {
        let mut g = self.value.lock().unwrap_or_else(|p| p.into_inner());
        let r = f(&mut g);
        if let Some(reset) = &self.reset { reset(&mut g); }
        r
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::atomic::{AtomicUsize, Ordering};
    use std::sync::Arc;

    #[test]
    fn reusesReleasedItems() {
        let made = Arc::new(AtomicUsize::new(0));
        let m = made.clone();
        let pool = GenericPool::new(move || m.fetch_add(1, Ordering::SeqCst));
        pool.action(|_| {});
        pool.action(|_| {});
        assert_eq!(made.load(Ordering::SeqCst), 1, "the second checkout reuses the first item");
    }

    #[test]
    fn releaseReturnsTheItem() {
        let pool = GenericPool::new(Vec::<u8>::new);
        assert_eq!(pool.idle(), 0);
        pool.action(|v| v.push(1));
        assert_eq!(pool.idle(), 1);
    }

    #[test]
    fn resetRunsOnRelease() {
        let pool = GenericPool::new(|| vec![1u8, 2, 3]).withReset(|v| v.clear());
        pool.action(|_| {});
        assert!(pool.get().is_empty(), "reset clears the item before it is reused");
    }

    #[test]
    fn retainCapDropsTheRest() {
        let pool = GenericPool::new(|| 0u32).withRetain(2);
        pool.release(1);
        pool.release(2);
        pool.release(3);
        assert_eq!(pool.idle(), 2);
    }

    #[test]
    fn twoGetsAreDistinct() {
        let n = Arc::new(AtomicUsize::new(0));
        let c = n.clone();
        let pool = GenericPool::new(move || c.fetch_add(1, Ordering::SeqCst));
        let a = pool.get();
        let b = pool.get();
        assert_ne!(a, b);
    }

    #[test]
    fn staticPoolResetsAfterEachUse() {
        let p = StaticPool::new(vec![1u8]).withReset(|v| v.clear());
        p.with(|v| v.push(2));
        assert!(p.with(|v| v.clone()).is_empty());
    }
}
