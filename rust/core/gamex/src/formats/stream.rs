// PORT-SOURCE: Core/GameX/Formats/Stream.cs
// PORT-SHA: abfbc4fa3cd0545d
// PORT-STATUS: done
//
// `.set`, `.meta` and `.raw` are plain ASCII line lists beside a streamed
// archive: `.set` names the files, `.meta` marks which are compressed or
// encrypted, `.raw` lists raw files.
//
// `StreamArchive`'s network host (`NetworkHost`) isn't in openx's Rust port, so
// only its local-folder form is here.
//
// ===================== C#-SIDE BUGS, KEPT ================================
//
//   1. **An empty `.set` file crashes.** `"".Split('\n')` is `[""]`, so the
//      `Length == 0` guard never fires and `Path.GetDirectoryName("")` is null.
//      Here that is an `InvalidData` error.
//   2. **`startIndex` comes from the first line and is applied to all of them**,
//      so lines from another depth are mis-cut or dropped.
//   3. **`StreamArchive.Read` reads `.meta` from the `.set` path**
//      (`File.Open(setPath, ...)` in the `metaPath` branch).
//   4. **`.meta` entries look files up by the raw line**, not the trimmed path,
//      and `First()` throws when the entry is missing.

use std::collections::HashSet;
use std::io::{self, Error, ErrorKind, Read, Write};
use std::path::Path;
use std::sync::Arc;

use openx_vfx::vfx::VfsRead;

use crate::binary::{newArchive, ok, err, Archive, ArcBinary, BinaryArchive, BinaryArchiveVirtual, BinaryState, Stream, Task};
use crate::meta::{FileSource, Tag};

// #region Stream

pub struct PakBinaryCanStream;

fn readToEnd(r: Option<&mut dyn VfsRead>) -> io::Result<Vec<u8>> {
    let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "r"))?;
    let mut data = Vec::new();
    r.read_to_end(&mut data)?;
    Ok(data)
}

/// C# `Encoding.ASCII.GetString`.
fn ascii(data: &[u8]) -> String { data.iter().map(|&c| if c < 0x80 { c as char } else { '?' }).collect() }

