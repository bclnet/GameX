// PORT-SOURCE: Core/GameX/Store.cs
// PORT-SHA: 38c2220e20a06fa7
// PORT-STATUS: done
//
// PARTIAL PORT: `Store_Blizzard` finds Battle.net's `product.db` but can't read
// it: the protobuf types are `BlizzardProtoDatabase.cs`, which isn't ported
// (see blizzard_proto_database.rs). Its `Paths` stays empty, so `Blizzard:`
// keys don't resolve and later keys are tried, as when Battle.net isn't installed.
//
// Each C# `static class Store_X` is a module: its `Paths` is a `LazyLock`, and
// its static constructor is `init`. A static constructor that throws makes C#
// throw `TypeInitializationException` on every later access; here `init`'s
// error is kept, and `paths` returns it each time. `Dictionary.Add` throwing on
// a duplicate key is `add`.
//
// .NET libraries and their stand-ins: `Microsoft.Win32.Registry` is `winreg`,
// SQLitePCL is `rusqlite`, `System.Text.Json` is `serde_json`, and `XDocument` +
// `XPathSelectElement` are `sxd_document` + `sxd_xpath`. `Environment.GetFolderPath`,
// `DriveInfo.GetDrives` and `RuntimeInformation` are the helpers at the bottom.
//
// Kept as C# has them (see rust/_/PORTING.md): the `"Query"` key for
// `Store_Archive`, `"ModelPath"` / `"Installed ModelPath"` in `Store_WinReg`, and
// `"Ubisoft Query Launcher"`.

use std::collections::HashMap;
use std::io::{self, Error, ErrorKind};
use std::path::Path;
use std::sync::LazyLock;

use indexmap::IndexMap;
use openx::platform::{platform_os, Os};
use openx::util::decodePath;
use serde_json::Value;

use crate::family::{applicationPath, getFullPath};

type Paths = LazyLock<Result<HashMap<String, String>, String>>;

/// C# reading a static class's field: rethrows its static constructor's error.
fn paths(s: &'static Paths) -> io::Result<&'static HashMap<String, String>> { s.as_ref().map_err(|e| Error::new(ErrorKind::Other, format!("TypeInitializationException: {e}"))) }

/// C# `Dictionary.Add`.
fn add(paths: &mut HashMap<String, String>, key: String, value: String) -> io::Result<()> {
    if paths.contains_key(&key) { return Err(Error::new(ErrorKind::InvalidInput, format!("An item with the same key has already been added. Key: {key}"))); }
    paths.insert(key, value);
    Ok(())
}

/// C# `Paths.TryGetValue(key, out var z) ? z : null`.
fn tryGetValue(paths: &HashMap<String, String>, key: Option<&str>) -> io::Result<Option<String>> {
    let key = key.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "ArgumentNullException: key"))?;
    Ok(paths.get(key).cloned())
}

/// Store
pub struct Store;

