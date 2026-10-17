// PORT-SOURCE: Vfx/OpenX.Vfx/Vfx.cs
// PORT-SHA: 4737d0dae6cc3f9e
// PORT-STATUS: done
//
// The virtual filesystem: a `FileSystem` abstraction with directory, in-memory,
// aggregate, and archive-backed implementations, plus `Advance`, which nests a
// filesystem inside a container file (a zip, a disc image, a 3DS cartridge).
//
// C# uses an abstract base class with four abstract members and two virtual
// ones. That maps to a trait with provided methods, and `Advance`/`FindPaths`
// become provided methods on the trait — same shape, no inheritance.
//
// ================= THE PATH MATCHER IS LOCALE-DEPENDENT ====================
//
// `CreateMatcher` compares filenames with `StringComparison.CurrentCultureIgnoreCase`
// in all three of its branches. For **file paths** that is the wrong
// comparison: under a Turkish locale `"I".ToLower()` is `"ı"`, not `"i"`, so a
// file named `INDEX.DAT` does not match the pattern `index.dat` — and the same
// build works fine in en-US. It is the classic Turkish-I bug, and it makes asset
// lookup silently locale-dependent. Paths want ordinal comparison; this port
// uses ASCII-case-insensitive matching, which is locale-invariant.
//
// Two more, both crashes where the surrounding code returns null:
//
//   * `ZipFileSystem.Open` is `Arc.GetEntry(...).Open()` with no null check, so
//     a missing entry throws `NullReferenceException` — while
//     `DirectoryFileSystem.Open` and `VirtualFileSystem.Open` return null for
//     the same condition. `AggregateFileSystem.Open` relies on that null to try
//     the next filesystem, so one zip in an aggregate turns a normal miss into
//     an exception.
//   * `SevenZipFileSystem.Open` uses `.First(...)`, which throws
//     `InvalidOperationException` on a miss, for the same reason.
//   * `Advance` for `.bin`/`.cue` calls `Glob("", "*.cue").Single()`, which
//     throws when a directory holds zero or several `.cue` files — a multi-disc
//     set is not exotic.
//
// `NetworkFileSystem` is ported as C# wrote it, which does not fetch anything.
// The constructor rejects a URI whose path has a filename. `Glob` asks the
// local matcher to search the pattern as a directory (it is not one).
// `FileExists` and `FileInfo` call local `File.Exists`, and `FileInfo`'s
// length is 0. `Open` returns null. `Advance` for `.zip`, `.7z`, `.bin`/`.cue`,
// `.n64`/`.v64`/`.z64` and `.3ds` returns null: those filesystems are not
// ported, so `Next` stays on this one. See `vfx_network.rs` for `NetworkHost`.

use std::io::{self, Read, Seek, Write};
use std::path::{Path, PathBuf};

/// C# `(string path, long length)` from `FileInfo`.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct FileInfo {
    pub path: String,
    pub length: u64,
}

/// Anything the VFS can hand back for reading.
pub trait VfsRead: Read + Seek {}
impl<T: Read + Seek> VfsRead for T {}

/// C# `Stream` opened with mode `"w"`: write and seek, owned by the caller.
pub trait VfsWrite: Write + Seek + Send {}
impl<T: Write + Seek + Send> VfsWrite for T {}

/// C# `abstract class FileSystem`.
pub trait FileSystem: Send + Sync {
    /// C# `Glob(string path, string searchPattern)`.
    fn glob(&self, path: &str, searchPattern: &str) -> Vec<String>;

    /// C# `FileExists(string path)`.
    fn fileExists(&self, path: &str) -> bool;

    /// C# `FileInfo(string path)` — `None` where the C# returned `(null, 0)`.
    fn fileInfo(&self, path: &str) -> Option<FileInfo>;

    /// C# `Open(string path, string mode)`.
    ///
    /// Returns `None` for a missing entry. The C#'s archive implementations
    /// threw here instead of returning null, which broke `AggregateFileSystem`'s
    /// fallback; see the module header.
    fn open(&self, path: &str) -> Option<Box<dyn VfsRead + '_>>;

    /// C# `Open(path, "w")`. `None` when this filesystem has no write mode.
    ///
    /// The file must already exist. C# uses `FileMode.Open` and `FileAccess.Write`,
    /// which does not create or truncate.
    fn openWrite(&self, path: &str) -> Option<Box<dyn VfsWrite + Send>> { None }

