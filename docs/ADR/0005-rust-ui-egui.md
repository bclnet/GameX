ADR 0005: Rust UI is egui, view for view with C#'s XAML
===

* **Status:** Accepted
* **Date:** 2026-10-11
* **Supersedes:** [ADR 0004](0004-rust-ui-slint.md)
* **Review by:** 2027-10-11 ([ADR 0006](0006-review-rust-ui-2027.md))

## Context

ADR 0004 chose Slint for the Rust Explorer. Comparing toolkits against every view the Explorer actually has (file and info trees, filter, content tabs, text, data grid, image, hex, audio, engine view, family views, menus, resizable panes, dialogs) showed Slint 1.18 has no tree view and no splitter, so the Explorer's centre, the file tree, would be hand-built. The remaining options were:

| Toolkit | Built-in coverage of the 14 views | Cost |
| -- | -- | -- |
| egui / eframe 0.36 | 10; tables and file dialogs are common add-on crates, hex is custom, audio goes through OpenX | views are immediate mode, so they match Python file for file but not line for line |
| Qt Widgets in code (qtrs 0.5) | 8; trees, images and hex custom, engine view hard | the only maintained binding is new (July 2026) with one author; tree items, selection, images and the GL widget need C++ shims we would own; Qt 6 SDK and a C++ compiler to build |

QML (cxx-qt) was ruled out because Python writes its views in code, not markup.

## Decision

1. **Rust UI uses egui through eframe.** It covers the file tree, panes and tabs out of the box, builds with cargo alone, and is MIT / Apache, so the app needs no credit or GPL arrangement.
2. **Views stay file for file with C#.** Each XAML view and its code-behind become the one `.rs` file the port already has (`FileExplorer.xaml` + `FileExplorer.xaml.cs` → `views/file_explorer_xaml.rs`), holding a struct with a `ui()` method. Python's views are the guide for what each one builds.
3. **Add-on crates:** `egui_extras` for the data grid, `rfd` for native file dialogs, `egui_ltreeview` only if the file tree needs keyboard navigation or multi-select. Audio plays through OpenX's sfx, not the UI.
4. **OpenX engines host their renderer in an egui paint callback**, the way they use a `QOpenGLWidget` in Python and a WPF control in C#. OpenX's GL engine uses the same `glow` version as eframe, so eframe's GL context passes straight to it and texture handles are shared.

## Consequences

* The Rust Explorer can be compared with C#'s file by file (ADR 0001), though not line for line, since egui redraws each view from state every frame.
* eframe 0.36 uses glow 0.17. glow 0.18 adds only 64-bit WebAssembly support and a fix to GL query results, which OpenX doesn't use, so OpenX's GL engine builds on 0.17 unchanged. OpenX moves to 0.18 when eframe does.
* egui is a dependency only of the Rust apps and the engine hosts, not of OpenX's core or GameX's families.
* The app looks like egui, not like the platform.
