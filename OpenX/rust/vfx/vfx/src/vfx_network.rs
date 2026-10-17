// PORT-SOURCE: Vfx/OpenX.Vfx/Vfx_Network.cs
// PORT-SHA: 924d62f99a065206
// PORT-STATUS: done
//
// `NetworkHost` is `HttpClient` plus `MemoryCache`. The client is blocking
// `ureq` (no tokio). The timeout is 30 minutes, as C#.
//
// `UriBuilder.Path = address.LocalPath + folder + "/"` concatenates with no
// extra slash, so `/files` and `a` become `/filesa/`. This port does the same.
//
// `GetSetAsync` and `GetFileAsync` take `shouldThrow` and never pass it to
// `CallAsync`, whose default is false. A failed status is cached as empty.
// A transport error still returns `Err`. `FromBytes` only accepts a byte
// stream and a case-insensitive set of ASCII lines. An empty first line makes
// `Path.GetDirectoryName` null in C# (`NullReferenceException`); here that is
// `InvalidData`.
//
// Family does not call this for an http archive. `GetArchive` throws
// `NotImplementedException` while the vfx is a `NetworkFileSystem`, and the
// `StreamArchive` line in C# is commented out.

use std::collections::HashMap;
use std::io::{self, Error, ErrorKind, Read};
use std::sync::Mutex;
use std::time::Duration;

/// C# `HashSet<string>(StringComparer.OrdinalIgnoreCase)`.
#[derive(Clone, Debug, Default)]
pub struct OrdinalIgnoreCaseSet {
    items: Vec<String>,
}

impl OrdinalIgnoreCaseSet {
    pub fn add(&mut self, value: String) {
        if !self.contains(&value) { self.items.push(value); }
    }

    pub fn contains(&self, value: &str) -> bool {
        self.items.iter().any(|s| s.eq_ignore_ascii_case(value))
    }

    pub fn iter(&self) -> impl Iterator<Item = &str> { self.items.iter().map(String::as_str) }
}

enum Cached {
    Missing,
    Bytes(Vec<u8>),
    Set(OrdinalIgnoreCaseSet),
}

/// C# `class NetworkHost`.
pub struct NetworkHost {
    cache: Mutex<HashMap<String, Cached>>,
    client: ureq::Agent,
    base: String,
}

impl NetworkHost {
    /// C# `NetworkHost(Uri address, string folder = null)`.
    pub fn new(address: &str, folder: Option<&str>) -> Self {
        let client = ureq::AgentBuilder::new().timeout(Duration::from_secs(30 * 60)).build();
        Self { cache: Mutex::new(HashMap::new()), client, base: baseWithFolder(address, folder) }
    }

    /// C# `GetSetAsync`. `shouldThrow` is ignored, as in the C#.
    pub fn getSetAsync(&self, _shouldThrow: bool) -> io::Result<Option<OrdinalIgnoreCaseSet>> {
        if let Some(hit) = self.cache.lock().unwrap_or_else(|p| p.into_inner()).get(".set") {
            return Ok(match hit { Cached::Set(s) => Some(s.clone()), _ => None });
        }
        let url = joinUrl(&self.base, ".set");
        let set = match self.fetch(&url, false)? {
            Some(data) => Some(fromBytesSet(&data)?),
            None => None,
        };
        let cached = match &set { Some(s) => Cached::Set(s.clone()), None => Cached::Missing };
        self.cache.lock().unwrap_or_else(|p| p.into_inner()).insert(".set".into(), cached);
        Ok(set)
    }

    /// C# `GetFileAsync`. The key is the path with `\` turned into `/`.
    /// `shouldThrow` is ignored, as in the C#.
    pub fn getFileAsync(&self, filePath: &str, _shouldThrow: bool) -> io::Result<Option<Vec<u8>>> {
        let key = filePath.replace('\\', "/");
        if let Some(hit) = self.cache.lock().unwrap_or_else(|p| p.into_inner()).get(&key) {
            return Ok(match hit { Cached::Bytes(b) => Some(b.clone()), _ => None });
        }
        let url = joinUrl(&self.base, &key);
        let bytes = self.fetch(&url, false)?;
        let cached = match &bytes { Some(b) => Cached::Bytes(b.clone()), None => Cached::Missing };
        self.cache.lock().unwrap_or_else(|p| p.into_inner()).insert(key, cached);
        Ok(bytes)
    }

    fn fetch(&self, url: &str, shouldThrow: bool) -> io::Result<Option<Vec<u8>>> {
        match self.client.get(url).call() {
            Ok(resp) => {
                let mut buf = Vec::new();
                resp.into_reader().read_to_end(&mut buf)?;
                Ok(Some(buf))
            }
            Err(ureq::Error::Status(code, resp)) => {
                if !shouldThrow { return Ok(None); }
                Err(Error::new(ErrorKind::Other, format!("InvalidOperationException: {} ({code})", resp.status_text())))
            }
            Err(e) => Err(Error::new(ErrorKind::Other, e.to_string())),
        }
    }
}

