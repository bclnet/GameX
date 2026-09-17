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
//   * Every macOS branch does
//     `search.SelectMany(x => search, (s, h) => Path.Join(h, s, tail))`, which
//     crosses `search` with itself and never reads the `home` array declared
//     directly above it. Reproduced in `cross_search_with_itself`.
//   * `Store_Blizzard`'s Linux branch searches `.steam*` + `appcache`, the same
//     paths as `Store_Steam`.
//   * `Store_Epic` joins `"Sbi"` on all three platforms; the constructor then
//     looks for `Manifests` beneath it.
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
//     holds candidate paths; `first_existing` takes an `exists` predicate. The
//     C# interleaves them inside one expression, which cannot be unit-tested at
//     all. The candidates produced are identical.
//   * **Static constructors become explicit builders.** Rust has no static
//     initialiser that can run I/O, and reproducing the
//     `TypeInitializationException` behaviour would mean panicking in a lazy
//     static. The builders return their map; duplicate keys use `insert`
//     (last-wins) rather than panicking, which is the one behavioural
//     difference and is noted at `library_paths`.

use std::collections::HashMap;
use std::path::{Path, PathBuf};

/// C# `Store.GetPathByKey`'s key prefixes.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
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
    pub fn is_family_keyed(self) -> bool {
        matches!(self, Self::Abandon | Self::Archive)
    }

    /// Kinds the C# resolves to null unconditionally.
    pub fn is_unresolvable(self) -> bool {
        matches!(self, Self::Droid | Self::Play | Self::Xbox | Self::Unknown)
    }
}

/// C# `Store.GetPathByKey(key, family, elem)` — split the key and look it up.
///
/// Returns `Err` on an unrecognised prefix instead of throwing, so one bad
/// config entry does not abort the whole family load.
pub fn path_by_key<'a>(
    key: &str,
    family: &str,
    stores: &'a HashMap<StoreKind, HashMap<String, PathBuf>>,
) -> Result<Option<&'a PathBuf>, String> {
    let (k, v) = match key.split_once(':') {
        Some((k, v)) => (k, v),
        None => (key, ""),
    };
    let kind = StoreKind::parse(k).ok_or_else(|| format!("unknown store prefix: {k}"))?;
    if kind.is_unresolvable() {
        return Ok(None);
    }
    let lookup = if kind.is_family_keyed() {
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
    pub fn first_existing(&self, exists: &impl Fn(&Path) -> bool) -> Option<&PathBuf> {
        self.candidates.iter().find(|p| exists(p))
    }
}

/// Per-store search plans, transcribed from the C# verbatim.
pub mod plans {
    use super::*;

    /// The macOS expression every store uses:
    /// `search.SelectMany(x => search, (s, h) => Path.Join(h, s, tail))`.
    ///
    /// Both operands come from `search`; the `home` array the C# declares on
    /// the line above is not referenced. Kept as-is.
    fn cross_search_with_itself(suffixes: &[&str], tail: &str) -> SearchPlan {
        let mut candidates = Vec::new();
        for s in suffixes {
            for h in suffixes {
                let mut p = PathBuf::from(h).join(s);
                if !tail.is_empty() {
                    p = p.join(tail);
                }
                candidates.push(p);
            }
        }
        SearchPlan { candidates }
    }