    /// An owned reader, so an archive can return it to a pool. `None` when the
    /// reader would borrow `self`.
    fn openOwned(&self, path: &str) -> Option<Box<dyn VfsRead + Send>> { None }

    /// C# `vfx is NetworkFileSystem`.
    fn isNetwork(&self) -> bool { false }

    /// C# `FindPaths(string path, string searchPattern)`.
    ///
    /// Expands `(a:b:c)` alternation groups in the pattern before globbing, so
    /// `tex(0:1).dds` searches `tex0.dds` and `tex1.dds`.
    fn findPaths(&self, path: &str, searchPattern: &str) -> Vec<String> {
        if let Some((prefix, alts, suffix)) = splitAlternation(searchPattern) {
            let mut out = Vec::new();
            for alt in alts {
                let expanded = format!("{prefix}{alt}{suffix}");
                out.extend(self.findPaths(path, &expanded));
            }
            return out;
        }
        self.glob(path, searchPattern)
    }
}

/// Splits `"a(x:y)b"` into `("a", ["x", "y"], "b")`.
fn splitAlternation(pattern: &str) -> Option<(&str, Vec<&str>, &str)> {
    let start = pattern.find('(')?;
    let mid = pattern[start..].find(':')? + start;
    let end = pattern[mid..].find(')')? + mid;
    if start >= end {
        return None;
    }
    Some((
        &pattern[..start],
        pattern[start + 1..end].split(':').collect(),
        &pattern[end + 1..],
    ))
}

/// C# `FileSystem.CreateMatcher(string searchPattern)`.
///
/// Locale-invariant, unlike the C# — see the module header. An empty pattern
/// matches everything, as in the C#.
pub fn createMatcher(searchPattern: &str) -> Box<dyn Fn(&str) -> bool> {
    if searchPattern.is_empty() {
        return Box::new(|_| true);
    }
    let stars = searchPattern.matches('*').count();
    if stars == 0 {
        let p = searchPattern.to_ascii_lowercase();
        return Box::new(move |x| x.to_ascii_lowercase() == p);
    }
    if stars == 1 {
        let bare = searchPattern.replace('*', "").to_ascii_lowercase();
        if searchPattern.starts_with('*') {
            return Box::new(move |x| x.to_ascii_lowercase().ends_with(&bare));
        }
        if searchPattern.ends_with('*') {
            return Box::new(move |x| x.to_ascii_lowercase().starts_with(&bare));
        }
    }
    // General case: `*` matches any run, everything else is literal. The C#
    // built a regex here; a direct matcher avoids the dependency and the
    // `catch { return false; }` that silently swallowed bad patterns.
    let parts: Vec<String> = searchPattern
        .to_ascii_lowercase()
        .split('*')
        .map(str::to_string)
        .collect();
    Box::new(move |x| globMatch(&x.to_ascii_lowercase(), &parts))
}

/// `parts` are the literal runs between `*`s, already lowercased.
fn globMatch(s: &str, parts: &[String]) -> bool {
    if parts.is_empty() {
        return true;
    }
    if !s.starts_with(&*parts[0]) {
        return false;
    }
    let mut rest = &s[parts[0].len()..];
    let last = parts.len() - 1;
    for (i, part) in parts.iter().enumerate().skip(1) {
        if i == last {
            // Final literal must land at the end.
            return rest.len() >= part.len() && rest.ends_with(&**part);
        }
        match rest.find(&**part) {
            Some(at) => rest = &rest[at + part.len()..],
            None => return false,
        }
    }
    true
}

/// C# `class VirtualFileSystem` — an in-memory map of path to bytes.
#[derive(Debug, Default, Clone)]
pub struct VirtualFileSystem {
    virtuals: std::collections::HashMap<String, Vec<u8>>,
}

impl VirtualFileSystem {
    pub fn new(virtuals: std::collections::HashMap<String, Vec<u8>>) -> Self {
        Self { virtuals }
    }

    pub fn insert(&mut self, path: impl Into<String>, data: Vec<u8>) {
        self.virtuals.insert(path.into(), data);
    }
}

impl FileSystem for VirtualFileSystem {
    fn glob(&self, _path: &str, searchPattern: &str) -> Vec<String> {
        let m = createMatcher(searchPattern);
        let mut out: Vec<String> = self
            .virtuals
            .keys()
            .filter(|k| m(k))
            .cloned()
            .collect();
        // HashMap order is arbitrary; the C# relied on Dictionary insertion
        // order, which is itself unspecified. Sorting makes globs reproducible.
        out.sort();
        out
    }

