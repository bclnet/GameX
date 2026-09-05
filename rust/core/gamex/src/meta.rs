// PORT-SOURCE: Core/GameX/Meta.cs
// PORT-SHA: 6f4c4365263f54a7
// PORT-STATUS: done
//
// PARTIAL PORT: `FileSource` and the `MetaInfo` / `MetaContent` tree are here.
// `MetaManager` and `Filter` depend on `Archive`, which is in
// `GameX.FileSystems` and not ported yet.
//
// ===================== FOUR C#-SIDE OBSERVATIONS =========================
//
//   1. **`FileSource` has 15 public mutable fields, four of them untyped.**
//      `Tag`, `Tag2`, `CachedObjectOption` and `MetaContent.Value`/`Tag` are
//      all `object`, so what a field holds depends on which archive produced
//      it. That is the same `object`-typed API surface flagged in the OpenStack
//      review, and it is where several of that port's bugs lived. Modelled here
//      with an explicit enum rather than `Box<dyn Any>` — a closed set is
//      checkable, and every consumer in the tree stores one of a handful of
//      shapes.
//
//   2. **`Fix()` mutates through a stored closure and returns `this`.**
//      `public FileSource Fix() { Lazy?.Invoke(this); return this; }` — a
//      lazily-populated record where nothing tracks whether `Lazy` has already
//      run, so calling `Fix()` twice invokes it twice. Whether that is
//      idempotent depends on the closure each archive installed.
//
//   3. **`EmptyAssetFactory` returns null rather than an empty result.** Every
//      caller therefore has to null-check a factory it just successfully looked
//      up.
//
//   4. **`MetaContent.Dispose` is an `IDisposable` *property*.** Storing a
//      disposable in a settable property means ownership is ambiguous — nothing
//      says whether assigning over it disposes the old one. `Drop` handles this
//      structurally in Rust, so the field disappears.

use std::time::SystemTime;

/// What an untyped `object` field in `FileSource` / `MetaContent` actually
/// holds across the tree. See observation 1.
#[derive(Debug, Clone, PartialEq)]
pub enum Tag {
    None,
    Int(i64),
    Str(String),
    Bytes(Vec<u8>),
    /// An index into an archive-specific side table.
    Handle(usize),
}

impl Default for Tag {
    fn default() -> Self {
        Self::None
    }
}

/// C# `FileSource`.
///
/// `Arc`, `Parts`, `Lazy` and the cached-factory fields are omitted: they point
/// back into `Archive`, which is not ported yet. The header of that file will
/// say so when it is.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct FileSource {
    pub id: i32,
    pub path: String,
    pub offset: i64,
    pub file_size: i64,
    pub packed_size: i64,
    /// C# `int Compressed` — a method id, not a bool. 0 means stored.
    pub compressed: i32,
    pub flags: i32,
    pub hash: u64,
    pub date: Option<SystemTime>,
    pub data: Option<Vec<u8>>,
    pub tag: Tag,
    pub tag2: Tag,
}

impl FileSource {
    /// Whether the payload is stored rather than compressed.
    #[inline]
    pub fn is_stored(&self) -> bool {
        self.compressed == 0
    }

    /// The number of bytes to read from the archive for this entry.
    ///
    /// The C# leaves callers to decide between `PackedSize` and `FileSize`,
    /// which is a per-archive convention that is easy to get backwards.
    #[inline]
    pub fn read_size(&self) -> i64 {
        if self.is_stored() || self.packed_size == 0 {
            self.file_size
        } else {
            self.packed_size
        }
    }

    /// Whether this entry's extent lies inside an archive of `len` bytes.
    ///
    /// No C# equivalent — `Offset` and the sizes are read straight from a file
    /// header and used unchecked, so a corrupt entry seeks anywhere.
    pub fn fits_within(&self, len: i64) -> bool {
        self.offset >= 0
            && self.read_size() >= 0
            && self.offset.checked_add(self.read_size()).map(|e| e <= len).unwrap_or(false)
    }
}

/// C# `MetaContent`.
///
/// The `IDisposable Dispose` property is dropped — see observation 4.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct MetaContent {
    pub type_: String,
    pub name: String,
    pub value: Tag,
    pub tag: Tag,
    pub max_width: i32,
    pub max_height: i32,
    /// C# `Type EngineType` — a reflection handle. A registry name here, for
    /// the same reason `Family.cs` uses one.
    pub engine_type: Option<String>,
}

/// C# `MetaInfo(string name, object tag, IEnumerable<MetaInfo> items, bool clickable)`.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct MetaInfo {
    pub name: String,
    pub tag: Tag,
    pub items: Vec<MetaInfo>,
    pub clickable: bool,
}