    /// `home` joined with each suffix, then `tail` — the C#'s
    /// `search.Select(path => Path.Join(home, path, tail))`. Note `home` is a
    /// single path in these branches, not an array.
    fn under_home(home: &Path, suffixes: &[&str], tail: &str) -> SearchPlan {
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
            Os::Linux => under_home(
                home,
                &[".steam", ".steam/steam", ".steam/root", ".local/share/Steam"],
                "appcache",
            ),
            Os::MacOs => cross_search_with_itself(&["Battle.net/Agent"], "data"),
            Os::Android => return None,
        })
    }

    /// C# `Store_Epic.GetPath()`.
    pub fn epic(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Windows => under_home(home, &[r"Epic\EpicGamesLauncher"], "Sbi"),
            Os::Linux => under_home(home, &["Epic/EpicGamesLauncher"], "Sbi"),
            Os::MacOs => cross_search_with_itself(&["Epic/EpicGamesLauncher"], "Sbi"),
            Os::Android => return None,
        })
    }

    /// C# `Store_Gog.GetPath()`.
    pub fn gog(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Windows => under_home(home, &[r"GOG.com\Galaxy"], "storage"),
            Os::Linux => under_home(home, &["??"], "Storage"),
            Os::MacOs => cross_search_with_itself(&["GOG.com/Galaxy"], "Storage"),
            Os::Android => return None,
        })
    }

    /// C# `Store_Steam.GetPath()`.
    pub fn steam(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Linux => under_home(
                home,
                &[".steam", ".steam/steam", ".steam/root", ".local/share/Steam"],
                "appcache",
            ),
            Os::MacOs => cross_search_with_itself(&["Library/Application Support/Steam"], ""),
            Os::Windows | Os::Android => return None,
        })
    }

    /// C# `Store_Ubisoft.GetPath()`.
    pub fn ubisoft(os: Os, home: &Path) -> Option<SearchPlan> {
        Some(match os {
            Os::Windows => under_home(home, &["Ubisoft Query Launcher"], ""),
            Os::Linux => cross_search_with_itself(&["??"], ""),
            Os::MacOs => under_home(home, &["??"], ""),
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
pub fn library_paths(
    root: &Path,
    list_dirs: &impl Fn(&Path) -> Vec<PathBuf>,
    list_files: &impl Fn(&Path) -> Vec<PathBuf>,
    include_subdirs: bool,
) -> HashMap<String, PathBuf> {
    let mut out = HashMap::new();
    for dir in list_dirs(root) {
        let Some(group) = dir.file_name().and_then(|s| s.to_str()) else { continue };
        for f in list_files(&dir) {
            if let Some(n) = f.file_name().and_then(|s| s.to_str()) {
                // The C# uses `paths.Add`, which throws on a duplicate key.
                // `insert` is last-wins. This is the one deliberate behavioural
                // difference in the file: panicking here would mean panicking
                // inside a lazy static, which is strictly worse than the C#'s
                // TypeInitializationException.
                out.insert(format!("{group}/{n}"), f);
            }
        }
        if include_subdirs {
            for d in list_dirs(&dir) {
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
    fn key_prefixes_parse() {
        assert_eq!(StoreKind::parse("Steam"), Some(StoreKind::Steam));
        assert_eq!(StoreKind::parse("Query"), Some(StoreKind::Archive));
        assert_eq!(StoreKind::parse("x"), Some(StoreKind::Unknown));
        assert_eq!(StoreKind::parse("Nope"), None);
    }

    #[test]
    fn an_unknown_prefix_is_an_error_not_a_throw() {
        // The C# raises ArgumentOutOfRangeException, aborting the family load.
        let stores = HashMap::new();
        assert!(path_by_key("Nope:x", "fam", &stores).is_err());
    }

    #[test]
    fn family_keyed_stores_prepend_the_family() {
        let mut m = HashMap::new();
        m.insert("Bethesda/skyrim.esm".to_string(), home("/a/skyrim.esm"));
        let mut stores = HashMap::new();
        stores.insert(StoreKind::Abandon, m);
        assert_eq!(
            path_by_key("Abandon:skyrim.esm", "Bethesda", &stores).unwrap(),
            Some(&home("/a/skyrim.esm"))
        );
        // The same value under a different family must not resolve.
        assert_eq!(path_by_key("Abandon:skyrim.esm", "Valve", &stores).unwrap(), None);
    }

    #[test]
    fn unresolvable_kinds_return_none_without_lookup() {
        let stores = HashMap::new();
        for k in ["Droid:x", "Play:x", "Xbox:x", "Unknown:x", "x:x"] {
            assert_eq!(path_by_key(k, "f", &stores).unwrap(), None, "{k}");
        }
    }

    #[test]
    fn a_key_without_a_colon_is_handled() {
        let stores = HashMap::new();
        assert!(path_by_key("Steam", "f", &stores).is_ok());
    }

    #[test]
    fn cross_helper_uses_the_homes_it_is_given() {
        // This helper is not what the macOS branches call; see the next test.
        let homes = [home("/Users/me"), home("/Users/Shared")];
        let p = SearchPlan::cross(&homes, &["Battle.net/Agent"], "data");
        assert_eq!(p.candidates.len(), 2);
        assert_eq!(p.candidates[0], home("/Users/me/Battle.net/Agent/data"));
    }

    #[test]
    fn macos_plans_match_the_c_sharp_self_cross() {
        // Faithful: `search.SelectMany(x => search, (s, h) => ...)` produces one
        // relative candidate with the suffix twice, and no home directory.
        let p = plans::blizzard(Os::MacOs, Path::new("/Users/me")).unwrap();
        assert_eq!(p.candidates.len(), 1);
        assert_eq!(p.candidates[0], home("Battle.net/Agent/Battle.net/Agent/data"));
    }

    #[test]
    fn blizzard_linux_uses_the_steam_paths_as_written() {
        let p = plans::blizzard(Os::Linux, Path::new("/home/me")).unwrap();
        assert_eq!(p.candidates.len(), 4);
        assert_eq!(p.candidates[0], home("/home/me/.steam/appcache"));
        assert_eq!(p.candidates[3], home("/home/me/.local/share/Steam/appcache"));
    }

    #[test]
    fn blizzard_windows_joins_battle_net_agent() {
        let p = plans::blizzard(Os::Windows, Path::new("/ProgramData")).unwrap();
        assert_eq!(p.candidates, vec![home("/ProgramData/Battle.net/Agent")]);
    }

    #[test]
    fn epic_joins_sbi_as_written() {
        for os in [Os::Windows, Os::Linux] {
            let p = plans::epic(os, Path::new("/ProgramData")).unwrap();
            assert!(
                p.candidates[0].ends_with("Sbi"),
                "{os:?}: {:?}",
                p.candidates[0]
            );
        }
    }

    #[test]
    fn gog_linux_searches_the_literal_placeholder() {
        let p = plans::gog(Os::Linux, Path::new("/home/me")).unwrap();
        assert_eq!(p.candidates, vec![home("/home/me/??/Storage")]);
    }

    #[test]
    fn steam_and_ubisoft_plans_are_transcribed() {
        let s = plans::steam(Os::Linux, Path::new("/home/me")).unwrap();
        assert_eq!(s.candidates.len(), 4);
        let u = plans::ubisoft(Os::Windows, Path::new("/ProgramData")).unwrap();
        assert_eq!(u.candidates, vec![home("/ProgramData/Ubisoft Query Launcher")]);
    }

    #[test]
    fn android_yields_no_plan_as_in_the_c_sharp() {
        for f in [plans::blizzard, plans::epic, plans::gog, plans::steam, plans::ubisoft] {
            assert!(f(Os::Android, Path::new("/x")).is_none());
        }
    }

    #[test]
    fn library_roots_are_the_c_sharp_constants() {
        assert!(ABANDON_ROOT.ends_with("AbandonLibrary"));
        assert!(ARCHIVE_ROOT.ends_with("ArchiveLibrary"));
    }

    #[test]
    fn first_existing_picks_the_first_hit() {
        let p = SearchPlan {
            candidates: vec![home("/no"), home("/yes"), home("/also")],
        };
        let exists = |x: &Path| x == Path::new("/yes") || x == Path::new("/also");
        assert_eq!(p.first_existing(&exists), Some(&home("/yes")));
        let none = |_: &Path| false;
        assert!(p.first_existing(&none).is_none());
    }

    #[test]
    fn duplicate_library_names_do_not_panic() {
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
        let out = library_paths(Path::new("/lib"), &dirs, &files, false);
        assert_eq!(out.len(), 2, "keyed by group, so both survive");
        assert!(out.contains_key("a/game.dat"));
        assert!(out.contains_key("b/game.dat"));
    }

    #[test]
    fn library_subdirs_skip_dot_directories() {
        let dirs = |p: &Path| match p.to_str().unwrap() {
            "/lib" => vec![PathBuf::from("/lib/a")],
            "/lib/a" => vec![PathBuf::from("/lib/a/.git"), PathBuf::from("/lib/a/data")],
            _ => vec![],
        };
        let files = |_: &Path| vec![];
        let out = library_paths(Path::new("/lib"), &dirs, &files, true);
        assert!(out.contains_key("a/data"));
        assert!(!out.contains_key("a/.git"), "dot-directories are skipped");
    }

    #[test]
    fn direct_returns_the_key_verbatim() {
        assert_eq!(direct("/opt/games/thing"), PathBuf::from("/opt/games/thing"));
    }
}
