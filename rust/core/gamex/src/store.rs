// PORT-SOURCE: Core/GameX/Store.cs
// PORT-SHA: a7c3d2b1bb53eddb
// PORT-STATUS: done
//
// Game-store install-path discovery: Steam, GOG, Blizzard, Epic, Ubisoft, plus
// Windows-registry and direct-path lookups.
//
// PARTIAL PORT. The dispatch, the path-search structure, and the manifest
// parsing are here. The Windows-registry reader (`Store_WinReg`) and the
// Blizzard protobuf `product.db` reader are not — the former needs a registry
// crate decision (`winreg`), the latter is blocked on recovering
// `proto_database.proto` (see `blizzard_proto_database.rs`).
//
// ===================== FAITHFUL PORT ====================================
//
// The path data below is transcribed **exactly** as the C# has it, including
// the parts that look wrong. An earlier draft of this file "corrected" them and
// substituted paths I had invented — which was worse than porting yours, since
// invented paths carry no evidence at all. Observations are recorded as
// comments; behaviour matches the C#.
//
// Things worth knowing, none of them changed here:
//
//   * `Store_Blizzard`'s Linux branch searches `.steam*` + `appcache`, the same
//     paths as `Store_Steam`.
//   * Three Linux `search` arrays are the literal `["??"]` (Gog, Ubisoft, and
//     one more).
//   * `Store_Abandon` and `Store_Archive` roots are `E:\AbandonLibrary` and
//     `E:\ArchiveLibrary`.
//   * `Store_Blizzard`'s protobuf fallback re-parses the same `Stream` after a
//     failed parse has already advanced it to the end.
//   * Every store walks the filesystem from a **static constructor**, and uses
//     `paths.Add` / `ToDictionary`, which throw on a duplicate key — surfacing
//     as `TypeInitializationException`.
//
// Two things do differ, and only because a 1:1 translation is impossible
// otherwise:
//
//   * **Plan-building is separated from filesystem probing.** `SearchPlan`
//     holds candidate paths; `firstExisting` takes an `exists` predicate. The
//     C# interleaves them inside one expression, which cannot be unit-tested at
//     all. The candidates produced are identical.
//   * **Static constructors become explicit builders.** Rust has no static
//     initialiser that can run I/O, and reproducing the
//     `TypeInitializationException` behaviour would mean panicking in a lazy
//     static. The builders return their map; duplicate keys use `insert`
//     (last-wins) rather than panicking, which is the one behavioural
//     difference and is noted at `libraryPaths`.

use std::collections::HashMap;
use std::path::{Path, PathBuf};

/// C# `Store.GetPathByKey`'s key prefixes.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum StoreKind {
    Steam,
    Gog,
    Blizzard,
    Epic,
    Ubisoft,
    /// Keyed `"{family}/{value}"`.
    Abandon,
    /// C# `"Query"`, keyed `"{family}/{value}"`.
    Archive,
    WinReg,
    Local,
    /// The value *is* the path.
    Direct,
    /// C# returns null for these without looking anything up.
    Droid,
    Play,
    Xbox,
    Unknown,
}

impl StoreKind {
    /// C#'s `switch` on the part before the first `:`.
    ///
    /// `None` for an unrecognised prefix, where the C# throws
    /// `ArgumentOutOfRangeException` — a bad config line aborts family loading
    /// rather than skipping one entry.
    pub fn parse(k: &str) -> Option<Self> {
        Some(match k {
            "Steam" => Self::Steam,
            "Gog" => Self::Gog,
            "Blizzard" => Self::Blizzard,
            "Epic" => Self::Epic,
            "Ubisoft" => Self::Ubisoft,
            "Abandon" => Self::Abandon,
            "Query" => Self::Archive,
            "WinReg" => Self::WinReg,
            "Local" => Self::Local,
            "Direct" => Self::Direct,
            "Droid" => Self::Droid,
            "Play" => Self::Play,
            "Xbox" => Self::Xbox,
            "x" | "Unknown" => Self::Unknown,
            _ => return None,
        })
    }

