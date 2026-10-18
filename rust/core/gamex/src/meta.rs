// PORT-SOURCE: Core/GameX/Meta.cs
// PORT-SHA: 6f4c4365263f54a7
// PORT-STATUS: done
//
// C# `object` fields (`Tag`, `Source`, `Icon`, `MetaContent.Value`, an archive
// path) are the `Tag` enum: the scalar shapes the tree stores, the GameX types
// it stores by reference, and `Any` for the rest. Types that are classes in C#
// are shared through `Arc`, so equality on them is reference equality, as in C#.
//
// `FileSource.Fix()` mutates the shared entry in C#; files are `Arc`s here, so
// `fix` returns the fixed entry (a copy when `Lazy` had to run).

use std::any::Any;
use std::collections::HashMap;
use std::fmt;
use std::io::{self, Cursor, Read, Seek, SeekFrom};
use std::sync::{Arc, Mutex, OnceLock};
use std::time::SystemTime;

use crate::binary::{Archive, ArchiveExt, AssetFactory, BinaryArchive, Stream, Task};

// #region Tag

/// C# `object`.
#[derive(Clone, Default)]
pub enum Tag {
    #[default]
    None,
    Bool(bool),
    Int(i64),
    Str(String),
    /// C# `string[]`.
    Strs(Vec<String>),
    Bytes(Vec<u8>),
    /// C# `Dictionary<string, object>`.
    Map(indexmap::IndexMap<String, Tag>),
    File(Arc<FileSource>),
    Archive(Arc<dyn Archive>),
    MetaContent(Arc<MetaContent>),
    /// An object implementing C# `IHaveMetaInfo`.
    Info(Arc<dyn IHaveMetaInfo>),
    /// An object implementing OpenX's `ITexture`, as a `MetaContent` value for the engine view.
    Texture(Arc<dyn openx_gfx::gfx::ITexture>),
    Stream(Arc<Mutex<Stream>>),
    Any(Arc<dyn Any + Send + Sync>),
}

impl Tag {
    pub fn isNone(&self) -> bool { matches!(self, Tag::None) }
    pub fn asStr(&self) -> Option<&str> { if let Tag::Str(z) = self { Some(z) } else { None } }
    pub fn asInt(&self) -> Option<i64> { if let Tag::Int(z) = self { Some(*z) } else { None } }
    pub fn asBytes(&self) -> Option<&[u8]> { if let Tag::Bytes(z) = self { Some(z) } else { None } }
    pub fn asFileSource(&self) -> Option<&Arc<FileSource>> { if let Tag::File(z) = self { Some(z) } else { None } }
    pub fn asArchive(&self) -> Option<&Arc<dyn Archive>> { if let Tag::Archive(z) = self { Some(z) } else { None } }
    pub fn asMetaContent(&self) -> Option<&Arc<MetaContent>> { if let Tag::MetaContent(z) = self { Some(z) } else { None } }
    pub fn asAny(&self) -> Option<&Arc<dyn Any + Send + Sync>> { if let Tag::Any(z) = self { Some(z) } else { None } }
    pub fn stream(s: Stream) -> Tag { Tag::Stream(Arc::new(Mutex::new(s))) }
}

impl PartialEq for Tag {
    fn eq(&self, other: &Self) -> bool {
        match (self, other) {
            (Tag::None, Tag::None) => true,
            (Tag::Bool(a), Tag::Bool(b)) => a == b,
            (Tag::Int(a), Tag::Int(b)) => a == b,
            (Tag::Str(a), Tag::Str(b)) => a == b,
            (Tag::Strs(a), Tag::Strs(b)) => a == b,
            (Tag::Bytes(a), Tag::Bytes(b)) => a == b,
            (Tag::Map(a), Tag::Map(b)) => a == b,
            (Tag::File(a), Tag::File(b)) => Arc::ptr_eq(a, b),
            (Tag::Archive(a), Tag::Archive(b)) => Arc::ptr_eq(a, b),
            (Tag::MetaContent(a), Tag::MetaContent(b)) => Arc::ptr_eq(a, b),
            (Tag::Info(a), Tag::Info(b)) => Arc::ptr_eq(a, b),
            (Tag::Texture(a), Tag::Texture(b)) => Arc::ptr_eq(a, b),
            (Tag::Stream(a), Tag::Stream(b)) => Arc::ptr_eq(a, b),
            (Tag::Any(a), Tag::Any(b)) => Arc::ptr_eq(a, b),
            _ => false,
        }
    }
}

