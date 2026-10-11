Architecture Decision Records
===

Decisions that shape how GameX is built, and why. Add a new numbered file for each decision; don't rewrite an accepted one, supersede it with a new record instead.

| ADR | Decision | Status
| -- | -- | --
| [0001](0001-ports-stay-one-to-one.md) | Every change is applied to every language | Accepted
| [0002](0002-library-homes-for-polyfills.md) | Polyfills go to the library they extend | Proposed
| [0003](0003-value-types.md) | C# value types are ported by behaviour | Proposed
| [0004](0004-rust-ui-slint.md) | Rust UI is Slint, view for view with C#'s XAML | Superseded by 0005
| [0005](0005-rust-ui-egui.md) | Rust UI is egui, view for view with C#'s XAML | Accepted
| [0006](0006-review-rust-ui-2027.md) | Recheck the Rust UI choice by October 2027 | Accepted
