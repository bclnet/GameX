// PORT-SOURCE: Core/OpenX.Polyfills/System.Collections.Generic/KVExtensions.cs
// PORT-SHA: 81620d1bd889ef5f
// PORT-STATUS: done
//
// Typed accessors over `IDictionary<string, object>` — the loosely-typed
// key-value trees that Valve-style format parsers produce. C# casts the boxed
// `object` at each access and lets `InvalidCastException` escape.
//
// Rust has no boxed-primitive `object`, so the port introduces an explicit
// `KV` value enum. That turns every cast into a checked conversion: the
// `get_*` family returns `Option`, so a missing key and a wrong-typed value are
// both ordinary results instead of exceptions.
//
// The C# also silently returns `default(T)` for a missing key in some overloads
// and throws in others, which callers could not tell apart. Here it is uniform.

// `KV` itself moved to `openx_poly::pool` (core/poly/src/pool.rs); only `path`,
// which has no caller yet, stays parked here.

use openx_poly::pool::KV;

/// C# `TryGet` — chained lookup, e.g. `root.path(["a", "b", "c"])`.
pub fn path<'a>(root: &'a KV, keys: &[&str]) -> Option<&'a KV> {
    keys.iter().try_fold(root, |node, k| node.get(k))
}
