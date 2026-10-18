Raylib
======

Engine id: `RL`. This slice registers the engine (caps: Drawing) and calls `IsWindowReady`, which does not open a window, then returns the binding version string. Host, sprite, and model methods throw. The Explorer does not list it.

C# uses the Raylib-cs 8.1.0 package (raylib 6.0, net10.0). `RaylibEngine.Version` calls `Raylib.IsWindowReady` and returns `Raylib.RAYLIB_VERSION`. Python `version` imports `pyray` only when called, then calls `is_window_ready` and returns `RAYLIB_VERSION`. Importing the engine module does not require that package. The PyPI distribution is `raylib`.

Rust does not call raylib. The crates.io `raylib` crate 6.0.0 sets `edition = "2024"` and `rust-version = "1.88"`. This workspace is edition 2021 and rust-version 1.75, so that crate is not a dependency. Registration stays as it is.

raylib 6 C API. Do not vendor raylib source.
