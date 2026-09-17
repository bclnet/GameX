// PORT-SOURCE: Core/GameX/Assembly.cs
// PORT-SHA: c09fddebd470ad26
// PORT-STATUS: done
//
// NOT PORTED — assembly-level attributes with no Rust equivalent:
//
//     [assembly: InternalsVisibleTo("GameXTests")]
//     [assembly: InternalsVisibleTo("GameX.AllTests")]
//     [assembly: InternalsVisibleTo("GameX.Uncore")]
//
// `InternalsVisibleTo` grants another assembly access to `internal` members.
// Rust's nearest equivalents are `pub(crate)` (narrower — no cross-crate
// escape) and `#[cfg(test)]` modules inside the crate under test (which is
// where this port puts unit tests anyway).
//
// The third line is worth noting: `GameX.Uncore` is not a test project, so the
// core crate is granting a *runtime* dependency access to its internals. In
// Rust that has to be an explicit `pub` API, which is stricter — anything
// `Uncore` needs will surface as a required export when it is ported.