impl MetaInfo {
    pub fn new(name: impl Into<String>) -> Self {
        Self { name: name.into(), ..Default::default() }
    }

    pub fn with_tag(mut self, tag: Tag) -> Self {
        self.tag = tag;
        self
    }

    pub fn with_items(mut self, items: Vec<MetaInfo>) -> Self {
        self.items = items;
        self
    }

    pub fn clickable(mut self) -> Self {
        self.clickable = true;
        self
    }

    /// Total nodes in this subtree, including self.
    ///
    /// Iterative rather than recursive: these trees are built from file
    /// metadata, and the C# walks them recursively with no depth bound, so a
    /// deeply nested archive can overflow the stack.
    pub fn node_count(&self) -> usize {
        let mut n = 0;
        let mut stack = vec![self];
        while let Some(cur) = stack.pop() {
            n += 1;
            stack.extend(cur.items.iter());
        }
        n
    }

    /// Depth of this subtree.
    pub fn depth(&self) -> usize {
        let mut max = 0;
        let mut stack = vec![(self, 1usize)];
        while let Some((cur, d)) = stack.pop() {
            max = max.max(d);
            stack.extend(cur.items.iter().map(|c| (c, d + 1)));
        }
        max
    }
}

/// C# `interface IHaveMetaInfo`.
pub trait HaveMetaInfo {
    /// C# `GetInfoNodes(MetaManager, FileSource, object tag)`.
    ///
    /// The `MetaManager` and `FileSource` parameters are optional in the C#
    /// (both default to null), so most implementors ignore them.
    fn info_nodes(&self, file: Option<&FileSource>, tag: &Tag) -> Vec<MetaInfo>;
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn stored_entries_read_their_file_size() {
        let f = FileSource { compressed: 0, file_size: 100, packed_size: 40, ..Default::default() };
        assert!(f.is_stored());
        assert_eq!(f.read_size(), 100);
    }

    #[test]
    fn compressed_entries_read_their_packed_size() {
        let f = FileSource { compressed: 8, file_size: 100, packed_size: 40, ..Default::default() };
        assert!(!f.is_stored());
        assert_eq!(f.read_size(), 40);
    }

    #[test]
    fn a_compressed_entry_with_no_packed_size_falls_back() {
        // Several archives leave PackedSize at 0 and mean "same as FileSize".
        let f = FileSource { compressed: 8, file_size: 100, packed_size: 0, ..Default::default() };
        assert_eq!(f.read_size(), 100);
    }

    #[test]
    fn extents_are_bounds_checked_against_the_archive() {
        // The C# uses Offset and the sizes straight from the header.
        let f = FileSource { offset: 90, file_size: 10, ..Default::default() };
        assert!(f.fits_within(100));
        assert!(!f.fits_within(99));
        let neg = FileSource { offset: -1, file_size: 1, ..Default::default() };
        assert!(!neg.fits_within(100));
    }

    #[test]
    fn an_overflowing_extent_is_rejected_rather_than_wrapping() {
        let f = FileSource { offset: i64::MAX, file_size: 1, ..Default::default() };
        assert!(!f.fits_within(i64::MAX));
    }

    #[test]
    fn meta_trees_build_and_count() {
        let t = MetaInfo::new("root").with_items(vec![
            MetaInfo::new("a").with_items(vec![MetaInfo::new("a1")]),
            MetaInfo::new("b").clickable(),
        ]);
        assert_eq!(t.node_count(), 4);
        assert_eq!(t.depth(), 3);
        assert!(t.items[1].clickable);
        assert!(!t.items[0].clickable);
    }

    #[test]
    fn deep_trees_do_not_overflow_the_stack() {
        // The C# walks these recursively with no depth bound.
        let mut node = MetaInfo::new("leaf");
        for i in 0..50_000 {
            node = MetaInfo::new(format!("n{i}")).with_items(vec![node]);
        }
        assert_eq!(node.depth(), 50_001);
        assert_eq!(node.node_count(), 50_001);
    }

    #[test]
    fn tags_default_to_none() {
        assert_eq!(FileSource::default().tag, Tag::None);
        assert_eq!(MetaContent::default().value, Tag::None);
    }

    #[test]
    fn builders_compose() {
        let m = MetaInfo::new("x").with_tag(Tag::Int(7)).clickable();
        assert_eq!(m.tag, Tag::Int(7));
        assert!(m.clickable);
        assert!(m.items.is_empty());
    }
}