impl Store {
    pub fn getPathByKey(key: &str, family: &str, elem: &Value) -> io::Result<Option<String>> {
        //eprintln!("Abandon:{:?}", Store_Abandon::Paths());
        //eprintln!("Archive:{:?}", Store_Archive::Paths());
        //eprintln!("Blizzard:{:?}", Store_Blizzard::Paths());
        //eprintln!("Direct:{:?}", Store_Direct::getPathByKey(Some("%AppPath%")));
        //eprintln!("Epic:{:?}", Store_Epic::Paths());
        //eprintln!("Gog:{:?}", Store_Gog::Paths());
        //eprintln!("Local:{:?}", Store_Local::Paths());
        //eprintln!("Steam:{:?}", Store_Steam::Paths());
        //eprintln!("Ubisoft:{:?}", Store_Ubisoft::Paths());
        //eprintln!("WinReg:{:?}", Store_WinReg::getPathByKey(Some("GOG.com/Games/1207658680"), &Value::Null));
        let (k, v) = match key.split_once(':') { Some((k, v)) => (k, Some(v)), None => (key, None) };
        match k {
            "Steam" => tryGetValue(Store_Steam::Paths()?, v),
            "Gog" => tryGetValue(Store_Gog::Paths()?, v),
            "Blizzard" => tryGetValue(Store_Blizzard::Paths()?, v),
            "Epic" => tryGetValue(Store_Epic::Paths()?, v),
            "Ubisoft" => tryGetValue(Store_Ubisoft::Paths()?, v),
            "Abandon" => tryGetValue(Store_Abandon::Paths()?, Some(&format!("{family}/{}", v.unwrap_or_default()))),
            "Query" => tryGetValue(Store_Archive::Paths()?, Some(&format!("{family}/{}", v.unwrap_or_default()))),
            "WinReg" => Store_WinReg::getPathByKey(v, elem),
            "Local" => tryGetValue(Store_Local::Paths()?, v),
            "Direct" => Ok(Store_Direct::getPathByKey(v)),
            "Droid" => Ok(None),
            "Play" | "Xbox" => Ok(None),
            "x" | "Unknown" => Ok(None),
            _ => Err(Error::new(ErrorKind::InvalidInput, format!("key: {key}"))),
        }
    }
}

// #region Store_Abandon

/// Store_Abandon
pub mod Store_Abandon {
    use super::*;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> Option<String> { Some(r"E:\AbandonLibrary".into()) }
    fn init() -> io::Result<HashMap<String, String>> {
        let mut paths = HashMap::new();
        let root = getPath();
        let Some(root) = root.filter(|s| Path::new(s).is_dir()) else { return Ok(paths) };
        // # query games
        for s in enumerateDirectories(&root)? {
            for t in enumerateFiles(&s)? {
                add(&mut paths, format!("{}/{}", getFileName(&s), getFileName(&t)), t)?;
            }
        }
        Ok(paths)
    }
}

// #endregion

// #region Store_Archive

/// Store_Archive
pub mod Store_Archive {
    use super::*;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> Option<String> { Some(r"E:\ArchiveLibrary".into()) }
    fn init() -> io::Result<HashMap<String, String>> {
        let mut paths = HashMap::new();
        let root = getPath();
        let Some(root) = root.filter(|s| Path::new(s).is_dir()) else { return Ok(paths) };
        // # query games
        for s in enumerateDirectories(&root)? {
            for t in enumerateFiles(&s)? {
                add(&mut paths, format!("{}/{}", getFileName(&s), getFileName(&t)), t)?;
            }
            for t in enumerateDirectories(&s)? {
                if !getFileName(&t).starts_with('.') { add(&mut paths, format!("{}/{}", getFileName(&s), getFileName(&t)), t)?; }
            }
        }
        Ok(paths)
    }
}

// #endregion

// #region Store_Blizzard

/// Store_Blizzard
pub mod Store_Blizzard {
    use super::*;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> io::Result<Option<String>> {
        let paths: Vec<String> = match platform_os() {
            // windows paths
            Os::Windows => { let home = commonApplicationData(); vec![pathJoin(&[&home, "Battle.net", "Agent"])] }
            Os::Android => return Ok(None),
            // linux paths
            Os::Linux => { let home = userProfile(); [".steam", ".steam/steam", ".steam/root", ".local/share/Steam"].iter().map(|path| pathJoin(&[&home, path, "appcache"])).collect() }
            // mac paths
            Os::OSX => { let home = [userProfile(), "/Users/Shared".into()]; let search = ["Battle.net/Agent"]; home.iter().flat_map(|h| search.iter().map(move |s| pathJoin(&[h, s, "data"]))).collect() }
            _ => return Err(platformNotSupported()),
        };
        Ok(paths.into_iter().find(|s| Path::new(s).is_dir()))
    }
    fn init() -> io::Result<HashMap<String, String>> {
        let paths = HashMap::new();
        let root = getPath()?;
        let Some(root) = root else { return Ok(paths) };
        let dbPath = pathJoin(&[&root, "product.db"]);
        if !Path::new(&dbPath).is_file() { return Ok(paths) }

        // query games: needs BlizzardProtoDatabase.cs's `Database` and `ProductInstall`, not ported
        Ok(paths)
    }
}

