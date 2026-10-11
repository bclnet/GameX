ADR 0006: Recheck the Rust UI choice in October 2027
===

* **Status:** Accepted
* **Date:** 2026-10-11
* **Review by:** 2027-10-11

## Context

ADR 0005 chose egui over Qt Widgets and Slint based on the state of Rust UI toolkits in October 2026. Several of the reasons are likely to change within a year:

* **Qt Widgets in code (qtrs):** first released July 2026, one main author. Its item model was flat, it had no `QOpenGLWidget`, and pixmaps only loaded from files. A mature binding would let the Rust views port line for line from Python.
* **Slint:** had no tree view and no splitter. With both, its markup could follow C#'s XAML file for file.
* **eframe:** used glow 0.17, so OpenX's GL engine is pinned to 0.17 rather than the current 0.18.

## Decision

1. **Recheck ADR 0005 by 11 October 2027**, or sooner if one of the questions below flips.
2. **The recheck answers these questions:**
   * Has a Qt Widgets binding (qtrs or another) got hierarchical item models with icons and data, selection signals that say what was selected, images from bytes, and an OpenGL widget? Is it maintained by more than one person?
   * Has Slint added a tree view and a splitter?
   * Which glow version does eframe use now? If it is newer than the pin, move `OpenX/rust/engines/opengl/Cargo.toml` to it.
   * How far has the Rust Explorer got under egui, and what has been hard?
3. **The outcome is a new ADR:** either keep egui and set the next review date, or supersede ADR 0005.

## Consequences

* The egui choice is not permanent, but it holds until the review, so Explorer work continues on egui in the meantime.
* The glow pin is reviewed at least once a year even if no one notices eframe moving.
