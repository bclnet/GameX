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
| [0007](0007-cell-shaping-stays-in-the-family.md) | Cell shaping stays in the family | Superseded in part by 0008
| [0008](0008-create-land-stays-in-the-family.md) | CreateLand stays in the family | Accepted
| [0009](0009-review-silk-vs-opentk.md) | Recheck Silk.NET.Vulkan against OpenTK by October 2027 | Superseded in part by 0010
| [0010](0010-wpf-vulkan-surface-stays-on-silk.md) | The WPF Vulkan surface stays on Silk | Accepted
| [0011](0011-explorer-and-explorerm-stay-one-to-one.md) | Explorer and ExplorerM stay one-to-one | Accepted
| [0012](0012-game-networking-lives-in-a-netcode-sibling.md) | Game networking lives in a Netcode sibling | Accepted
| [0013](0013-scripts-are-cmd-files.md) | Repo scripts are .cmd files | Accepted
| [0014](0014-csharp-projects-do-not-multi-target.md) | A C# project has one TargetFramework | Accepted