// #endregion

// #region Store_Direct

/// Store_Direct
pub mod Store_Direct {
    pub fn getPathByKey(key: Option<&str>) -> Option<String> { key.map(str::to_string) }
}

// #endregion

// #region Store_Epic

/// Store_Epic
pub mod Store_Epic {
    use super::*;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> io::Result<Option<String>> {
        let paths: Vec<String> = match platform_os() {
            Os::Windows => { let home = commonApplicationData(); let search = [r"Epic\EpicGamesLauncher"]; search.iter().map(|path| pathJoin(&[&home, path, "Data"])).collect() }
            Os::Android => return Ok(None),
            Os::Linux => { let home = userProfile(); let search = ["Epic/EpicGamesLauncher"]; search.iter().map(|path| pathJoin(&[&home, path, "Data"])).collect() }
            Os::OSX => { let home = [userProfile(), "/Users/Shared".into()]; let search = ["Epic/EpicGamesLauncher"]; home.iter().flat_map(|h| search.iter().map(move |s| pathJoin(&[h, s, "Data"]))).collect() }
            _ => return Err(platformNotSupported()),
        };
        Ok(paths.into_iter().find(|s| Path::new(s).is_dir()))
    }
    fn init() -> io::Result<HashMap<String, String>> {
        let mut path = HashMap::new();
        let root = getPath()?;
        let Some(root) = root else { return Ok(path) };
        let dbPath = pathJoin(&[&root, "Manifests"]);
        if !Path::new(&dbPath).is_dir() { return Ok(path) }
        // # query games
        for s in enumerateFiles(&dbPath)?.into_iter().filter(|s| s.ends_with(".item")) {
            // add appPath if exists
            let elem: Value = serde_json::from_str(&std::fs::read_to_string(&s)?).map_err(|e| Error::new(ErrorKind::InvalidData, e))?;
            let appPath = elem.get("InstallLocation").ok_or_else(|| Error::new(ErrorKind::NotFound, "KeyNotFoundException: InstallLocation"))?.as_str();
            if let Some(appPath) = appPath.filter(|s| Path::new(s).is_dir()) { add(&mut path, getFileNameWithoutExtension(&s), appPath.to_string())?; }
        }
        Ok(path)
    }
}

// #endregion

// #region Store_Gog

/// Store_Gog
pub mod Store_Gog {
    use super::*;
    use rusqlite::Connection;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> io::Result<Option<String>> {
        let paths: Vec<String> = match platform_os() {
            Os::Windows => { let home = commonApplicationData(); let search = [r"GOG.com\Galaxy"]; search.iter().map(|path| pathJoin(&[&home, path, "storage"])).collect() }
            Os::Android => return Ok(None),
            Os::Linux => { let home = userProfile(); let search = ["??"]; search.iter().map(|path| pathJoin(&[&home, path, "Storage"])).collect() }
            Os::OSX => { let home = [userProfile(), "/Users/Shared".into()]; let search = ["GOG.com/Galaxy"]; home.iter().flat_map(|h| search.iter().map(move |s| pathJoin(&[h, s, "Storage"]))).collect() }
            _ => return Err(platformNotSupported()),
        };
        Ok(paths.into_iter().find(|s| Path::new(s).is_dir()))
    }
    fn init() -> io::Result<HashMap<String, String>> {
        let mut paths = HashMap::new();
        let root = getPath()?;
        let Some(root) = root else { return Ok(paths) };
        let dbPath = pathJoin(&[&root, "galaxy-2.0.db"]);
        if !Path::new(&dbPath).is_file() { return Ok(paths) }
        // query games
        let Ok(conn) = Connection::open(&dbPath) else { return Ok(paths) };
        let Ok(mut stmt) = conn.prepare("SELECT productId, installationPath FROM InstalledBaseProducts") else { return Ok(paths) };
        let Ok(rows) = stmt.query_map([], |row| Ok((row.get::<_, i32>(0)?, row.get::<_, String>(1)?))) else { return Ok(paths) };
        for row in rows {
            // C# loops on sqlite3_step until SQLITE_DONE; another result code loops forever. Here a failed step ends the query.
            let Ok((appId, appPath)) = row else { break };
            // add appPath if exists
            if Path::new(&appPath).is_dir() { add(&mut paths, appId.to_string(), appPath)?; }
        }
        Ok(paths)
    }
}

