// PORT-SOURCE: Core/GameX.FileSystems/Casc/WildcardRegex.cs
// PORT-SHA: 929bd4e7907f08a2
// PORT-STATUS: done
//
// Glob-to-regex translation for CASC file listing: `*` -> `.*`, `?` -> `.`.
//
// Ported as a direct glob matcher rather than by building a regex string. The
// C# derives from `Regex` and rewrites an escaped pattern with two string
// replaces, which is where its edge cases come from — see below. A matcher
// needs no dependency and cannot be tricked by escaping.
//
// ===================== TWO C#-SIDE BUGS ==================================
//
//   1. **A literal `*` or `?` cannot be expressed.**
//
//          Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")
//
//      `Regex.Escape` turns `*` into `\*`, and the replace then turns every
//      `\*` back into `.*`. So a pattern meaning "a filename containing an
//      asterisk" is indistinguishable from a wildcard — there is no escape
//      hatch, because escaping is exactly what gets undone. Filenames with `?`
//      or `*` are legal in CASC's virtual paths.
//
//   2. **`matchStartEnd: false` anchors nothing, so it matches substrings.**
//      Without `^...$` the regex is a search, not a match, so pattern `foo`
//      matches `barfoobaz`. Both call styles exist in the tree, and the
//      difference is a bool at the call site.
//
// Also: deriving from `Regex` means every `WildcardRegex` compiles a pattern
// even when the caller only tests one path once.

/// C# `WildcardRegex`, as a matcher.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct WildcardRegex {
    pattern: String,
    /// C# `matchStartEnd` — anchor the pattern at both ends.
    anchored: bool,
}

impl WildcardRegex {
    /// C# `WildcardRegex(string pattern, bool matchStartEnd)`.
    pub fn new(pattern: impl Into<String>, match_start_end: bool) -> Self {
        Self { pattern: pattern.into(), anchored: match_start_end }
    }

    /// C# `IsMatch(input)`.
    pub fn is_match(&self, input: &str) -> bool {
        if self.anchored {
            glob_match(&self.pattern, input)
        } else {
            // Unanchored: the C# regex is a search, so any substring may match.
            let n: Vec<char> = input.chars().collect();
            (0..=n.len()).any(|i| glob_prefix(&self.pattern, &n[i..]))
        }
    }

    /// C# `WildcardToRegex(pattern, matchStartEnd)`, kept for callers that
    /// need the string form. Reproduces the C#'s output exactly, including
    /// bug 1 — a caller feeding this to a real regex engine gets the C#'s
    /// behaviour.
    pub fn to_regex(pattern: &str, match_start_end: bool) -> String {
        // Regex.Escape escapes these; `*` and `?` are then un-escaped by the
        // two replaces, which is the bug.
        let mut out = String::new();
        for c in pattern.chars() {
            match c {
                '*' => out.push_str(".*"),
                '?' => out.push('.'),
                '\\' | '^' | '$' | '.' | '[' | ']' | '(' | ')' | '|' | '+' | '{' | '}' => {
                    out.push('\\');
                    out.push(c);
                }
                ' ' | '\t' | '\n' | '#' => {
                    out.push('\\');
                    out.push(c);
                }
                _ => out.push(c),
            }
        }
        if match_start_end {
            format!("^{out}$")
        } else {
            out
        }
    }
}

/// Whole-string glob match: `*` any run, `?` exactly one char.
fn glob_match(pattern: &str, input: &str) -> bool {
    let p: Vec<char> = pattern.chars().collect();
    let s: Vec<char> = input.chars().collect();
    // Iterative backtracking, so a pathological pattern cannot blow the stack
    // or take exponential time the way naive recursion does.
    let (mut pi, mut si) = (0usize, 0usize);
    let (mut star, mut mark) = (usize::MAX, 0usize);
    while si < s.len() {
        if pi < p.len() && (p[pi] == '?' || p[pi] == s[si]) {
            pi += 1;
            si += 1;
        } else if pi < p.len() && p[pi] == '*' {
            star = pi;
            mark = si;
            pi += 1;
        } else if star != usize::MAX {
            pi = star + 1;
            mark += 1;
            si = mark;
        } else {
            return false;
        }
    }
    while pi < p.len() && p[pi] == '*' {
        pi += 1;
    }
    pi == p.len()
}