    fn fileExists(&self, path: &str) -> bool {
        self.virtuals.contains_key(path)
    }

    fn fileInfo(&self, path: &str) -> Option<FileInfo> {
        self.virtuals.get(path).map(|d| FileInfo {
            path: path.to_string(),
            length: d.len() as u64,
        })
    }

    fn open(&self, path: &str) -> Option<Box<dyn VfsRead + '_>> {
        self.virtuals
            .get(path)
            .map(|d| Box::new(std::io::Cursor::new(d.as_slice())) as Box<dyn VfsRead + '_>)
    }

    fn openOwned(&self, path: &str) -> Option<Box<dyn VfsRead + Send>> {
        self.virtuals.get(path).map(|d| Box::new(std::io::Cursor::new(d.clone())) as Box<dyn VfsRead + Send>)
    }
}

/// C# `class AggregateFileSystem` — first hit wins.
pub struct AggregateFileSystem {
    aggregate: Vec<Box<dyn FileSystem>>,
}

impl AggregateFileSystem {
    /// C# spells the field `aggreate`; corrected here.
    pub fn new(aggregate: Vec<Box<dyn FileSystem>>) -> Self {
        Self { aggregate }
    }
}

impl FileSystem for AggregateFileSystem {
    fn glob(&self, path: &str, searchPattern: &str) -> Vec<String> {
        self.aggregate
            .iter()
            .flat_map(|f| f.glob(path, searchPattern))
            .collect()
    }

    fn fileExists(&self, path: &str) -> bool {
        self.aggregate.iter().any(|f| f.fileExists(path))
    }

    fn fileInfo(&self, path: &str) -> Option<FileInfo> {
        self.aggregate.iter().find_map(|f| f.fileInfo(path))
    }

    fn open(&self, path: &str) -> Option<Box<dyn VfsRead + '_>> {
        self.aggregate.iter().find_map(|f| f.open(path))
    }

    fn openWrite(&self, path: &str) -> Option<Box<dyn VfsWrite + Send>> {
        self.aggregate.iter().find_map(|f| f.openWrite(path))
    }

    fn openOwned(&self, path: &str) -> Option<Box<dyn VfsRead + Send>> {
        self.aggregate.iter().find_map(|f| f.openOwned(path))
    }

    fn isNetwork(&self) -> bool { self.aggregate.iter().any(|f| f.isNetwork()) }
}

/// C# `class DirectoryFileSystem` — a real directory on disk.
///
/// `baseRoot` is the path passed in. `basePath` is the optional extra segment.
/// `Next` may move `root` (a file root becomes its parent; a non-empty
/// `basePath` is joined on) and, in C#, into a zip or disc. Those archive
/// filesystems are not ported, so `advance` returns `None` and `Next` stays here.
///
/// A missing directory does not throw. C# `Directory.EnumerateFiles` throws
/// `DirectoryNotFoundException`; here `Next` leaves `root` as it is, which is
/// what a game install that is not on this machine needs.
pub struct DirectoryFileSystem {
    baseRoot: PathBuf,
    basePath: String,
    root: PathBuf,
    /// C# `Skip`, the root length plus one. Glob still strips with `strip_prefix`.
    skip: usize,
}

impl DirectoryFileSystem {
    /// C# `DirectoryFileSystem(string baseRoot, string basePath)`. `None` is C#'s null.
    pub fn new(baseRoot: impl AsRef<Path>, basePath: Option<&str>) -> Self {
        let baseRoot = baseRoot.as_ref().to_path_buf();
        let skip = baseRoot.as_os_str().len() + 1;
        Self { root: baseRoot.clone(), baseRoot, basePath: basePath.unwrap_or("").to_string(), skip }
    }

    /// Joins under the current root and rejects anything that escapes it.
    ///
    /// The C# calls `Path.Combine(Root, path)` with no such check, so a path
    /// containing `..` reads outside the intended tree — and these paths come
    /// from asset files. This guard was already in the port; `Next` does not remove it.
    fn resolve(&self, path: &str) -> Option<PathBuf> {
        let joined = self.root.join(path);
        let mut depth = 0i32;
        for c in std::path::Path::new(path).components() {
            match c {
                std::path::Component::ParentDir => {
                    depth -= 1;
                    if depth < 0 {
                        return None;
                    }
                }
                std::path::Component::Normal(_) => depth += 1,
                std::path::Component::RootDir | std::path::Component::Prefix(_) => return None,
                std::path::Component::CurDir => {}
            }
        }
        Some(joined)
    }