// #endregion

// #region Store_Local

/// Store_Local
pub mod Store_Local {
    use super::*;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    const GAMESPATH: &str = "Games";
    fn init() -> io::Result<HashMap<String, String>> {
        // get locale games
        let mut gameRoots: Vec<String> = getDrives().iter().map(|x| pathCombine(x, GAMESPATH)).collect();
        gameRoots.push(pathCombine(&userProfile(), GAMESPATH));
        if platform_os() == Os::Android { gameRoots.push(pathCombine("/sdcard", GAMESPATH)); }
        let mut paths = HashMap::new();
        for x in gameRoots.iter().filter(|s| Path::new(s).is_dir()) {
            for x in enumerateDirectories(x)? { add(&mut paths, getFileName(&x), x)?; }
        }
        Ok(paths)
    }
}

// #endregion

// #region Store_Steam

/// Store_Steam
pub mod Store_Steam {
    use super::*;

    #[derive(Debug, Clone, Default)]
    pub struct AcfStruct {
        pub Get: IndexMap<String, Option<AcfStruct>>,
        pub Value: IndexMap<String, String>,
    }

    impl AcfStruct {
        pub fn read(path: &str) -> io::Result<Option<AcfStruct>> { Ok(if Path::new(path).is_file() { Some(AcfStruct::new(&std::fs::read_to_string(path)?)?) } else { None }) }
        pub fn new(region: &str) -> io::Result<AcfStruct> {
            let mut s = AcfStruct::default();
            let indexOf = |c: u8, start: usize| region.as_bytes().get(start..).and_then(|z| z.iter().position(|&x| x == c)).map(|i| i + start);
            let (lengthOfRegion, mut index) = (region.len(), 0);
            while lengthOfRegion > index {
                let Some(firstStart) = indexOf(b'"', index) else { break };
                let firstEnd = indexOf(b'"', firstStart + 1).ok_or_else(outOfRange)?;
                index = firstEnd + 1;
                let first = region[firstStart + 1..firstEnd].to_string();
                let (secondStart, secondOpen) = (indexOf(b'"', index), indexOf(b'{', index));
                if secondStart.is_none() && secondOpen.is_none() {
                    s.Get.insert(first, None);
                }
                else if secondOpen.is_none() || (secondStart.is_some() && secondStart < secondOpen) {
                    let secondStart = secondStart.unwrap();
                    let secondEnd = indexOf(b'"', secondStart + 1).ok_or_else(outOfRange)?;
                    index = secondEnd + 1;
                    let second = &region[secondStart + 1..secondEnd];
                    if s.Value.contains_key(&first) { return Err(duplicate(&first)) }
                    s.Value.insert(first, second.replace(r"\\", r"\"));
                }
                else {
                    let secondOpen = secondOpen.unwrap();
                    let secondClose = Self::nextEndOf(region, b'{', b'}', secondOpen + 1)?;
                    let acfs = AcfStruct::new(&region[secondOpen + 1..secondClose])?;
                    index = secondClose + 1;
                    if s.Get.contains_key(&first) { return Err(duplicate(&first)) }
                    s.Get.insert(first, Some(acfs));
                }
            }
            Ok(s)
        }
        fn nextEndOf(str: &str, open: u8, close: u8, startIndex: usize) -> io::Result<usize> {
            if open == close { return Err(Error::new(ErrorKind::InvalidInput, "\"Open\" and \"Close\" char are equivalent!")) }
            let (mut openItem, mut closeItem) = (0, 0);
            for (i, &c) in str.as_bytes().iter().enumerate().skip(startIndex) {
                if c == open { openItem += 1; }
                if c == close {
                    closeItem += 1;
                    if closeItem > openItem { return Ok(i) }
                }
            }
            Err(Error::new(ErrorKind::InvalidData, "Not enough closing characters!"))
        }
        pub fn toString(&self, depth: usize) -> String {
            let mut b = String::new();
            for item in &self.Value {
                b.push_str(&"\t".repeat(depth));
                b.push_str(&format!("\"{}\"\t\t\"{}\"\r\n", item.0, item.1));
            }
            for item in &self.Get {
                b.push_str(&"\t".repeat(depth));
                b.push_str(&format!("\"{}\"\n", item.0));
                b.push_str(&"\t".repeat(depth));
                b.push_str("{\r\n");
                // C# calls item.Value.ToString on a null entry and throws
                if let Some(z) = item.1 { b.push_str(&z.toString(depth + 1)); }
                b.push_str(&"\t".repeat(depth));
                b.push_str("}\r\n");
            }
            b
        }
    }

    impl std::fmt::Display for AcfStruct {
        fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result { write!(f, "{}", self.toString(0)) }
    }

    fn outOfRange() -> Error { Error::new(ErrorKind::InvalidData, "ArgumentOutOfRangeException: length") }
    fn duplicate(key: &str) -> Error { Error::new(ErrorKind::InvalidInput, format!("An item with the same key has already been added. Key: {key}")) }
    fn get<'a>(s: &'a AcfStruct, key: &str) -> io::Result<&'a AcfStruct> { s.Get.get(key).ok_or_else(|| Error::new(ErrorKind::NotFound, format!("KeyNotFoundException: {key}")))?.as_ref().ok_or_else(nullReference) }
    fn value<'a>(s: &'a AcfStruct, key: &str) -> io::Result<&'a str> { s.Value.get(key).map(String::as_str).ok_or_else(|| Error::new(ErrorKind::NotFound, format!("KeyNotFoundException: {key}"))) }

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> io::Result<Option<String>> {
        let paths: Vec<String> = match platform_os() {
            // windows paths
            Os::Windows => {
                #[cfg(windows)] {
                    use winreg::{enums::*, RegKey};
                    let key = RegKey::predef(HKEY_CURRENT_USER).open_subkey(r"SOFTWARE\Valve\Steam")
                        .or_else(|_| RegKey::predef(HKEY_LOCAL_MACHINE).open_subkey_with_flags(r"SOFTWARE\Valve\Steam", KEY_READ | KEY_WOW64_64KEY));
                    let Ok(key) = key else { return Ok(None) };
                    return Ok(key.get_value::<String, _>("SteamPath").ok());
                }
                #[cfg(not(windows))] return Ok(None);
            }
            Os::Android => return Ok(None),
            // linux paths
            Os::Linux => { let home = userProfile(); let search = [".steam", ".steam/steam", ".steam/root", ".local/share/Steam"]; search.iter().map(|path| pathJoin(&[&home, path, "appcache"])).collect() }
            // mac paths
            Os::OSX => { let home = [userProfile(), "/Users/Shared".into()]; let search = ["Library/Application Support/Steam"]; home.iter().flat_map(|h| search.iter().map(move |s| pathJoin(&[h, s]))).collect() }
            _ => return Err(platformNotSupported()),
        };
        Ok(paths.into_iter().find(|s| Path::new(s).is_dir()))
    }
    fn init() -> io::Result<HashMap<String, String>> {
        let mut paths = HashMap::new();
        let root = getPath()?;
        let Some(root) = root else { return Ok(paths) };

        // query games
        let libraryFolders = AcfStruct::read(&pathJoin(&[&root, "steamapps", "libraryfolders.vdf"]))?.ok_or_else(nullReference)?;
        for folder in get(&libraryFolders, "libraryfolders")?.Get.values() {
            let folder = folder.as_ref().ok_or_else(nullReference)?;
            let path = value(folder, "path")?;
            if !Path::new(path).is_dir() { continue; }
            for appId in get(folder, "apps")?.Value.keys() {
                let Some(appManifest) = AcfStruct::read(&pathJoin(&[path, "steamapps", &format!("appmanifest_{appId}.acf")]))? else { continue };
                // add appPath if exists
                let appPath = pathJoin(&[path, "steamapps", &pathJoin(&["common", value(get(&appManifest, "AppState")?, "installdir")?])]);
                if Path::new(&appPath).is_dir() { add(&mut paths, appId.clone(), appPath)?; }
            }
        }
        Ok(paths)
    }
}