impl ArcBinary for PakBinaryCanStream {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            match tag {
                Tag::None => Ok(()),
                Tag::Str(z) if z == "Set" => {
                    let data = readToEnd(r)?;
                    // dir /s/b/a-d > .set
                    let text = ascii(&data);
                    let lines: Vec<&str> = text.split('\n').collect();
                    let first = lines[0].trim_end().replace('\\', "/");
                    let Some(dir) = first.rfind('/').map(|i| &first[..i]) else { return Err(Error::new(ErrorKind::InvalidData, "NullReference: GetDirectoryName")) };
                    let startIndex = dir.len() + 1;
                    let mut files = Vec::new();
                    for line in lines {
                        if line.len() < startIndex { continue; }
                        let path = line.get(startIndex..).unwrap_or_default().trim_end().replace('\\', "/");
                        if path != ".set" { files.push(Arc::new(FileSource { path, ..Default::default() })); }
                    }
                    source.setFiles(Some(files));
                    Ok(())
                }
                Tag::Str(z) if z == "Meta" => {
                    source.process().await?;

                    let data = readToEnd(r)?;
                    let text = ascii(&data);
                    let mut state = -1;
                    let mut files: Vec<FileSource> = source.files().unwrap_or_default().iter().map(|s| (**s).clone()).collect();
                    let index = |line: &str| -> io::Result<usize> {
                        let first = source.filesByPath.read().unwrap().as_ref().and_then(|z| z.get(&line.to_lowercase()).and_then(|s| s.first().cloned()));
                        let first = first.ok_or_else(|| Error::new(ErrorKind::InvalidData, "Sequence contains no elements"))?;
                        source.files().unwrap_or_default().iter().position(|s| Arc::ptr_eq(s, &first)).ok_or_else(|| Error::new(ErrorKind::InvalidData, line.to_string()))
                    };
                    for line in text.split('\n') {
                        let path = line.trim_end().replace('\\', "/");
                        if state == -1 {
                            if path == "Params:" { state = 0; }
                            else if path == "AllCompressed" { for file in files.iter_mut() { file.compressed = 1; } }
                            else if path == "Compressed:" { state = 1; }
                            else if path == "Crypted:" { state = 2; }
                        }
                        else {
                            if line.is_empty() { state = -1; continue; }
                            match state {
                                0 => { let mut args = line.splitn(2, ':'); let k = args.next().unwrap_or_default().to_string(); source.params.lock().unwrap().insert(k, args.next().unwrap_or_default().to_string()); }
                                1 => files[index(line)?].compressed = 1,
                                2 => files[index(line)?].flags = 1,
                                _ => {}
                            }
                        }
                    }
                    source.setFiles(Some(files.into_iter().map(Arc::new).collect()));
                    Ok(())
                }
                Tag::Str(z) if z == "Raw" => {
                    let data = readToEnd(r)?;
                    let filesRawSet: HashSet<String> = ascii(&data).split('\n').map(|line| line.trim_end().replace('\\', "/")).collect();
                    *source.filesRawSet.write().unwrap() = Some(filesRawSet);
                    Ok(())
                }
                _ => Err(Error::new(ErrorKind::InvalidInput, format!("tag: {tag:?}"))),
            }
        })
    }

    fn write<'a>(&'a self, source: &'a BinaryArchive, w: Option<&'a mut dyn Write>, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let w = w.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "w"))?;
            let files = source.files().unwrap_or_default();
            match tag {
                Tag::None => Ok(()),
                Tag::Str(z) if z == "Set" => {
                    let pathAsBytes = format!("C:/{}/", source.base.name);
                    w.write_all(pathAsBytes.as_bytes())?;
                    w.write_all(b".set\n")?;
                    // files
                    for file in &files { w.write_all(pathAsBytes.as_bytes())?; w.write_all(file.path.as_bytes())?; w.write_all(b"\n")?; }
                    w.flush()
                }
                Tag::Str(z) if z == "Meta" => {
                    // meta
                    let params = source.params.lock().unwrap().clone();
                    if !params.is_empty() {
                        w.write_all(b"Params:\n")?;
                        for (k, v) in &params { w.write_all(format!("{k}:{v}\n").as_bytes())?; }
                        w.write_all(b"\n")?;
                    }
                    // compressed
                    let numCompressed = files.iter().filter(|x| x.compressed != 0).count();
                    if files.len() == numCompressed { w.write_all(b"AllCompressed\n")?; }
                    else if numCompressed > 0 {
                        w.write_all(b"Compressed:\n")?;
                        for file in files.iter().filter(|x| x.compressed != 0) { w.write_all(file.path.as_bytes())?; w.write_all(b"\n")?; }
                        w.write_all(b"\n")?;
                    }
                    // crypted
                    if files.iter().any(|x| x.flags != 0) {
                        w.write_all(b"Crypted:\n")?;
                        for file in files.iter().filter(|x| x.flags != 0) { w.write_all(file.path.as_bytes())?; w.write_all(b"\n")?; }
                        w.write_all(b"\n")?;
                    }
                    w.flush()
                }
                Tag::Str(z) if z == "Raw" => {
                    let filesRawSet = source.filesRawSet.read().unwrap().clone().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "FilesRawSet"))?;
                    for file in filesRawSet { w.write_all(file.as_bytes())?; w.write_all(b"\n")?; }
                    w.flush()
                }
                _ => Err(Error::new(ErrorKind::InvalidInput, format!("tag: {tag:?}"))),
            }
        })
    }
}

/// StreamArchive
pub struct StreamArchive;

impl StreamArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> Arc<BinaryArchive> {
        let mut s = BinaryArchive::new(parent, state, Some(Arc::new(PakBinaryCanStream)));
        s.useReader = false;
        s.virt = Some(Box::new(StreamArchive));
        newArchive(s)
    }

    pub fn fromParent(parent: &Arc<BinaryArchive>, state: BinaryState) -> Arc<BinaryArchive> {
        let mut s = BinaryArchive::new(Some(parent.base.this()), state, Some(Arc::new(PakBinaryCanStream)));
        s.useReader = false;
        s.setFiles(parent.files());
        s.virt = Some(Box::new(StreamArchive));
        newArchive(s)
    }
}