/// Whether the pattern matches a prefix of `s` — the unanchored case.
fn glob_prefix(pattern: &str, s: &[char]) -> bool {
    let p: Vec<char> = pattern.chars().collect();
    let (mut pi, mut si) = (0usize, 0usize);
    let (mut star, mut mark) = (usize::MAX, 0usize);
    loop {
        if pi == p.len() {
            return true; // pattern consumed: a prefix matched
        }
        if si < s.len() && (p[pi] == '?' || p[pi] == s[si]) {
            pi += 1;
            si += 1;
        } else if p[pi] == '*' {
            star = pi;
            mark = si;
            pi += 1;
        } else if star != usize::MAX && mark < s.len() {
            pi = star + 1;
            mark += 1;
            si = mark;
        } else {
            return false;
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn star_matches_any_run() {
        let r = WildcardRegex::new("*.blp", true);
        assert!(r.is_match("icon.blp"));
        assert!(r.is_match(".blp"));
        assert!(!r.is_match("icon.blp.bak"));
    }

    #[test]
    fn question_matches_exactly_one() {
        let r = WildcardRegex::new("file?.dat", true);
        assert!(r.is_match("file1.dat"));
        assert!(!r.is_match("file.dat"), "? requires a character");
        assert!(!r.is_match("file12.dat"), "? is not a run");
    }

    #[test]
    fn anchored_patterns_match_the_whole_string() {
        let r = WildcardRegex::new("foo", true);
        assert!(r.is_match("foo"));
        assert!(!r.is_match("barfoobaz"));
    }

    #[test]
    fn unanchored_patterns_match_substrings_as_the_c_sharp_does() {
        // Bug 2: without ^...$ the regex is a search.
        let r = WildcardRegex::new("foo", false);
        assert!(r.is_match("barfoobaz"));
        assert!(r.is_match("foo"));
        assert!(!r.is_match("bar"));
    }

    #[test]
    fn multiple_stars_and_a_trailing_star() {
        let r = WildcardRegex::new("*/icons/*", true);
        assert!(r.is_match("interface/icons/temp.blp"));
        assert!(!r.is_match("interface/glues/temp.blp"));
        assert!(WildcardRegex::new("a*", true).is_match("a"));
        assert!(WildcardRegex::new("*", true).is_match(""));
    }

    #[test]
    fn a_pathological_pattern_terminates_quickly() {
        // Naive recursive globbing is exponential on this shape.
        let r = WildcardRegex::new("*a*a*a*a*a*a*b", true);
        assert!(!r.is_match(&"a".repeat(40)));
    }

    #[test]
    fn the_regex_string_form_reproduces_the_c_sharp_output() {
        assert_eq!(WildcardRegex::to_regex("*.blp", true), "^.*\\.blp$");
        assert_eq!(WildcardRegex::to_regex("*.blp", false), ".*\\.blp");
        assert_eq!(WildcardRegex::to_regex("a?b", true), "^a.b$");
    }

    #[test]
    fn the_c_sharp_cannot_express_a_literal_star() {
        // Bug 1: Escape turns `*` into `\*`, then the replace undoes it. So a
        // pattern intended to mean "contains an asterisk" becomes a wildcard.
        let as_regex = WildcardRegex::to_regex("a*b", true);
        assert!(as_regex.contains(".*"), "the star became a wildcard: {as_regex}");
        assert!(!as_regex.contains("\\*"), "no escape survives");
    }

    #[test]
    fn regex_metacharacters_are_escaped() {
        // These must stay literal or a filename with a dot or bracket matches
        // far too much.
        for (pat, ch) in [("a.b", "\\."), ("a[b", "\\["), ("a+b", "\\+"), ("a(b", "\\(")] {
            assert!(
                WildcardRegex::to_regex(pat, true).contains(ch),
                "{pat} should escape {ch}"
            );
        }
    }

    #[test]
    fn an_empty_pattern_matches_only_an_empty_string_when_anchored() {
        assert!(WildcardRegex::new("", true).is_match(""));
        assert!(!WildcardRegex::new("", true).is_match("x"));
        // Unanchored, an empty pattern matches anything (it matches at offset 0).
        assert!(WildcardRegex::new("", false).is_match("x"));
    }
}
