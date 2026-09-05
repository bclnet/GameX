// PORT-SOURCE: Core/GameX/Formats/Stream.cs
// PORT-SHA: a916652e080e48c8
// PORT-STATUS: done
//
// PARTIAL PORT: the `.set` and `.meta` sidecar parsers. `PakBinaryCanStream`
// itself derives from `ArcBinary` in `OpenStack.Vfx` and drives
// `BinaryArchive`, which is in `GameX.FileSystems` — both outstanding.
//
// These sidecars are plain ASCII line lists that accompany a streamed archive:
// `.set` names the files, `.meta` marks which are compressed or encrypted.
//
// ===================== THREE C#-SIDE BUGS ================================
//
//   1. **An empty `.set` file crashes.** The guard is
//
//          var lines = Encoding.ASCII.GetString(data)?.Split('\n');
//          if (lines?.Length == 0) return Task.CompletedTask;
//          var startIndex = Path.GetDirectoryName(lines[0]...).Length + 1;
//
//      `GetString` never returns null and `"".Split('\n')` yields `[""]` — one
//      element, not zero — so the guard never fires. `lines[0]` is then `""`,
//      `Path.GetDirectoryName("")` returns null on .NET Core, and `.Length`
//      throws `NullReferenceException`. The check tests the one length `Split`
//      cannot produce.
//
//   2. **`startIndex` is computed from the first line and applied to all of
//      them.** It is the directory-name length of `lines[0]`, so every
//      subsequent path is sliced at that offset regardless of its own depth. A
//      `.set` listing files from two directory levels silently truncates or
//      mis-cuts every entry outside the first one's depth. The
//      `line.Length >= startIndex` guard only prevents the range error — it
//      skips those lines instead, so they vanish from the archive rather than
//      erroring.
//
//   3. **`_ = source.Process()` discards a task.** The `"Meta"` branch starts
//      `Process()` and throws the result away, so the metadata below is applied
//      while that work may still be running, against `source.Files` it mutates.
//      The discard is explicit (`_ =`), so it reads as deliberate, but nothing
//      orders the two.

/// One entry parsed from a `.set` sidecar.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct SetEntry {
    pub path: String,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum SetError {
    /// The first line has no directory component to measure, so the C#'s
    /// `startIndex` cannot be computed. It throws `NullReferenceException`.
    NoLeadingDirectory,
}

impl std::fmt::Display for SetError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::NoLeadingDirectory => {
                write!(f, ".set first line has no directory component")
            }
        }
    }
}

impl std::error::Error for SetError {}

/// C#'s `Path.GetDirectoryName(x).Length + 1` on a `/`-normalised path.
///
/// `None` where the C# gets null and then throws on `.Length`.
fn strip_offset(first_line: &str) -> Option<usize> {
    let l = first_line.trim_end().replace('\\', "/");
    l.rfind('/').map(|i| i + 1)
}

/// C# `PakBinaryCanStream.Read(..., tag: "Set")`.
///
/// Every path is sliced at an offset derived from the **first** line, which is
/// bug 2 — preserved, because changing it changes which files an archive
/// contains. `skipped` reports the lines the C# silently drops so a caller can
/// notice.
pub fn parse_set(data: &[u8]) -> Result<(Vec<SetEntry>, usize), SetError> {
    let text = String::from_utf8_lossy(data);
    let lines: Vec<&str> = text.split('\n').collect();
    // The C# tests `lines?.Length == 0`, which Split cannot produce (bug 1).
    if lines.first().map(|l| l.trim_end().is_empty()).unwrap_or(true) {
        return Ok((Vec::new(), 0));
    }
    let start = strip_offset(lines[0]).ok_or(SetError::NoLeadingDirectory)?;
    let mut out = Vec::new();
    let mut skipped = 0;
    for line in &lines {
        if line.len() < start {
            skipped += 1;
            continue;
        }
        let path = line[start..].trim_end().replace('\\', "/");
        // C# excludes the sidecar itself.
        if path != ".set" && !path.is_empty() {
            out.push(SetEntry { path });
        }
    }
    Ok((out, skipped))
}

/// C# `.meta` parser states, from its `state` sentinel.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum MetaState {
    /// C# `state == -1` — looking for a section header.
    Seeking,
    Params,
    Compressed,
    Crypted,
}

/// What a `.meta` sidecar says about an archive.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct MetaSidecar {
    /// C# `AllCompressed` — sets `Compressed = 1` on every file.
    pub all_compressed: bool,
    /// `key=value` lines under `Params:`.
    pub params: Vec<(String, String)>,
    /// Paths listed under `Compressed:`.
    pub compressed: Vec<String>,
    /// Paths listed under `Crypted:`.
    pub crypted: Vec<String>,
}