impl fmt::Debug for Tag {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Tag::None => write!(f, "null"),
            Tag::Bool(z) => write!(f, "{z}"),
            Tag::Int(z) => write!(f, "{z}"),
            Tag::Str(z) => write!(f, "{z:?}"),
            Tag::Strs(z) => write!(f, "{z:?}"),
            Tag::Bytes(z) => write!(f, "byte[{}]", z.len()),
            Tag::Map(z) => write!(f, "{z:?}"),
            Tag::File(z) => write!(f, "FileSource({})", z.path),
            Tag::Archive(z) => write!(f, "Archive({})", z.base().name),
            Tag::MetaContent(z) => write!(f, "{z:?}"),
            Tag::Info(_) => write!(f, "IHaveMetaInfo"),
            Tag::Texture(_) => write!(f, "ITexture"),
            Tag::Stream(_) => write!(f, "Stream"),
            Tag::Any(_) => write!(f, "object"),
        }
    }
}

impl From<&str> for Tag { fn from(s: &str) -> Self { Tag::Str(s.to_string()) } }
impl From<String> for Tag { fn from(s: String) -> Self { Tag::Str(s) } }
impl From<i64> for Tag { fn from(s: i64) -> Self { Tag::Int(s) } }
impl From<Arc<FileSource>> for Tag { fn from(s: Arc<FileSource>) -> Self { Tag::File(s) } }

// #region FileSource

/// C# `FileSource.Lazy`.
pub type FileSourceLazy = Arc<dyn Fn(&mut FileSource) + Send + Sync>;

#[derive(Clone, Default)]
pub struct FileSource {
    // common
    pub id: i32,
    pub path: String,
    pub offset: i64,
    pub fileSize: i64,
    pub packedSize: i64,
    pub compressed: i32,
    pub flags: i32,
    pub hash: u64,
    /// C# `DateTime`; `None` is `default(DateTime)`.
    pub date: Option<SystemTime>,
    pub arc: Option<Arc<BinaryArchive>>,
    pub parts: Option<Vec<Arc<FileSource>>>,
    pub data: Option<Vec<u8>>,
    pub tag: Tag,
    pub tag2: Tag,
    // lazy
    pub lazy: Option<FileSourceLazy>,
    // cached
    /// C# `internal`; `pub` so family crates can build a `FileSource` with `..Default::default()`.
    pub cachedObjectFactory: OnceLock<Option<AssetFactory>>,
    pub cachedObjectOption: OnceLock<Tag>,
}

impl FileSource {
    /// C# `Fix()`: runs `Lazy`, which clears itself.
    pub fn fix(self: &Arc<Self>) -> Arc<FileSource> {
        let Some(lazy) = self.lazy.clone() else { return self.clone() };
        let mut z = (**self).clone();
        lazy(&mut z);
        Arc::new(z)
    }
}

impl fmt::Debug for FileSource {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.path) }
}

// #region Meta

#[derive(Debug, Clone, Default)]
pub struct MetaContent {
    pub type_: String,
    pub name: String,
    pub value: Tag,
    pub tag: Tag,
    pub maxWidth: i32,
    pub maxHeight: i32,
    /// C# `IDisposable Dispose`: held until the content is dropped.
    pub dispose: Tag,
    /// C# `Type EngineType`, by name.
    pub engineType: Option<String>,
}

pub trait IHaveMetaInfo: Any + Send + Sync {
    fn getInfoNodes(&self, resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, tag: &Tag) -> Vec<MetaInfo>;
}

#[derive(Debug, Clone, Default, PartialEq)]
pub struct MetaInfo {
    pub name: Option<String>,
    pub tag: Tag,
    pub items: Vec<MetaInfo>,
    pub clickable: bool,
}

impl MetaInfo {
    pub fn new(name: Option<String>, tag: Tag, items: Vec<MetaInfo>, clickable: bool) -> Self { Self { name, tag, items, clickable } }

    pub fn wrapWithGroup<T>(source: &[T], groupName: &str, body: Vec<MetaInfo>) -> Option<MetaInfo> {
        if source.is_empty() { None }
        else if source.len() == 1 { body.into_iter().next() }
        else { Some(MetaInfo::new(Some(groupName.to_string()), Tag::None, body, false)) }
    }
}

/// C# `MetaItem.Filter`.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct MetaItemFilter {
    pub name: String,
    pub description: String,
}

impl MetaItemFilter {
    pub fn new(name: &str, description: &str) -> Self { Self { name: name.to_string(), description: description.to_string() } }
}

impl fmt::Display for MetaItemFilter {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.name) }
}