// #endregion

// #region Store_Ubisoft

/// Store_Ubisoft
pub mod Store_Ubisoft {
    use super::*;

    static _Paths: Paths = LazyLock::new(|| init().map_err(|e| e.to_string()));
    pub fn Paths() -> io::Result<&'static HashMap<String, String>> { paths(&_Paths) }
    fn getPath() -> io::Result<Option<String>> {
        let paths: Vec<String> = match platform_os() {
            Os::Windows => { let home = localApplicationData(); let search = ["Ubisoft Query Launcher"]; search.iter().map(|path| pathJoin(&[&home, path])).collect() }
            Os::Android => return Ok(None),
            Os::Linux => { let home = [userProfile(), "/Users/Shared".into()]; let search = ["??"]; home.iter().flat_map(|h| search.iter().map(move |s| pathJoin(&[h, s]))).collect() }
            Os::OSX => { let home = "/Users/Shared"; let search = ["??"]; search.iter().map(|path| pathJoin(&[home, path])).collect() }
            _ => return Err(platformNotSupported()),
        };
        Ok(paths.into_iter().find(|s| Path::new(s).is_dir()))
    }
    fn init() -> io::Result<HashMap<String, String>> {
        let mut paths = HashMap::new();
        let root = getPath()?;
        let Some(root) = root else { return Ok(paths) };
        let dbPath = pathJoin(&[&root, "settings.yaml"]);
        if !Path::new(&dbPath).is_file() { return Ok(paths) }
        // query games
        let body = std::fs::read_to_string(&dbPath)?;
        let (start, end) = (body.find("game_installation_path:").map_or(22, |i| i + 23), body.find("installer_cache_path"));
        let gamePath = end.and_then(|end| body.get(start..end)).ok_or_else(|| Error::new(ErrorKind::InvalidData, "ArgumentOutOfRangeException: settings.yaml"))?.trim();
        for s in enumerateDirectories(gamePath)? {
            add(&mut paths, getFileName(&s), s)?;
        }
        Ok(paths)
    }
}