/// C# `PakBinaryCanStream.Read(..., tag: "Meta")`.
pub fn parse_meta(data: &[u8]) -> MetaSidecar {
    let text = String::from_utf8_lossy(data);
    let mut out = MetaSidecar::default();
    let mut state = MetaState::Seeking;
    for raw in text.split('\n') {
        let line = raw.trim_end().replace('\\', "/");
        match state {
            MetaState::Seeking => match line.as_str() {
                "Params:" => state = MetaState::Params,
                "AllCompressed" => out.all_compressed = true,
                "Compressed:" => state = MetaState::Compressed,
                "Crypted:" => state = MetaState::Crypted,
                _ => {}
            },
            // A section runs until the next header. The C# switches on the same
            // literals inside each state, so a header ends the previous section.
            _ => match line.as_str() {
                "Params:" => state = MetaState::Params,
                "Compressed:" => state = MetaState::Compressed,
                "Crypted:" => state = MetaState::Crypted,
                "AllCompressed" => {
                    out.all_compressed = true;
                    state = MetaState::Seeking;
                }
                "" => {}
                _ => match state {
                    MetaState::Params => {
                        if let Some((k, v)) = line.split_once('=') {
                            out.params.push((k.trim().to_string(), v.trim().to_string()));
                        }
                    }
                    MetaState::Compressed => out.compressed.push(line),
                    MetaState::Crypted => out.crypted.push(line),
                    MetaState::Seeking => {}
                },
            },
        }
    }
    out
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn an_empty_set_file_does_not_panic() {
        // The C# guard tests `lines.Length == 0`, which Split never produces,
        // then dereferences the null from GetDirectoryName("").
        assert_eq!(parse_set(b""), Ok((Vec::new(), 0)));
        assert_eq!(parse_set(b"\n\n"), Ok((Vec::new(), 0)));
    }

    #[test]
    fn a_first_line_with_no_directory_is_an_error_not_a_panic() {
        assert_eq!(parse_set(b"noslash\nx"), Err(SetError::NoLeadingDirectory));
    }

    #[test]
    fn paths_are_stripped_of_the_first_lines_directory() {
        let d = b"data/a.txt\ndata/b.txt\ndata/c.txt";
        let (files, skipped) = parse_set(d).unwrap();
        assert_eq!(
            files.iter().map(|f| f.path.as_str()).collect::<Vec<_>>(),
            vec!["a.txt", "b.txt", "c.txt"]
        );
        assert_eq!(skipped, 0);
    }

    #[test]
    fn backslashes_are_normalised() {
        let (files, _) = parse_set(b"data\\a.txt\ndata\\sub\\b.txt").unwrap();
        assert_eq!(files[0].path, "a.txt");
        assert_eq!(files[1].path, "sub/b.txt");
    }

    #[test]
    fn the_sidecar_itself_is_excluded() {
        let (files, _) = parse_set(b"data/a.txt\ndata/.set").unwrap();
        assert_eq!(files.len(), 1);
        assert_eq!(files[0].path, "a.txt");
    }

    #[test]
    fn mixed_directory_depths_mis_cut_as_in_the_c_sharp() {
        // Bug 2: startIndex comes from line 0 only. A shallower path later is
        // sliced at the wrong offset; a shorter one is dropped entirely.
        let (files, skipped) = parse_set(b"deep/nested/a.txt\nb.txt").unwrap();
        assert_eq!(files[0].path, "a.txt");
        // "b.txt" is 5 chars, startIndex is 12, so the C# skips it.
        assert_eq!(skipped, 1, "the second line vanishes rather than erroring");
        assert_eq!(files.len(), 1);
    }

    #[test]
    fn meta_all_compressed_is_a_bare_flag() {
        let m = parse_meta(b"AllCompressed");
        assert!(m.all_compressed);
        assert!(m.compressed.is_empty());
    }

    #[test]
    fn meta_sections_collect_their_entries() {
        let m = parse_meta(b"Params:\nkey=value\nother = two\nCompressed:\na.txt\nb.txt\nCrypted:\nc.txt");
        assert_eq!(
            m.params,
            vec![("key".to_string(), "value".to_string()), ("other".to_string(), "two".to_string())]
        );
        assert_eq!(m.compressed, vec!["a.txt", "b.txt"]);
        assert_eq!(m.crypted, vec!["c.txt"]);
    }

    #[test]
    fn a_header_ends_the_previous_section() {
        let m = parse_meta(b"Compressed:\na.txt\nCrypted:\nb.txt");
        assert_eq!(m.compressed, vec!["a.txt"]);
        assert_eq!(m.crypted, vec!["b.txt"]);
    }

    #[test]
    fn params_without_an_equals_are_skipped() {
        let m = parse_meta(b"Params:\nnoequals\nk=v");
        assert_eq!(m.params, vec![("k".to_string(), "v".to_string())]);
    }

    #[test]
    fn an_empty_meta_file_yields_defaults() {
        assert_eq!(parse_meta(b""), MetaSidecar::default());
    }

    #[test]
    fn meta_normalises_backslashes_too() {
        let m = parse_meta(b"Compressed:\nsub\\a.txt");
        assert_eq!(m.compressed, vec!["sub/a.txt"]);
    }
}