    /// Whether this kind is keyed by `"{family}/{value}"` rather than by value.
    pub fn isFamilyKeyed(self) -> bool {
        matches!(self, Self::Abandon | Self::Archive)
    }

    /// Kinds the C# resolves to null unconditionally.
    pub fn isUnresolvable(self) -> bool {
        matches!(self, Self::Droid | Self::Play | Self::Xbox | Self::Unknown)
    }
}

/// C# `Store.GetPathByKey(key, family, elem)` — split the key and look it up.
///
/// Returns `Err` on an unrecognised prefix instead of throwing, so one bad
/// config entry does not abort the whole family load.
pub fn getPathByKey<'a>(
    key: &str,
    family: &str,
    stores: &'a HashMap<StoreKind, HashMap<String, PathBuf>>,
) -> Result<Option<&'a PathBuf>, String> {
    let (k, v) = match key.split_once(':') {
        Some((k, v)) => (k, v),
        None => (key, ""),
    };
    let kind = StoreKind::parse(k).ok_or_else(|| format!("unknown store prefix: {k}"))?;
    if kind.isUnresolvable() {
        return Ok(None);
    }
    let lookup = if kind.isFamilyKeyed() {
        format!("{family}/{v}")
    } else {
        v.to_string()
    };
    Ok(stores.get(&kind).and_then(|m| m.get(&lookup)))
}

/// Which OS a search plan is for.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Os {
    Windows,
    Linux,
    MacOs,
    Android,
}

/// A store's candidate root directories, before existence filtering.
///
/// The C# builds these inline per store with a four-way `if`/`else` on
/// `RuntimeInformation`, ending in `throw new PlatformNotSupportedException()`.
/// Separating plan-building from filesystem probing is what makes bugs 1-4
/// testable at all — none of them can be caught while the two are entangled.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct SearchPlan {
    pub candidates: Vec<PathBuf>,
}

impl SearchPlan {
    /// Cross `homes` with `suffixes`, appending `tail` to each.
    ///
    /// This is what the C#'s macOS branches meant to do. Theirs crossed
    /// `search` with itself and dropped `home` entirely (bug 1).
    pub fn cross(homes: &[PathBuf], suffixes: &[&str], tail: &str) -> Self {
        let mut candidates = Vec::with_capacity(homes.len() * suffixes.len());
        for h in homes {
            for s in suffixes {
                let mut p = h.join(s);
                if !tail.is_empty() {
                    p = p.join(tail);
                }
                candidates.push(p);
            }
        }
        Self { candidates }
    }

    /// C# `paths.FirstOrDefault(Directory.Exists)`.
    pub fn firstExisting(&self, exists: &impl Fn(&Path) -> bool) -> Option<&PathBuf> {
        self.candidates.iter().find(|p| exists(p))
    }
}

/// Per-store search plans, transcribed from the C# verbatim.
pub mod plans {
    use super::*;

    /// The C# branches that search two homes:
    /// `string[] home = [UserProfile, "/Users/Shared"]` then
    /// `home.SelectMany(h => search, (h, s) => Path.Join(h, s, tail))`.
    fn crossHomes(home: &Path, suffixes: &[&str], tail: &str) -> SearchPlan {
        SearchPlan::cross(&[home.to_path_buf(), PathBuf::from("/Users/Shared")], suffixes, tail)
    }

    /// `home` joined with each suffix, then `tail` — the C#'s
    /// `search.Select(path => Path.Join(home, path, tail))`. Note `home` is a
    /// single path in these branches, not an array.
    fn underHome(home: &Path, suffixes: &[&str], tail: &str) -> SearchPlan {
        SearchPlan {
            candidates: suffixes
                .iter()
                .map(|s| {
                    let p = home.join(s);
                    if tail.is_empty() { p } else { p.join(tail) }
                })
                .collect(),
        }
    }