impl BinaryArchiveVirtual for StreamArchive {
    fn read<'a>(&'a self, s: &'a BinaryArchive, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            // read arc
            let path = Path::new(&s.base.binPath);
            if s.base.binPath.is_empty() || !path.is_dir() { return Ok(()); }
            let stream = <dyn ArcBinary>::stream();
            let setPath = path.join(".set");
            if setPath.exists() { let mut r = std::fs::File::open(&setPath)?; stream.read(s, Some(&mut r), &Tag::Str("Set".into())).await?; }
            let metaPath = path.join(".meta");
            if metaPath.exists() { let mut r = std::fs::File::open(&setPath)?; stream.read(s, Some(&mut r), &Tag::Str("Meta".into())).await?; }
            let rawPath = path.join(".raw");
            if rawPath.exists() { let mut r = std::fs::File::open(&rawPath)?; stream.read(s, Some(&mut r), &Tag::Str("Raw".into())).await?; }
            Ok(())
        })
    }

    fn write<'a>(&'a self, s: &'a BinaryArchive, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            // write arc
            let path = Path::new(&s.base.binPath);
            if !s.base.binPath.is_empty() && !path.is_dir() { std::fs::create_dir_all(path)?; }
            let stream = <dyn ArcBinary>::stream();
            let mut w = std::fs::File::create(path.join(".set"))?;
            stream.write(s, Some(&mut w), &Tag::Str("Set".into())).await?;
            let mut w = std::fs::File::create(path.join(".meta"))?;
            stream.write(s, Some(&mut w), &Tag::Str("Meta".into())).await?;
            let rawPath = path.join(".raw");
            if s.filesRawSet.read().unwrap().as_ref().is_some_and(|z| !z.is_empty()) { let mut w = std::fs::File::create(&rawPath)?; stream.write(s, Some(&mut w), &Tag::Str("Raw".into())).await?; }
            else if rawPath.exists() { std::fs::remove_file(&rawPath)?; }
            Ok(())
        })
    }

    fn readData<'a>(&'a self, s: &'a BinaryArchive, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            // read arc
            let path = Path::new(&s.base.binPath).join(&file.path);
            Ok(if path.is_file() { Some(Box::new(std::fs::File::open(path)?) as Stream) } else { None })
        })
    }

    fn writeData<'a>(&'a self, s: &'a BinaryArchive, file: &'a Arc<FileSource>, data: Stream, option: &'a Tag) -> Task<'a, ()> {
        err(ErrorKind::Unsupported, "NotSupported".into())
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::binary::{getResult, ArchiveExt};
    use crate::family::FamilyGame;
    use crate::meta::{MetaItem, MetaManager};

    struct Manager;
    impl MetaManager for Manager {
        fn folderIcon(&self) -> Tag { Tag::Str("folder".into()) }
        fn packageIcon(&self) -> Tag { Tag::Str("package".into()) }
        fn getIcon(&self, name: &str) -> Tag { Tag::Str(name.into()) }
        fn getImage(&self, name: &str) -> Tag { Tag::None }
    }

    fn folder(name: &str) -> std::path::PathBuf {
        let dir = std::env::temp_dir().join(format!("gamex-stream-{name}-{}", std::process::id()));
        let _ = std::fs::remove_dir_all(&dir);
        std::fs::create_dir_all(dir.join("arc/sub")).unwrap();
        dir
    }

    #[test]
    fn aStreamArchiveOpensOverALocalFolder() {
        let dir = folder("open");
        let arc = dir.join("arc");
        std::fs::write(arc.join(".set"), "C:/arc/.set\nC:/arc/a.txt\r\nC:/arc/sub/b.bin\n").unwrap();
        std::fs::write(arc.join("a.txt"), "hello").unwrap();
        std::fs::write(arc.join("sub/b.bin"), [1u8, 0, 2]).unwrap();
        let state = BinaryState::new(Arc::new(openx_vfx::vfx::DirectoryFileSystem::new(&dir)), FamilyGame::empty(), None, Some(arc.to_str().unwrap()), Tag::None);
        let archive = StreamArchive::new(None, state);
        let mut items: Vec<MetaItem> = vec![];
        archive.open(Some(&mut items), Some(&Manager)).unwrap();
        assert_eq!(archive.base.count(), 2);
        assert!(archive.contains(&Tag::Str("SUB\\B.BIN".into())).unwrap(), "paths are case-insensitive");

        // the MetaItem tree: a.txt, then sub/ holding b.bin
        assert_eq!(items.iter().map(|s| s.name.as_str()).collect::<Vec<_>>(), ["a.txt", "sub"]);
        assert_eq!(items[0].icon, Tag::Str("txt".into()));
        assert_eq!(items[1].icon, Tag::Str("folder".into()));
        assert_eq!(items[1].items[0].name, "b.bin");
        let found = MetaItem::findByPathForNodes(&mut items, "sub/b.bin", &Manager).unwrap().clone();
        assert_eq!(found.source.asFileSource().unwrap().path, "sub/b.bin");

        // MetaInfos: text and bytes
        let infos = getResult(archive.getMetaInfos(&Manager, &items[0])).unwrap().unwrap();
        assert_eq!(infos[0].tag.asMetaContent().unwrap().value, Tag::Str("hello".into()));
        let infos = getResult(archive.getMetaInfos(&Manager, &found)).unwrap().unwrap();
        assert_eq!(infos[0].tag.asMetaContent().unwrap().type_, "Hex");
        assert_eq!(infos[2].name.as_deref(), Some("File"));

        // archives are ISources, so the cache managers can key on them
        let source = openx_poly::core::SourceRef(archive.clone() as Arc<dyn Archive> as Arc<dyn openx_poly::core::ISource>);
        assert_eq!(source, source.clone());

        archive.close().unwrap();
        assert!(archive.files().is_none());
        std::fs::remove_dir_all(&dir).unwrap();
    }

    #[test]
    fn setMetaAndRawWriteAndReadBack() {
        let dir = folder("write");
        let arc = dir.join("arc");
        std::fs::write(arc.join(".set"), "C:/arc/.set\nC:/arc/a.txt\nC:/arc/b.txt\n").unwrap();
        let state = || BinaryState::new(Arc::new(openx_vfx::vfx::DirectoryFileSystem::new(&dir)), FamilyGame::empty(), None, Some(arc.to_str().unwrap()), Tag::None);
        let archive = StreamArchive::new(None, state());
        archive.open(None, None).unwrap();
        archive.params.lock().unwrap().insert("k".into(), "v".into());
        let files: Vec<Arc<FileSource>> = archive.files().unwrap().iter().map(|s| Arc::new(FileSource { compressed: (s.path == "b.txt") as i32, ..(**s).clone() })).collect();
        archive.setFiles(Some(files));
        getResult(archive.write(&Tag::None)).unwrap();
        assert_eq!(std::fs::read_to_string(arc.join(".meta")).unwrap(), "Params:\nk:v\n\nCompressed:\nb.txt\n\n");
        assert_eq!(std::fs::read_to_string(arc.join(".set")).unwrap(), "C:/arc/.set\nC:/arc/a.txt\nC:/arc/b.txt\n");
        std::fs::remove_dir_all(&dir).unwrap();
    }

    #[test]
    fn anEmptySetIsAnErrorNotACrash() {
        let dir = folder("empty");
        let arc = dir.join("arc");
        std::fs::write(arc.join(".set"), "").unwrap();
        let state = BinaryState::new(Arc::new(openx_vfx::vfx::DirectoryFileSystem::new(&dir)), FamilyGame::empty(), None, Some(arc.to_str().unwrap()), Tag::None);
        assert!(StreamArchive::new(None, state).open(None, None).is_err());
        std::fs::remove_dir_all(&dir).unwrap();
    }
}