#[derive(Clone, Default)]
pub struct MetaItem {
    pub source: Tag,
    pub name: String,
    pub icon: Tag,
    pub tag: Tag,
    pub archive: Option<Arc<dyn Archive>>,
    pub items: Vec<MetaItem>,
}

impl fmt::Debug for MetaItem {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}, items: {}", self.name, self.items.len()) }
}

impl PartialEq for MetaItem {
    fn eq(&self, other: &Self) -> bool {
        self.source == other.source && self.name == other.name && self.tag == other.tag && self.items == other.items
            && match (&self.archive, &other.archive) { (None, None) => true, (Some(a), Some(b)) => Arc::ptr_eq(a, b), _ => false }
    }
}

impl MetaItem {
    pub fn new(source: Tag, name: &str, icon: Tag, tag: Tag, archive: Option<Arc<dyn Archive>>, items: Option<Vec<MetaItem>>) -> Self {
        Self { source, name: name.to_string(), icon, tag, archive, items: items.unwrap_or_default() }
    }

    pub fn path(&self) -> Option<String> { self.source.asFileSource().map(|z| format!("Morrowind.bsa:{}", z.path)) }

    pub fn search(&self, predicate: &dyn Fn(&MetaItem) -> bool) -> Option<MetaItem> {
        // if node is a leaf
        if self.items.is_empty() { if predicate(self) { Some(self.clone()) } else { None } }
        // otherwise if node is not a leaf
        else {
            let results: Vec<MetaItem> = self.items.iter().filter_map(|i| i.search(predicate)).collect();
            if !results.is_empty() {
                let mut result = self.clone();
                result.items = results;
                return Some(result);
            }
            None
        }
    }

    pub fn findByPath(&mut self, path: &str, manager: &dyn MetaManager) -> Option<&mut MetaItem> {
        Self::findByPathForNodes(&mut self.items, path, manager)
    }

    pub fn findByPathForNodes<'a>(nodes: &'a mut Vec<MetaItem>, path: &str, manager: &dyn MetaManager) -> Option<&'a mut MetaItem> {
        let paths: Vec<&str> = path.splitn(2, ['\\', '/', ':']).collect();
        let node = nodes.iter_mut().find(|x| x.name == paths[0])?;
        if let Some(arc) = node.source.asFileSource().and_then(|s| s.arc.clone()) { let _ = arc.open(Some(&mut node.items), Some(manager)); }
        if paths.len() == 1 { Some(node) } else { node.findByPath(paths[1], manager) }
    }
}

pub trait MetaManager: Send + Sync {
    fn folderIcon(&self) -> Tag;
    fn packageIcon(&self) -> Tag;
    fn getIcon(&self, name: &str) -> Tag;
    fn getImage(&self, name: &str) -> Tag;
}

/// C# `MetaManager`'s statics.
impl dyn MetaManager {
    /// Gets the string or bytes.
    pub fn guessStringOrBytes(stream: &mut Stream) -> io::Result<Tag> {
        let mut bytes = Vec::new();
        stream.seek(SeekFrom::Start(0))?;
        stream.read_to_end(&mut bytes)?;
        Ok(if !bytes.contains(&0x00) { Tag::Str(String::from_utf8_lossy(&bytes).into_owned()) } else { Tag::Bytes(bytes) })
    }