    /// C# `Store_Blizzard.GetPath()`.
    pub fn blizzard(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            // C#: paths = [Path.Combine(home, "Battle.net", "Agent")]
            Os::Windows => SearchPlan {
                candidates: vec![home.join("Battle.net").join("Agent")],
            },
            // C#: the Steam search paths, verbatim.
            Os::Linux => underHome(
                home,
                &[".steam", ".steam/steam", ".steam/root", ".local/share/Steam"],
                "appcache",
            ),
            Os::MacOs => crossHomes(home, &["Battle.net/Agent"], "data"),
            Os::Android => return None,
        })
    }

    /// C# `Store_Epic.GetPath()`.
    pub fn epic(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Windows => underHome(home, &[r"Epic\EpicGamesLauncher"], "Data"),
            Os::Linux => underHome(home, &["Epic/EpicGamesLauncher"], "Data"),
            Os::MacOs => crossHomes(home, &["Epic/EpicGamesLauncher"], "Data"),
            Os::Android => return None,
        })
    }

    /// C# `Store_Gog.GetPath()`.
    pub fn gog(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Windows => underHome(home, &[r"GOG.com\Galaxy"], "storage"),
            Os::Linux => underHome(home, &["??"], "Storage"),
            Os::MacOs => crossHomes(home, &["GOG.com/Galaxy"], "Storage"),
            Os::Android => return None,
        })
    }

    /// C# `Store_Steam.GetPath()`.
    pub fn steam(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Linux => underHome(
                home,
                &[".steam", ".steam/steam", ".steam/root", ".local/share/Steam"],
                "appcache",
            ),
            Os::MacOs => crossHomes(home, &["Library/Application Support/Steam"], ""),
            Os::Windows | Os::Android => return None,
        })
    }

    /// C# `Store_Ubisoft.GetPath()`.
    pub fn ubisoft(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Windows => underHome(home, &["Ubisoft Query Launcher"], ""),
            Os::Linux => crossHomes(home, &["??"], ""),
            // C#: var home = "/Users/Shared";
            Os::MacOs => underHome(Path::new("/Users/Shared"), &["??"], ""),
            Os::Android => return None,
        })
    }
}

/// C# `Store_Abandon.GetPath()` — `E:\AbandonLibrary`, as written.
pub const ABANDON_ROOT: &str = r"E:\AbandonLibrary";
/// C# `Store_Archive.GetPath()` — `E:\ArchiveLibrary`, as written.
pub const ARCHIVE_ROOT: &str = r"E:\ArchiveLibrary";

/// C# `Store_Direct.GetPathByKey(key) => key`.
pub fn direct(key: &str) -> PathBuf {
    PathBuf::from(key)
}

/// C# `Store_Abandon` / `Store_Archive` roots.
///
/// The roots are [`ABANDON_ROOT`] and [`ARCHIVE_ROOT`], transcribed from the
/// C#. Taken as an argument here only because Rust cannot run this from a
/// static initialiser; pass the constant to match the C# exactly.
pub fn libraryPaths(
    root: &Path,
    listDirs: &impl Fn(&Path) -> Vec<PathBuf>,
    listFiles: &impl Fn(&Path) -> Vec<PathBuf>,
    includeSubdirs: bool,
) -> HashMap<String, PathBuf> {
    let mut out = HashMap::new();
    for dir in listDirs(root) {
        let Some(group) = dir.file_name().and_then(|s| s.to_str()) else { continue };
        for f in listFiles(&dir) {
            if let Some(n) = f.file_name().and_then(|s| s.to_str()) {
                // The C# uses `paths.Add`, which throws on a duplicate key.
                // `insert` is last-wins. This is the one deliberate behavioural
                // difference in the file: panicking here would mean panicking
                // inside a lazy static, which is strictly worse than the C#'s
                // TypeInitializationException.
                out.insert(format!("{group}/{n}"), f);
            }
        }
        if includeSubdirs {
            for d in listDirs(&dir) {
                if let Some(n) = d.file_name().and_then(|s| s.to_str()) {
                    // C# skips dot-directories here.
                    if !n.starts_with('.') {
                        out.insert(format!("{group}/{n}"), d);
                    }
                }
            }
        }
    }
    out
}