/// C# `UriBuilder` path assignment: `localPath + folder + "/"`, no inserted slash.
pub fn baseWithFolder(address: &str, folder: Option<&str>) -> String {
    let Some(folder) = folder else { return address.to_string(); };
    let (origin, path) = splitOrigin(address);
    format!("{origin}{path}{folder}/")
}

fn splitOrigin(address: &str) -> (String, String) {
    let Some(scheme) = address.find("://") else { return (String::new(), address.to_string()); };
    let rest = &address[scheme + 3..];
    match rest.find('/') {
        Some(i) => (address[..scheme + 3 + i].to_string(), rest[i..].to_string()),
        None => (address.to_string(), "/".to_string()),
    }
}

/// Trim a trailing slash on the base and a leading slash on the path, then join.
pub fn joinUrl(base: &str, path: &str) -> String {
    if path.contains("://") { return path.to_string(); }
    let base = base.trim_end_matches('/');
    let path = path.trim_start_matches('/');
    if path.is_empty() { base.to_string() } else { format!("{base}/{path}") }
}

/// C# `ToPathAndQueryString`. An empty value is left out. Space encodes as `+`.
pub fn toPathAndQueryString(path: &str, nvc: Option<&[(&str, &str)]>) -> String {
    let Some(nvc) = nvc else { return path.to_string(); };
    let parts: Vec<String> = nvc.iter().filter(|(_, v)| !v.is_empty()).map(|(k, v)| format!("{}={}", urlEncode(k), urlEncode(v))).collect();
    if parts.is_empty() { path.to_string() } else { format!("{path}?{}", parts.join("&")) }
}

fn urlEncode(s: &str) -> String {
    let mut out = String::new();
    for b in s.bytes() {
        match b {
            b'A'..=b'Z' | b'a'..=b'z' | b'0'..=b'9' | b'-' | b'_' | b'.' => out.push(b as char),
            b' ' => out.push('+'),
            _ => out.push_str(&format!("%{b:02X}")),
        }
    }
    out
}

fn ascii(data: &[u8]) -> String {
    data.iter().map(|&b| if b.is_ascii() { b as char } else { '?' }).collect()
}

/// C# `FromBytes<HashSet<string>>`. `startIndex` comes from the first line.
pub fn fromBytesSet(data: &[u8]) -> io::Result<OrdinalIgnoreCaseSet> {
    let text = ascii(data);
    let lines: Vec<&str> = text.split('\n').collect();
    let Some(start) = lines.first().and_then(|line| startIndex(line)) else {
        return Err(Error::new(ErrorKind::InvalidData, "NullReferenceException: GetDirectoryName"));
    };
    let mut set = OrdinalIgnoreCaseSet::default();
    for line in lines {
        if line.len() >= start {
            let path = line[start..].trim_end().replace('\\', "/");
            if path != ".set" { set.add(path); }
        }
    }
    Ok(set)
}

/// Index just past `Path.GetDirectoryName` of the first line. `None` when that is null.
fn startIndex(first: &str) -> Option<usize> {
    let line = first.trim_end().replace('\\', "/");
    if line.is_empty() || line == "/" { return None; }
    if line.len() >= 3 && line.as_bytes()[1] == b':' && line[2..].chars().all(|c| c == '/') { return None; }
    match line.rfind('/') { Some(i) => Some(i + 1), None => Some(1) }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn folderIsConcatenatedWithoutASlash() {
        assert_eq!(baseWithFolder("http://host/files", Some("a")), "http://host/filesa/");
        assert_eq!(baseWithFolder("http://host/files", None), "http://host/files");
        assert_eq!(baseWithFolder("http://host", Some("a")), "http://host/a/");
    }

    #[test]
    fn queryEncodesSpacesAsPlus() {
        assert_eq!(toPathAndQueryString("p", Some(&[("a b", "c d")])), "p?a+b=c+d");
        assert_eq!(toPathAndQueryString("p", Some(&[("a", "")])), "p");
        assert_eq!(toPathAndQueryString("p", None), "p");
        assert_eq!(joinUrl("http://host/filesa/", ".set"), "http://host/filesa/.set");
    }

    #[test]
    fn fromBytesSetCutsAtTheFirstDirectory() {
        let data = b"C:\\games\\a.txt\nC:\\games\\b.txt\nC:\\games\\.set\n";
        let set = fromBytesSet(data).unwrap();
        assert!(set.contains("a.txt"));
        assert!(set.contains("B.TXT"));
        assert!(!set.contains(".set"));
        assert!(fromBytesSet(b"").is_err());
    }
}
