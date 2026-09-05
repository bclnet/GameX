// PORT-SOURCE: Core/GameX.FileSystems/Casc/Utils.cs
// PORT-SHA: 18e0989509185468
// PORT-STATUS: done
//
// PARTIAL PORT: the CDN path and URL builders. The HTTP fetch helpers
// (`HttpWebResponse`, `GetResponse`) are not ported — they need an HTTP client
// decision (`reqwest` or `ureq`) and, more importantly, the scheme question
// below settled first.
//
// ============ CDN FETCHES ARE PLAINTEXT HTTP ============================
//
//     public static string MakeCDNUrl(string cdnHost, string cdnPath)
//         => $"http://{cdnHost}/{cdnPath}";
//
// Every CDN download goes over unencrypted HTTP. Three more sites do the same:
// `CascConfig.cs:341` and `:343` build `http://{host}/{path}` from the CDN
// config, and two commented-out lines reach `http://us.patch.battle.net:1119`.
// There is no `https` anywhere in the project.
//
// How bad this is depends on something worth being precise about: **CASC
// verifies content by hash**, so a tampered payload fails its content-key check
// rather than being executed. So this is not a code-execution hole. What it
// does leak is *which* files a user downloads — the CDN path contains the
// content key — and it leaves the fetch open to disruption rather than
// substitution.
//
// Blizzard's CDN hosts serve HTTPS. Switching is a one-word change per site and
// costs nothing. **Worth doing in the C# too**, not just in the port; this
// function takes the scheme as a parameter so neither choice is silent.
//
// ===================== TWO MORE C#-SIDE BUGS =============================
//
//   1. **`MakeCDNPath` slices without checking the length.**
//      `fileName.Substring(0, 2)` and `Substring(2, 2)` throw
//      `ArgumentOutOfRangeException` for any name under four characters. CASC
//      keys are 32 hex characters so this holds in practice — but the function
//      is also called with names from config files, and the failure is an
//      exception from string slicing rather than a diagnosable error.
//
//   2. **`HttpWebResponse` retries by recursion with `numRetries >= 5`**, and
//      the recursive call is the *only* thing that increments it. Every retry
//      re-enters the whole function including the `WebRequest.CreateHttp`, so
//      the 5-deep stack holds five live `HttpWebRequest` objects. It also
//      retries unconditionally on any exception, including 404, so a missing
//      file costs five round trips before failing.

/// URL scheme for CDN fetches.
///
/// A parameter rather than a hard-coded literal, so the choice is visible at
/// every call site. The C# hard-codes `http` in four places.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum Scheme {
    /// What the C# does.
    Http,
    /// What Blizzard's CDN hosts also serve.
    #[default]
    Https,
}

impl Scheme {
    pub const fn as_str(self) -> &'static str {
        match self {
            Self::Http => "http",
            Self::Https => "https",
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum PathError {
    /// The C# throws `ArgumentOutOfRangeException` from `Substring`.
    NameTooShort(usize),
}

impl std::fmt::Display for PathError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::NameTooShort(n) => {
                write!(f, "CDN file name is {n} characters, need at least 4")
            }
        }
    }
}

impl std::error::Error for PathError {}

/// C# `MakeCDNPath(string cdnPath, string folder, string fileName)`.
///
/// CASC shards by the first two byte-pairs of the key:
/// `{cdnPath}/{folder}/{ab}/{cd}/{abcdef...}`.
pub fn make_cdn_path_in(
    cdn_path: &str,
    folder: &str,
    file_name: &str,
) -> Result<String, PathError> {
    let (a, b) = shard(file_name)?;
    Ok(format!("{cdn_path}/{folder}/{a}/{b}/{file_name}"))
}

/// C# `MakeCDNPath(string cdnPath, string fileName)` — the two-argument form.
pub fn make_cdn_path(cdn_path: &str, file_name: &str) -> Result<String, PathError> {
    let (a, b) = shard(file_name)?;
    Ok(format!("{cdn_path}/{a}/{b}/{file_name}"))
}

/// The two shard components, checked. The C# slices unchecked (bug 1).
fn shard(file_name: &str) -> Result<(&str, &str), PathError> {
    if file_name.len() < 4 || !file_name.is_char_boundary(2) || !file_name.is_char_boundary(4) {
        return Err(PathError::NameTooShort(file_name.len()));
    }
    Ok((&file_name[0..2], &file_name[2..4]))
}

/// C# `MakeCDNUrl(string cdnHost, string cdnPath)`.
///
/// The C# hard-codes `http`; the scheme is explicit here. See the module note.
pub fn make_cdn_url(scheme: Scheme, cdn_host: &str, cdn_path: &str) -> String {
    format!("{}://{cdn_host}/{cdn_path}", scheme.as_str())
}

#[cfg(test)]
mod tests {
    use super::*;

    const KEY: &str = "0123456789abcdef0123456789abcdef";

    #[test]
    fn cdn_paths_shard_on_the_first_two_byte_pairs() {
        assert_eq!(
            make_cdn_path("tpr/wow", KEY).unwrap(),
            format!("tpr/wow/01/23/{KEY}")
        );
        assert_eq!(
            make_cdn_path_in("tpr/wow", "data", KEY).unwrap(),
            format!("tpr/wow/data/01/23/{KEY}")
        );
    }

    #[test]
    fn a_short_name_is_an_error_not_a_substring_exception() {
        // The C# throws ArgumentOutOfRangeException from Substring.
        assert_eq!(make_cdn_path("p", "abc"), Err(PathError::NameTooShort(3)));
        assert_eq!(make_cdn_path("p", ""), Err(PathError::NameTooShort(0)));
        // Exactly four is the minimum and must work.
        assert_eq!(make_cdn_path("p", "abcd").unwrap(), "p/ab/cd/abcd");
    }

    #[test]
    fn multibyte_names_do_not_split_a_character() {
        // Slicing at byte 2 of a multi-byte character would panic in Rust and
        // silently split a UTF-16 surrogate in the C#.
        assert!(make_cdn_path("p", "\u{00e9}\u{00e9}\u{00e9}").is_err());
    }

    #[test]
    fn the_scheme_is_explicit_at_the_call_site() {
        assert_eq!(
            make_cdn_url(Scheme::Http, "level3.blizzard.com", "tpr/wow"),
            "http://level3.blizzard.com/tpr/wow"
        );
        assert_eq!(
            make_cdn_url(Scheme::Https, "level3.blizzard.com", "tpr/wow"),
            "https://level3.blizzard.com/tpr/wow"
        );
    }

    #[test]
    fn https_is_the_default() {
        // The C# has no https anywhere; this makes the safe choice the one you
        // get by not thinking about it.
        assert_eq!(Scheme::default(), Scheme::Https);
        assert_eq!(Scheme::default().as_str(), "https");
    }

    #[test]
    fn a_full_url_composes_from_the_two_halves() {
        let path = make_cdn_path_in("tpr/wow", "config", KEY).unwrap();
        let url = make_cdn_url(Scheme::Https, "blzddist1-a.akamaihd.net", &path);
        assert_eq!(
            url,
            format!("https://blzddist1-a.akamaihd.net/tpr/wow/config/01/23/{KEY}")
        );
    }
}
