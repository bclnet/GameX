ADR 0004: Rust UI is Slint, view for view with C#'s XAML
===

* **Status:** Superseded by [ADR 0005](0005-rust-ui-egui.md)
* **Date:** 2026-10-11

## Context

The Explorer needs a UI in every language. C# uses WPF (`GameX.App.Explorer`, XAML views), and the MAUI explorer (`GameX.App.ExplorerM`) is C#-only. Python uses Qt (PyQt6), with each OpenX engine supplying a `QOpenGLWidget` that hosts its renderer. Rust has no built-in UI, so it needs a toolkit, and the toolkit has to host OpenX's renderer, which is built on `glow` 0.18.

Options considered (October 2026 versions):

| Toolkit | Views | Hosting OpenX's GL view |
| -- | -- | -- |
| Slint 1.18 | declarative `.slint` markup files | rendering notifier underlay; its GL renderer uses glow 0.18, as OpenX does |
| egui / eframe 0.36 | Rust code, immediate mode | paint callback; uses glow 0.17 |
| Qt via `cxx-qt` 0.10 | QML | possible but involved; needs the Qt 6 SDK and a C++ compiler |
| iced 0.14 | Rust code | wgpu, not GL; OpenX's GL engine doesn't plug in |

## Decision

1. **Rust UI uses Slint**, because C# is the reference and Slint's markup lets the Rust views follow it one to one: each XAML view becomes a `.slint` file with the same name in snake_case file form (`MainPage.xaml` → `main_page.slint`), and its code-behind becomes the matching `.rs` file (`MainPage.xaml.cs` → `main_page.rs`).
2. **OpenX engines host their renderer in Slint** the way they host it in a `QOpenGLWidget` in Python and a WPF control in C#: through Slint's rendering notifier, sharing its glow 0.18 context.
3. **License:** GameX is MIT, so the Rust apps use Slint's royalty-free license, which requires a visible "Made with Slint" credit (for example on an About page). Slint's GPL option is not used, since it would make distributed binaries GPL.

## Consequences

* The Rust Explorer's views can be compared with C#'s file by file, like the rest of the port (ADR 0001).
* OpenX's Rust engines need a Slint host alongside the WPF and Qt hosts the other languages have.
* The Explorer must show the Slint credit.
* Slint is a dependency only of the Rust apps and the engine hosts, not of OpenX's core or GameX's families.
