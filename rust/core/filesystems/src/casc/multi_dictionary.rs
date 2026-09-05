// PORT-SOURCE: Core/GameX.FileSystems/Casc/MultiDictionary.cs
// PORT-SHA: 3bc821fd0793c20e
// PORT-STATUS: done
//
// A one-key-many-values map.
//
// ============ THE C# USES `new` HIDING, NOT `override` ===================
//
//     public class MultiDictionary<K, V> : Dictionary<K, List<V>> {
//         public void Add(K key, V value) { ... }        // hides Add(K, List<V>)
//         public new void Clear() { ... }
//     }
//
// `Add(K, V)` is an overload rather than an override — fine — but `Clear` is
// declared `new`, which **hides** rather than overrides. `Dictionary<,>.Clear`
// is not virtual, so:
//
//     MultiDictionary<string, int> m = ...;
//     m.Clear();                              // the custom Clear
//     ((Dictionary<string, List<int>>)m).Clear();   // the base Clear
//     ((IDictionary)m).Clear();                     // also the base Clear
//
// Three call styles, two behaviours. The custom `Clear` empties each inner
// list *and then* clears the outer dictionary, so the inner-list clearing only
// happens on one of the three paths. Since the lists are being discarded
// anyway, that inner loop is pointless on every path — it only matters if
// something else still holds a reference to one, in which case the two paths
// differ observably.
//
// Ported as its own type wrapping a `HashMap`, so there is no base class to
// route around.

use std::collections::HashMap;
use std::hash::Hash;

/// C# `MultiDictionary<K, V>`.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct MultiDictionary<K: Eq + Hash, V> {
    inner: HashMap<K, Vec<V>>,
}

impl<K: Eq + Hash, V> MultiDictionary<K, V> {
    pub fn new() -> Self {
        Self { inner: HashMap::new() }
    }

    /// C# `Add(K key, V value)` — appends, creating the list if absent.
    pub fn add(&mut self, key: K, value: V) {
        self.inner.entry(key).or_default().push(value);
    }

    /// C# indexer `this[K]`, which throws `KeyNotFoundException` when absent.
    pub fn get(&self, key: &K) -> Option<&[V]> {
        self.inner.get(key).map(|v| v.as_slice())
    }

    pub fn get_mut(&mut self, key: &K) -> Option<&mut Vec<V>> {
        self.inner.get_mut(key)
    }

    /// C# `TryGetValue`.
    pub fn contains_key(&self, key: &K) -> bool {
        self.inner.contains_key(key)
    }

    /// C# `Clear()` — the custom one.
    pub fn clear(&mut self) {
        self.inner.clear();
    }

    /// Number of keys, as `Dictionary.Count` gives.
    pub fn len(&self) -> usize {
        self.inner.len()
    }

    pub fn is_empty(&self) -> bool {
        self.inner.is_empty()
    }

    /// Total values across all keys. No C# equivalent, and the number a caller
    /// of a multimap usually wants — `Count` gives keys, which is easy to
    /// misread.
    pub fn value_count(&self) -> usize {
        self.inner.values().map(Vec::len).sum()
    }

    pub fn iter(&self) -> impl Iterator<Item = (&K, &[V])> {
        self.inner.iter().map(|(k, v)| (k, v.as_slice()))
    }

    pub fn keys(&self) -> impl Iterator<Item = &K> {
        self.inner.keys()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn adding_the_same_key_appends() {
        let mut m: MultiDictionary<&str, i32> = MultiDictionary::new();
        m.add("a", 1);
        m.add("a", 2);
        m.add("b", 3);
        assert_eq!(m.get(&"a"), Some(&[1, 2][..]));
        assert_eq!(m.get(&"b"), Some(&[3][..]));
    }

    #[test]
    fn a_missing_key_is_none_not_a_throw() {
        // The C# indexer raises KeyNotFoundException.
        let m: MultiDictionary<&str, i32> = MultiDictionary::new();
        assert_eq!(m.get(&"nope"), None);
        assert!(!m.contains_key(&"nope"));
    }

    #[test]
    fn len_counts_keys_and_value_count_counts_values() {
        // C# `Count` gives keys, which reads as "how many things are in here".
        let mut m: MultiDictionary<&str, i32> = MultiDictionary::new();
        m.add("a", 1);
        m.add("a", 2);
        m.add("a", 3);
        assert_eq!(m.len(), 1, "one key");
        assert_eq!(m.value_count(), 3, "three values");
    }

    #[test]
    fn clear_empties_everything_on_the_only_path_there_is() {
        // The C#'s `new Clear` is bypassed by a base-class or interface
        // reference; there is no base class here.
        let mut m: MultiDictionary<&str, i32> = MultiDictionary::new();
        m.add("a", 1);
        m.clear();
        assert!(m.is_empty());
        assert_eq!(m.value_count(), 0);
        assert_eq!(m.get(&"a"), None);
    }

    #[test]
    fn duplicate_values_under_one_key_are_kept() {
        // It is a list, not a set - the C# uses List<V>, and the local variable
        // is even named `hset`, which suggests a set was once intended.
        let mut m: MultiDictionary<&str, i32> = MultiDictionary::new();
        m.add("a", 7);
        m.add("a", 7);
        assert_eq!(m.get(&"a"), Some(&[7, 7][..]));
    }

    #[test]
    fn iteration_covers_every_key() {
        let mut m: MultiDictionary<i32, i32> = MultiDictionary::new();
        for i in 0..5 {
            m.add(i, i * 10);
        }
        assert_eq!(m.iter().count(), 5);
        let mut keys: Vec<i32> = m.keys().copied().collect();
        keys.sort_unstable();
        assert_eq!(keys, vec![0, 1, 2, 3, 4]);
    }
}