    /// C# `Next()`. A missing directory leaves `root` as it is. C# throws
    /// `DirectoryNotFoundException` there. The first file's order is the
    /// filesystem's, which can differ from .NET's. `Advance` for zip, 7z, disc,
    /// n64 and 3ds is not ported, so those extensions stay on this filesystem.
    pub fn next(mut self) -> Self {
        if self.root.is_file() || fileName(&self.root).contains('*') {
            if let Some(parent) = self.root.parent() {
                self.root = parent.to_path_buf();
                self.skip = self.root.as_os_str().len() + 1;
            }
            return self;
        }
        let Some(first) = firstFile(&self.root) else { return self; };
        if endsIgnore(&first, ".bin") || endsIgnore(&first, ".cue") { return self; }
        if !self.basePath.is_empty() {
            self.root = pathCombine(&self.baseRoot, &self.basePath);
            self.skip = self.root.as_os_str().len() + 1;
        }
        self
    }
}

impl FileSystem for DirectoryFileSystem {
    fn glob(&self, path: &str, searchPattern: &str) -> Vec<String> {
        let Some(base) = self.resolve(path) else {
            return Vec::new();
        };
        let m = createMatcher(searchPattern);
        let mut out = Vec::new();
        walk(&base, &base, &m, &mut out);
        out.sort();
        out
    }

    fn fileExists(&self, path: &str) -> bool {
        self.resolve(path).map(|p| p.is_file()).unwrap_or(false)
    }

    fn fileInfo(&self, path: &str) -> Option<FileInfo> {
        let p = self.resolve(path)?;
        let md = std::fs::metadata(&p).ok()?;
        if !md.is_file() {
            return None;
        }
        Some(FileInfo { path: path.to_string(), length: md.len() })
    }

    fn open(&self, path: &str) -> Option<Box<dyn VfsRead + '_>> {
        let p = self.resolve(path)?;
        std::fs::File::open(p)
            .ok()
            .map(|f| Box::new(f) as Box<dyn VfsRead + '_>)
    }

    /// C# `Open(path, "w")`: the file must exist. `FileMode.Open` does not create or truncate.
    fn openWrite(&self, path: &str) -> Option<Box<dyn VfsWrite + Send>> {
        let p = self.resolve(path)?;
        if !p.is_file() { return None; }
        std::fs::OpenOptions::new().write(true).open(p).ok().map(|f| Box::new(f) as Box<dyn VfsWrite + Send>)
    }

    fn openOwned(&self, path: &str) -> Option<Box<dyn VfsRead + Send>> {
        let p = self.resolve(path)?;
        std::fs::File::open(p).ok().map(|f| Box::new(f) as Box<dyn VfsRead + Send>)
    }
}

/// C# `Path.GetFileName`. A trailing separator means there is no name.
fn pathFileName(path: &str) -> &str {
    if path.is_empty() || path.ends_with('/') || path.ends_with('\\') { return ""; }
    match path.rfind(['/', '\\']) { Some(i) => &path[i + 1..], None => path }
}

fn fileName(path: &Path) -> String {
    path.file_name().map(|s| s.to_string_lossy().into_owned()).unwrap_or_default()
}

fn endsIgnore(s: &str, ext: &str) -> bool {
    s.get(s.len().saturating_sub(ext.len())..).is_some_and(|t| t.eq_ignore_ascii_case(ext))
}

/// C# `Path.Combine`. A rooted `b` (`/`, `\`, or `X:` on Windows) replaces `a`.
fn pathCombine(a: &Path, b: &str) -> PathBuf {
    if b.is_empty() { return a.to_path_buf(); }
    let bytes = b.as_bytes();
    let rooted = bytes.first().is_some_and(|c| *c == b'/' || *c == b'\\')
        || (cfg!(windows) && bytes.len() >= 2 && bytes[0].is_ascii_alphabetic() && bytes[1] == b':');
    if rooted { PathBuf::from(b) } else { a.join(b) }
}

/// The first file in `root`, or `Some("")` when the directory is empty.
/// `None` when the directory cannot be read. Order is the filesystem's.
fn firstFile(root: &Path) -> Option<String> {
    let rd = std::fs::read_dir(root).ok()?;
    for e in rd.flatten() {
        if e.path().is_file() { return Some(e.file_name().to_string_lossy().into_owned()); }
    }
    Some(String::new())
}

/// C# `class NetworkFileSystem`. It does not open a network stream.
pub struct NetworkFileSystem;

