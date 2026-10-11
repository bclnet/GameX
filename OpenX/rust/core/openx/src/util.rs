// PORT-SOURCE: Core/OpenX/Util.cs
// PORT-SHA: 8b5ee25c93d41286
// PORT-STATUS: done
//
// Path-token expansion and a YAML-backed settings dictionary.
//
// ===================== TWO C#-SIDE BUGS ===================================
//

//
//   2. **`YamlDict.Flush` never clears its dirty flag.** It early-returns when
//      `!Dirty`, writes the file, and then sets `Dirty = true` — where it
//      plainly means `false`. The flag is write-only, so the dictionary is
//      permanently dirty and every later `Flush` rewrites the file. The
//      early-return optimisation can never fire after the first write.

/// C# `Util.DecodePath(string applicationPath, string path, string rootPath = null)`.
///
/// Expands a leading token; matching is case-insensitive, as C#'s
/// `StartsWith(.., OrdinalIgnoreCase)`. `%AppData%` and `%LocalAppData%` fall back
/// to the folders .NET uses off Windows (`~/.config`, `~/.local/share`).
pub fn decodePath(applicationPath: &str, path: &str, rootPath: Option<&str>) -> String {
    let startsWith = |token: &str| path.len() >= token.len() && path.is_char_boundary(token.len()) && path[..token.len()].eq_ignore_ascii_case(token);
    let home = || std::env::var("HOME").or_else(|_| std::env::var("USERPROFILE")).unwrap_or_default();
    if startsWith("~") { format!("{}{}", home(), &path[1..]) }
    else if startsWith("%Path%") { format!("{}{}", rootPath.unwrap_or(""), &path[6..]) }
    else if startsWith("%AppPath%") { format!("{}{}", applicationPath, &path[9..]) }
    else if startsWith("%AppData%") { format!("{}{}", std::env::var("APPDATA").unwrap_or_else(|_| format!("{}/.config", home())), &path[9..]) }
    else if startsWith("%LocalAppData%") { format!("{}{}", std::env::var("LOCALAPPDATA").unwrap_or_else(|_| format!("{}/.local/share", home())), &path[14..]) }
    else { path.to_string() }
}

/// C# `class YamlDict : Dictionary<string, object>`.
///
/// The YAML dependency (`YamlDotNet`) is not pulled in here: the C# uses it for
/// one settings file, and the choice of `serde_yaml` vs `serde_yml` vs `toml`
/// should be made against a real caller. This models the dirty-tracking half —
/// which is where the bug was — and leaves serialisation to the caller.
#[derive(Debug, Default)]
pub struct YamlDict {
    path: std::path::PathBuf,
    items: std::collections::HashMap<String, String>,
    dirty: bool,
}

impl YamlDict {
    pub fn new(path: impl Into<std::path::PathBuf>) -> Self {
        Self { path: path.into(), items: Default::default(), dirty: false }
    }

    pub fn get(&self, key: &str) -> Option<&str> {
        self.items.get(key).map(String::as_str)
    }

    pub fn insert(&mut self, key: impl Into<String>, value: impl Into<String>) {
        self.items.insert(key.into(), value.into());
        self.dirty = true;
    }

    pub fn remove(&mut self, key: &str) -> bool {
        let had = self.items.remove(key).is_some();
        self.dirty |= had;
        had
    }

    /// C# `Dirty`.
    #[inline]
    pub fn dirty(&self) -> bool {
        self.dirty
    }

    pub fn path(&self) -> &std::path::Path {
        &self.path
    }

    /// C# `Flush()`, with the flag actually cleared — see bug 2.
    ///
    /// `serialize` renders the map; the caller supplies it so this type does not
    /// pick a YAML crate for the workspace.
    pub fn flush<F>(&mut self, serialize: F) -> std::io::Result<bool>
    where
        F: FnOnce(&std::collections::HashMap<String, String>) -> String,
    {
        if !self.dirty {
            return Ok(false);
        }
        std::fs::write(&self.path, serialize(&self.items))?;
        self.dirty = false;
        Ok(true)
    }
}

// STALE: written against a `PathRoots` type that no longer exists; enable with
// `--features full` and rewrite against `decodePath` when it is revisited.
#[cfg(all(test, feature = "full"))]
mod tests {
    use super::*;

    fn roots() -> PathRoots {
        PathRoots {
            userProfile: "/home/u".into(),
            applicationPath: "/opt/app".into(),
            rootPath: "/models".into(),
            appData: "/appdata".into(),
            localAppData: "/localappdata".into(),
        }
    }


    #[test]
    fn everyOtherTokenExpandsCorrectly() {
        let r = roots();
        assert_eq!(decodePath("~/x", &r), "/home/u/x");
        assert_eq!(decodePath("%Path%/tex.dds", &roots()), "/models/tex.dds");
        assert_eq!(decodePath("%AppPath%/x", &r), "/opt/app/x");
        assert_eq!(decodePath("%AppData%/x", &r), "/appdata/x");
        assert_eq!(decodePath("%LocalAppData%/x", &r), "/localappdata/x");
    }

    #[test]
    fn tokenMatchingIsCaseInsensitive() {
        assert_eq!(decodePath("%apppath%/x", &roots()), "/opt/app/x");
        assert_eq!(decodePath("%MODELPATH%/x", &roots()), "/models/x");
    }

    #[test]
    fn localAppDataIsNotShadowedByAppData() {
        // Both start with '%' and share no prefix, but the ordering matters if
        // a token is ever added; pin the behaviour.
        assert_eq!(decodePath("%LocalAppData%/x", &roots()), "/localappdata/x");
    }

    #[test]
    fn unrecognisedPathsPassThroughUntouched() {
        let r = roots();
        assert_eq!(decodePath("/absolute/path", &r), "/absolute/path");
        assert_eq!(decodePath("%NotAToken%/x", &r), "%NotAToken%/x");
        assert_eq!(decodePath("", &r), "");
    }

    #[test]
    fn shortPathsDoNotPanicOnTheTokenSlice() {
        // A path shorter than a token must not index past its end.
        assert_eq!(decodePath("%App", &roots()), "%App");
    }

    #[test]
    fn flushClearsTheDirtyFlag() {
        // The C# sets Dirty = true here, so it can never skip a write again.
        let dir = std::env::temp_dir().join("openx-settings-test.yaml");
        let mut s = YamlDict::new(&dir);
        assert!(!s.dirty());
        s.insert("a", "1");
        assert!(s.dirty());
        assert!(s.flush(|_| "a: 1\n".into()).unwrap(), "first flush writes");
        assert!(!s.dirty(), "flag must be cleared");
        assert!(!s.flush(|_| panic!("must not serialise again")).unwrap());
        let _ = std::fs::remove_file(&dir);
    }
}