// #endregion

// #region Store_WinReg

/// Store_WinReg
pub mod Store_WinReg {
    use super::*;

    pub fn getPathByKey(key: Option<&str>, elem: &Value) -> io::Result<Option<String>> {
        if platform_os() != Os::Windows { return Ok(None) }
        let key = key.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "ArgumentNullException: propertyName"))?;
        getPathByRegistryKey(key, elem.get(key))
    }

    /// Gets the executable path.
    #[cfg(windows)]
    fn findRegistryPath(paths: &[String]) -> Option<String> {
        use winreg::{enums::*, RegKey};
        let localMachine64 = RegKey::predef(HKEY_LOCAL_MACHINE);
        let currentUser64 = RegKey::predef(HKEY_CURRENT_USER);
        for p in paths {
            let keyPath = match p.strip_prefix('@') { Some(z) => z.replace('/', "\\"), None => format!("SOFTWARE\\{}", p.replace('/', "\\")) };
            let key = localMachine64.open_subkey_with_flags(&keyPath, KEY_READ | KEY_WOW64_64KEY)
                .or_else(|_| currentUser64.open_subkey_with_flags(&keyPath, KEY_READ | KEY_WOW64_64KEY))
                .or_else(|_| RegKey::predef(HKEY_CLASSES_ROOT).open_subkey(format!("VirtualStore\\MACHINE\\{keyPath}")));
            let Ok(key) = key else { continue };
            // search directories
            let mut path = ["ModelPath", "Install Dir", "InstallDir", "InstallLocation", ""].iter()
                .filter_map(|x| key.get_value::<String, _>(x).ok())
                .find(|x| !x.is_empty() && Path::new(x).is_dir());
            if path.is_none() {
                // search files
                path = ["Installed ModelPath", "ExePath", "Exe"].iter()
                    .filter_map(|x| key.get_value::<String, _>(x).ok())
                    .find(|x| !x.is_empty() && Path::new(x).is_file());
                if let Some(z) = path { path = Some(getDirectoryName(&z)); }
            }
            if let Some(path) = path.filter(|s| Path::new(s).is_dir()) { return Some(path) }
        }
        None
    }
    #[cfg(not(windows))]
    fn findRegistryPath(paths: &[String]) -> Option<String> { None }

    fn getPathByRegistryKey(key: &str, elem: Option<&Value>) -> io::Result<Option<String>> {
        let path = findRegistryPath(&[format!(r"Wow6432Node\{key}"), key.to_string()]);
        let Some(elem) = elem else { return Ok(path) };
        let str = |z: &Value| z.as_str().unwrap_or_default().to_string();
        Ok(if let Some(z) = elem.get("path") { Some(getFullPath(&decodePath(&applicationPath(), &str(z), path.as_deref()))) }
        else if let (Some(z), Some(y)) = (elem.get("xml"), elem.get("xmlPath")) { getSingleFileValue(&decodePath(&applicationPath(), &str(z), path.as_deref()), "xml", &str(y))? }
        else { None })
    }

    fn getSingleFileValue(path: &str, ext: &str, select: &str) -> io::Result<Option<String>> {
        if !Path::new(path).is_file() { return Ok(None) }
        let content = std::fs::read_to_string(path)?;
        let value = match ext {
            "xml" => {
                let package = sxd_document::parser::parse(&content).map_err(|e| Error::new(ErrorKind::InvalidData, format!("{e:?}")))?;
                let document = package.as_document();
                let value = sxd_xpath::evaluate_xpath(&document, select).map_err(|e| Error::new(ErrorKind::InvalidData, format!("{e:?}")))?;
                match value { sxd_xpath::Value::Nodeset(z) => z.document_order_first().map(|z| z.string_value()), _ => None }
            }
            _ => return Err(Error::new(ErrorKind::InvalidInput, "ext")),
        };
        Ok(value.map(|z| getDirectoryName(&z)))
    }
}