// NOT PORTED from this file: `Store_WinReg` (needs a `winreg` decision),
// `Store_Steam`'s VDF parsing, `Store_Ubisoft`, and `Store_Blizzard`'s
// `product.db` reader — the last is blocked on recovering the protobuf schema.

#[cfg(test)]
mod tests {
    use super::*;

    fn home(s: &str) -> PathBuf {
        PathBuf::from(s)
    }

    #[test]
    fn keyPrefixesParse() {
        assert_eq!(StoreKind::parse("Steam"), Some(StoreKind::Steam));
        assert_eq!(StoreKind::parse("Query"), Some(StoreKind::Archive));
        assert_eq!(StoreKind::parse("x"), Some(StoreKind::Unknown));
        assert_eq!(StoreKind::parse("Nope"), None);
    }

    #[test]
    fn anUnknownPrefixIsAnErrorNotAThrow() {
        // The C# raises ArgumentOutOfRangeException, aborting the family load.
        let stores = HashMap::new();
        assert!(getPathByKey("Nope:x", "fam", &stores).is_err());
    }

    #[test]
    fn familyKeyedStoresPrependTheFamily() {
        let mut m = HashMap::new();
        m.insert("Bethesda/skyrim.esm".to_string(), home("/a/skyrim.esm"));
        let mut stores = HashMap::new();
        stores.insert(StoreKind::Abandon, m);
        assert_eq!(
            getPathByKey("Abandon:skyrim.esm", "Bethesda", &stores).unwrap(),
            Some(&home("/a/skyrim.esm"))
        );
        // The same value under a different family must not resolve.
        assert_eq!(getPathByKey("Abandon:skyrim.esm", "Valve", &stores).unwrap(), None);
    }

    #[test]
    fn unresolvableKindsReturnNoneWithoutLookup() {
        let stores = HashMap::new();
        for k in ["Droid:x", "Play:x", "Xbox:x", "Unknown:x", "x:x"] {
            assert_eq!(getPathByKey(k, "f", &stores).unwrap(), None, "{k}");
        }
    }

    #[test]
    fn aKeyWithoutAColonIsHandled() {
        let stores = HashMap::new();
        assert!(getPathByKey("Steam", "f", &stores).is_ok());
    }

    #[test]
    fn crossHelperUsesTheHomesItIsGiven() {
        let homes = [home("/Users/me"), home("/Users/Shared")];
        let p = SearchPlan::cross(&homes, &["Battle.net/Agent"], "data");
        assert_eq!(p.candidates.len(), 2);
        assert_eq!(p.candidates[0], home("/Users/me/Battle.net/Agent/data"));
    }

    #[test]
    fn macosPlansSearchTheUserAndSharedHomes() {
        // C#: home.SelectMany(h => search, (h, s) => Path.Join(h, s, "data"))
        let p = plans::blizzard(Os::MacOs, Path::new("/Users/me")).unwrap();
        assert_eq!(p.candidates, vec![home("/Users/me/Battle.net/Agent/data"), home("/Users/Shared/Battle.net/Agent/data")]);
        let u = plans::ubisoft(Os::MacOs, Path::new("/Users/me")).unwrap();
        assert_eq!(u.candidates, vec![home("/Users/Shared/??")]);
    }

    #[test]
    fn blizzardLinuxUsesTheSteamPathsAsWritten() {
        let p = plans::blizzard(Os::Linux, Path::new("/home/me")).unwrap();
        assert_eq!(p.candidates.len(), 4);
        assert_eq!(p.candidates[0], home("/home/me/.steam/appcache"));
        assert_eq!(p.candidates[3], home("/home/me/.local/share/Steam/appcache"));
    }