impl NetworkFileSystem {
    /// C# constructor. `localPath` is `uri.LocalPath`. A file name throws
    /// `ArgumentOutOfRangeException`.
    pub fn new(localPath: &str) -> io::Result<Self> {
        if !pathFileName(localPath).is_empty() {
            return Err(io::Error::new(io::ErrorKind::InvalidInput, format!("ArgumentOutOfRangeException: {localPath}")));
        }
        Ok(Self)
    }
}

impl FileSystem for NetworkFileSystem {
    /// C# builds a `Matcher` and searches the pattern itself as the directory.
    /// A pattern is not a directory, so this is empty.
    fn glob(&self, _path: &str, _searchPattern: &str) -> Vec<String> { Vec::new() }

    /// C# `File.Exists(path)` — the local disk, not the URI.
    fn fileExists(&self, path: &str) -> bool { Path::new(path).is_file() }

    /// Length is 0 whenever the local file exists.
    fn fileInfo(&self, path: &str) -> Option<FileInfo> {
        if Path::new(path).is_file() { Some(FileInfo { path: path.to_string(), length: 0 }) } else { None }
    }

    fn open(&self, _path: &str) -> Option<Box<dyn VfsRead + '_>> { None }

    fn isNetwork(&self) -> bool { true }
}

fn walk(
    base: &std::path::Path,
    dir: &std::path::Path,
    m: &dyn Fn(&str) -> bool,
    out: &mut Vec<String>,
) {
    let Ok(rd) = std::fs::read_dir(dir) else {
        return;
    };
    for e in rd.flatten() {
        let p = e.path();
        if p.is_dir() {
            walk(base, &p, m, out);
        } else if let Ok(rel) = p.strip_prefix(base) {
            let rel = rel.to_string_lossy().replace('\\', "/");
            if m(&rel) {
                out.push(rel);
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashMap;

    fn vfs() -> VirtualFileSystem {
        let mut m = HashMap::new();
        m.insert("a/index.dat".to_string(), b"one".to_vec());
        m.insert("a/INDEX.DAT2".to_string(), b"two".to_vec());
        m.insert("b/tex0.dds".to_string(), b"t0".to_vec());
        m.insert("b/tex1.dds".to_string(), b"t1".to_vec());
        VirtualFileSystem::new(m)
    }

    #[test]
    fn exact_match_is_case_insensitive_but_locale_invariant() {
        // Under a Turkish locale the C# fails this: "I".ToLower() is "ı".
        let m = createMatcher("a/index.dat");
        assert!(m("a/INDEX.DAT"));
        assert!(m("A/Index.Dat"));
        assert!(!m("a/index.dat2"));
    }

    #[test]
    fn prefix_and_suffix_wildcards() {
        assert!(createMatcher("*.dds")("b/tex0.dds"));
        assert!(!createMatcher("*.dds")("b/tex0.png"));
        assert!(createMatcher("b/*")("b/tex0.dds"));
        assert!(!createMatcher("b/*")("a/tex0.dds"));
    }

    #[test]
    fn interior_wildcards_match_runs() {
        let m = createMatcher("b/*.dds");
        assert!(m("b/tex0.dds"));
        assert!(!m("a/tex0.dds"));
        let m2 = createMatcher("*tex*dds");
        assert!(m2("b/tex1.dds"));
        assert!(!m2("b/img1.png"));
    }

    #[test]
    fn empty_pattern_matches_everything() {
        assert!(createMatcher("")("anything at all"));
    }

    #[test]
    fn alternation_groups_expand() {
        let f = vfs();
        let mut found = f.findPaths("", "b/tex(0:1).dds");
        found.sort();
        assert_eq!(found, vec!["b/tex0.dds", "b/tex1.dds"]);
    }

    #[test]
    fn alternation_with_no_group_falls_through_to_glob() {
        assert_eq!(vfs().findPaths("", "b/tex0.dds"), vec!["b/tex0.dds"]);
    }

    #[test]
    fn virtual_fs_reads_back_what_was_put_in() {
        let f = vfs();
        assert!(f.fileExists("a/index.dat"));
        assert_eq!(f.fileInfo("a/index.dat").unwrap().length, 3);
        let mut s = String::new();
        f.open("a/index.dat").unwrap().read_to_string(&mut s).unwrap();
        assert_eq!(s, "one");
    }

    #[test]
    fn missing_entries_return_none_rather_than_throwing() {
        // The archive filesystems in C# throw here, which breaks the aggregate.
        let f = vfs();
        assert!(f.open("nope").is_none());
        assert!(f.fileInfo("nope").is_none());
        assert!(!f.fileExists("nope"));
    }

    #[test]
    fn aggregate_falls_through_to_the_next_filesystem() {
        let mut a = HashMap::new();
        a.insert("only_in_a".to_string(), b"A".to_vec());
        let mut b = HashMap::new();
        b.insert("only_in_b".to_string(), b"B".to_vec());
        let agg = AggregateFileSystem::new(vec![
            Box::new(VirtualFileSystem::new(a)),
            Box::new(VirtualFileSystem::new(b)),
        ]);
        assert!(agg.fileExists("only_in_a"));
        assert!(agg.fileExists("only_in_b"));
        assert!(agg.open("only_in_b").is_some(), "must not stop at the first miss");
        assert!(agg.open("nowhere").is_none());
    }

    #[test]
    fn directory_fs_rejects_paths_escaping_its_root() {
        // The C# Path.Combine's straight through, so `..` reads outside.
        let d = DirectoryFileSystem::new("/tmp/does-not-matter", None);
        assert!(d.resolve("../../etc/passwd").is_none());
        assert!(d.resolve("a/../b").is_some(), "interior .. that stays inside is fine");
        assert!(d.resolve("ok/path").is_some());
    }

    fn tempDir(name: &str) -> std::path::PathBuf {
        let dir = std::env::temp_dir().join(format!("openx-vfx-{name}-{}", std::process::id()));
        let _ = std::fs::remove_dir_all(&dir);
        std::fs::create_dir_all(&dir).unwrap();
        dir
    }

    #[test]
    fn next_joins_base_path_onto_the_root() {
        let dir = tempDir("next");
        std::fs::create_dir_all(dir.join("sub")).unwrap();
        std::fs::write(dir.join("top.txt"), b"t").unwrap();
        std::fs::write(dir.join("sub/a.txt"), b"hi").unwrap();
        let d = DirectoryFileSystem::new(&dir, Some("sub")).next();
        assert!(d.fileExists("a.txt"));
        assert!(!d.fileExists("top.txt"));
        std::fs::remove_dir_all(&dir).unwrap();
    }

    #[test]
    fn next_on_a_bin_or_cue_does_not_apply_base_path() {
        let dir = tempDir("cue");
        std::fs::create_dir_all(dir.join("sub")).unwrap();
        std::fs::write(dir.join("a.cue"), b"cue").unwrap();
        std::fs::write(dir.join("sub/a.txt"), b"hi").unwrap();
        let d = DirectoryFileSystem::new(&dir, Some("sub")).next();
        assert!(d.fileExists("a.cue"), "Advance is not ported, so the directory stays");
        assert!(!d.fileExists("a.txt"));
        std::fs::remove_dir_all(&dir).unwrap();
    }

    #[test]
    fn next_on_a_file_moves_the_root_to_its_parent() {
        let dir = tempDir("file");
        let file = dir.join("game.pak");
        std::fs::write(&file, b"pak").unwrap();
        let d = DirectoryFileSystem::new(&file, Some("nope")).next();
        assert!(d.fileExists("game.pak"));
        std::fs::remove_dir_all(&dir).unwrap();
    }

    #[test]
    fn open_write_updates_an_existing_file_and_does_not_create() {
        let dir = tempDir("write");
        std::fs::write(dir.join("a.txt"), b"old").unwrap();
        let d = DirectoryFileSystem::new(&dir, None);
        {
            let mut w = d.openWrite("a.txt").unwrap();
            w.write_all(b"new").unwrap();
        }
        assert_eq!(std::fs::read(dir.join("a.txt")).unwrap(), b"new");
        assert!(d.openWrite("missing.txt").is_none());
        std::fs::remove_dir_all(&dir).unwrap();
    }

    #[test]
    fn network_file_system_rejects_a_filename_and_does_not_open() {
        assert!(NetworkFileSystem::new("/pak/file.pak").is_err());
        let n = NetworkFileSystem::new("/").unwrap();
        assert!(n.isNetwork());
        assert!(n.open("anything").is_none());
        assert!(n.glob("", "*.pak").is_empty());
        let dir = tempDir("net");
        let file = dir.join("local.txt");
        std::fs::write(&file, b"abcd").unwrap();
        let path = file.to_str().unwrap();
        assert!(n.fileExists(path));
        assert_eq!(n.fileInfo(path).unwrap().length, 0);
        assert!(!n.fileExists(dir.join("nope.txt").to_str().unwrap()));
        std::fs::remove_dir_all(&dir).unwrap();
    }
}