// #endregion

// C# `Environment.GetFolderPath`, `DriveInfo.GetDrives`, `RuntimeInformation` and `System.IO`.

fn platformNotSupported() -> Error { Error::new(ErrorKind::Unsupported, "PlatformNotSupportedException") }
fn nullReference() -> Error { Error::new(ErrorKind::InvalidData, "NullReferenceException") }

/// `SpecialFolder.CommonApplicationData`.
fn commonApplicationData() -> String { if cfg!(windows) { std::env::var("ProgramData").unwrap_or_else(|_| r"C:\ProgramData".into()) } else { "/usr/share".into() } }
/// `SpecialFolder.LocalApplicationData`.
fn localApplicationData() -> String { if cfg!(windows) { std::env::var("LOCALAPPDATA").unwrap_or_default() } else { pathJoin(&[&userProfile(), ".local/share"]) } }
/// `SpecialFolder.UserProfile`.
fn userProfile() -> String { std::env::var(if cfg!(windows) { "USERPROFILE" } else { "HOME" }).unwrap_or_default() }

/// `DriveInfo.GetDrives()`'s names. Off Windows, .NET lists every mount point; this lists `/`.
fn getDrives() -> Vec<String> {
    if cfg!(windows) { (b'A'..=b'Z').map(|c| format!("{}:\\", c as char)).filter(|s| Path::new(s).exists()).collect() }
    else { vec!["/".into()] }
}