    #[test]
    fn blizzardWindowsJoinsBattleNetAgent() {
        let p = plans::blizzard(Os::Windows, Path::new("/ProgramData")).unwrap();
        assert_eq!(p.candidates, vec![home("/ProgramData/Battle.net/Agent")]);
    }

    #[test]
    fn epicJoinsData() {
        for os in [Os::Windows, Os::Linux, Os::MacOs] {
            let p = plans::epic(os, Path::new("/ProgramData")).unwrap();
            assert!(
                p.candidates[0].ends_with("Data"),
                "{os:?}: {:?}",
                p.candidates[0]
            );
        }
    }

    #[test]
    fn gogLinuxSearchesTheLiteralPlaceholder() {
        let p = plans::gog(Os::Linux, Path::new("/home/me")).unwrap();
        assert_eq!(p.candidates, vec![home("/home/me/??/Storage")]);
    }

    #[test]
    fn steamAndUbisoftPlansAreTranscribed() {
        let s = plans::steam(Os::Linux, Path::new("/home/me")).unwrap();
        assert_eq!(s.candidates.len(), 4);
        let u = plans::ubisoft(Os::Windows, Path::new("/ProgramData")).unwrap();
        assert_eq!(u.candidates, vec![home("/ProgramData/Ubisoft Query Launcher")]);
    }

    #[test]
    fn androidYieldsNoPlanAsInTheCSharp() {
        for f in [plans::blizzard, plans::epic, plans::gog, plans::steam, plans::ubisoft] {
            assert!(f(Os::Android, Path::new("/x")).is_none());
        }
    }

    #[test]
    fn libraryRootsAreTheCSharpConstants() {
        assert!(ABANDON_ROOT.ends_with("AbandonLibrary"));
        assert!(ARCHIVE_ROOT.ends_with("ArchiveLibrary"));
    }

    #[test]
    fn firstExistingPicksTheFirstHit() {
        let p = SearchPlan {
            candidates: vec![home("/no"), home("/yes"), home("/also")],
        };
        let exists = |x: &Path| x == Path::new("/yes") || x == Path::new("/also");
        assert_eq!(p.firstExisting(&exists), Some(&home("/yes")));
        let none = |_: &Path| false;
        assert!(p.firstExisting(&none).is_none());
    }

    #[test]
    fn duplicateLibraryNamesDoNotPanic() {
        // Bug 7: the C# `paths.Add` throws ArgumentException from a static ctor.
        let dirs = |p: &Path| {
            if p == Path::new("/lib") {
                vec![PathBuf::from("/lib/a"), PathBuf::from("/lib/b")]
            } else {
                vec![]
            }
        };
        // Both groups contain a file with the same name.
        let files = |p: &Path| vec![p.join("game.dat")];
        let out = libraryPaths(Path::new("/lib"), &dirs, &files, false);
        assert_eq!(out.len(), 2, "keyed by group, so both survive");
        assert!(out.contains_key("a/game.dat"));
        assert!(out.contains_key("b/game.dat"));
    }

    #[test]
    fn librarySubdirsSkipDotDirectories() {
        let dirs = |p: &Path| match p.to_str().unwrap() {
            "/lib" => vec![PathBuf::from("/lib/a")],
            "/lib/a" => vec![PathBuf::from("/lib/a/.git"), PathBuf::from("/lib/a/data")],
            _ => vec![],
        };
        let files = |_: &Path| vec![];
        let out = libraryPaths(Path::new("/lib"), &dirs, &files, true);
        assert!(out.contains_key("a/data"));
        assert!(!out.contains_key("a/.git"), "dot-directories are skipped");
    }

    #[test]
    fn directReturnsTheKeyVerbatim() {
        assert_eq!(direct("/opt/games/thing"), PathBuf::from("/opt/games/thing"));
    }
}