    /// Gets the explorer information nodes.
    pub fn getMetaInfos<'a>(manager: &'a dyn MetaManager, archive: &'a BinaryArchive, file: &'a Arc<FileSource>) -> Task<'a, Option<Vec<MetaInfo>>> {
        Box::pin(async move {
            let mut nodes: Option<Vec<MetaInfo>> = None;
            let obj = archive.getAsset(&Tag::File(file.clone()), &Tag::None, true).await?;
            match &obj {
                Tag::None => return Ok(None),
                Tag::Info(info) => nodes = Some(info.getInfoNodes(Some(manager), Some(file), &Tag::None)),
                Tag::Stream(stream) => {
                    let value = Self::guessStringOrBytes(&mut stream.lock().unwrap())?;
                    let fileName = fileName(&file.path);
                    nodes = Some(match value {
                        Tag::Str(text) => vec![
                            MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: "Text".into(), name: fileName, value: Tag::Str(text.clone()), ..Default::default() })), vec![], false),
                            MetaInfo::new(Some("Text".into()), Tag::None, vec![
                                MetaInfo::new(Some(format!("Length: {}", text.chars().count())), Tag::None, vec![], false),
                            ], false)],
                        Tag::Bytes(bytes) => vec![
                            MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent { type_: "Hex".into(), name: fileName, value: Tag::stream(Box::new(Cursor::new(bytes.clone()))), ..Default::default() })), vec![], false),
                            MetaInfo::new(Some("Bytes".into()), Tag::None, vec![
                                MetaInfo::new(Some(format!("Length: {}", bytes.len())), Tag::None, vec![], false),
                            ], false)],
                        _ => unreachable!(),
                    });
                }
                _ => {} // IDisposable: dropped with obj
            }
            let Some(mut nodes) = nodes else { return Ok(None) };
            let mut items = vec![
                MetaInfo::new(Some(format!("Path: {}", file.path)), Tag::None, vec![], false),
                MetaInfo::new(Some(format!("FileSize: {}", file.fileSize)), Tag::None, vec![], false),
                MetaInfo::new(Some(format!("AtEnd: {}", if archive.atEnd() { "True" } else { "False" })), Tag::None, vec![], false),
            ];
            if let Some(parts) = &file.parts {
                items.push(MetaInfo::new(Some("Parts".into()), Tag::None, parts.iter().map(|s| MetaInfo::new(Some(format!("{}@{}", s.fileSize, s.path)), Tag::None, vec![], false)).collect(), false));
            }
            nodes.push(MetaInfo::new(Some("File".into()), Tag::None, items, false));
            Ok(Some(nodes))
        })
    }

    /// Gets the meta items.
    pub fn getMetaItems(manager: &dyn MetaManager, archive: &BinaryArchive) -> Vec<MetaItem> {
        let mut root: Vec<MetaItem> = Vec::new();
        let Some(mut files) = archive.files() else { return root };
        if files.is_empty() { return root; }
        let mut currentPath: Option<String> = None;
        // index path from root to the current folder
        let mut currentFolder: Vec<usize> = Vec::new();
        fn folder<'a>(root: &'a mut Vec<MetaItem>, at: &[usize]) -> &'a mut Vec<MetaItem> {
            at.iter().fold(root, |s, &i| &mut s[i].items)
        }

        // parse paths
        files.sort_by(|a, b| a.path.cmp(&b.path));
        let archiveDyn: Arc<dyn Archive> = archive.base.this();
        for file in files {
            // next path, skip empty
            let Some(path) = file.path.get(archive.pathSkip..) else { continue };
            if path.is_empty() { continue; }

            // folder
            let fileFolder = directoryName(path);
            if currentPath.as_deref() != Some(fileFolder) {
                currentPath = Some(fileFolder.to_string());
                currentFolder.clear();
                if !fileFolder.is_empty() {
                    for name in fileFolder.split(['\\', '/']) {
                        let items = folder(&mut root, &currentFolder);
                        match items.iter().position(|x| x.name == name && x.archive.is_none()) {
                            Some(i) => currentFolder.push(i),
                            None => {
                                items.push(MetaItem::new(Tag::None, name, manager.folderIcon(), Tag::None, None, None));
                                currentFolder.push(items.len() - 1);
                            }
                        }
                    }
                }
            }

            // pakfile
            if let Some(arc) = &file.arc {
                let items = Self::getMetaItems(manager, arc);
                let name = fileName(&file.path);
                folder(&mut root, &currentFolder).push(MetaItem::new(Tag::File(file.clone()), &name, manager.packageIcon(), Tag::None, Some(archiveDyn.clone()), Some(items)));
                continue;
            }

            // file
            let fileName = fileName(path);
            let mask = *archive.fileMask.lock().unwrap();
            let fileNameForIcon = mask.map(|s| s(&fileName)).unwrap_or_else(|| fileName.clone());
            let extentionForIcon = extension(&fileNameForIcon);
            let extentionForIcon = extentionForIcon.strip_prefix('.').unwrap_or(&extentionForIcon);
            folder(&mut root, &currentFolder).push(MetaItem::new(Tag::File(file.clone()), &fileName, manager.getIcon(extentionForIcon), Tag::None, Some(archiveDyn.clone()), None));
        }
        root
    }
}

// #endregion

// C# `System.IO.Path`, on either separator.
pub fn fileName(path: &str) -> String { path.rsplit(['\\', '/']).next().unwrap_or("").to_string() }
pub fn directoryName(path: &str) -> &str { path.rfind(['\\', '/']).map(|i| &path[..i]).unwrap_or("") }
pub fn extension(path: &str) -> String {
    let name = fileName(path);
    name.rfind('.').map(|i| name[i..].to_string()).unwrap_or_default()
}