fn enumerateDirectories(path: &str) -> io::Result<Vec<String>> { enumerate(path, true) }
fn enumerateFiles(path: &str) -> io::Result<Vec<String>> { enumerate(path, false) }
fn enumerate(path: &str, dirs: bool) -> io::Result<Vec<String>> {
    let mut s = Vec::new();
    for e in std::fs::read_dir(path)? { let e = e?; if e.file_type()?.is_dir() == dirs { s.push(e.path().to_string_lossy().into_owned()); } }
    Ok(s)
}

/// `Path.Join`: joins with a separator unless one is already there.
fn pathJoin(parts: &[&str]) -> String {
    let mut s = String::new();
    for p in parts.iter().filter(|p| !p.is_empty()) {
        if !s.is_empty() && !s.ends_with(['\\', '/']) && !p.starts_with(['\\', '/']) { s.push(std::path::MAIN_SEPARATOR); }
        s.push_str(p);
    }
    s
}
/// `Path.Combine`: as `Path.Join`, but a rooted part replaces what's before it.
fn pathCombine(a: &str, b: &str) -> String { if Path::new(b).has_root() { b.to_string() } else { pathJoin(&[a, b]) } }
fn getFileName(path: &str) -> String { path.rsplit(['\\', '/']).next().unwrap_or_default().to_string() }
fn getFileNameWithoutExtension(path: &str) -> String { let s = getFileName(path); match s.rfind('.') { Some(i) => s[..i].to_string(), None => s } }
fn getDirectoryName(path: &str) -> String { path.rfind(['\\', '/']).map_or(String::new(), |i| path[..i].to_string()) }

#[cfg(test)]
mod tests {
    use super::*;
    use super::Store_Steam::AcfStruct;

    #[test]
    fn unknownPrefixIsAnError() {
        // The C# throws ArgumentOutOfRangeException, which FamilyGame logs.
        assert!(Store::getPathByKey("Nope:x", "fam", &Value::Null).is_err());
    }

    #[test]
    fn unresolvableKindsReturnNone() {
        for k in ["Droid:x", "Play:x", "Xbox:x", "Unknown:x", "x:x"] {
            assert_eq!(Store::getPathByKey(k, "f", &Value::Null).unwrap(), None, "{k}");
        }
    }

    #[test]
    fn directReturnsTheKey() {
        assert_eq!(Store::getPathByKey("Direct:/opt/games/thing", "f", &Value::Null).unwrap().as_deref(), Some("/opt/games/thing"));
        assert_eq!(Store::getPathByKey("Direct", "f", &Value::Null).unwrap(), None);
    }

    #[test]
    fn acfStructParsesNestedSections() {
        let s = AcfStruct::new("\"libraryfolders\"\n{\n\t\"0\"\n\t{\n\t\t\"path\"\t\t\"C:\\\\Steam\"\n\t\t\"apps\"\n\t\t{\n\t\t\t\"2100\"\t\t\"123\"\n\t\t}\n\t}\n}\n").unwrap();
        let folder = s.Get["libraryfolders"].as_ref().unwrap().Get["0"].as_ref().unwrap();
        assert_eq!(folder.Value["path"], r"C:\Steam");
        assert_eq!(folder.Get["apps"].as_ref().unwrap().Value["2100"], "123");
    }

    #[test]
    fn acfStructRejectsDuplicateKeys() {
        assert!(AcfStruct::new("\"a\" \"1\" \"a\" \"2\"").is_err());
    }

    #[test]
    fn pathJoinAddsOneSeparator() {
        let sep = std::path::MAIN_SEPARATOR;
        assert_eq!(pathJoin(&["a", "b", "c"]), format!("a{sep}b{sep}c"));
        assert_eq!(pathJoin(&["a/", "b"]), "a/b");
    }
}
