// PORT-SOURCE: Core/OpenX.Polyfills/Log.cs
// PORT-SHA: 6b9de09a36e10c69
// PORT-STATUS: done
//
// `Log.Func` is a static `Action<string>` that `Info` / `Warn` / `Error` /
// `Trace` / `Exception` call with no null check. C# throws
// `NullReferenceException` until `Engine.Activate` assigns it. The sink here
// starts as stderr, which is also what Rust's `Engine.logFunc` prints, so a
// log before activate still comes out.
//
// `Log.Assert` has an empty body. The real call is commented out. This port
// keeps that: assertions do nothing.
//
// `LogFile.Write` rents `message.Length` bytes and writes that many, which
// truncates non-ASCII UTF-8. The write here uses the encoded bytes.

use std::io::Write;
use std::sync::{Mutex, OnceLock, RwLock};

fn func() -> &'static RwLock<fn(&str)> {
    static FUNC: OnceLock<RwLock<fn(&str)>> = OnceLock::new();
    FUNC.get_or_init(|| RwLock::new(|message| eprintln!("{message}")))
}

/// C# `class Log`.
pub struct Log;

impl Log {
    /// C# `Log.Func = ...`. `Engine.Activate` assigns the engine's `LogFunc`.
    pub fn setFunc(f: fn(&str)) {
        *func().write().unwrap_or_else(|p| p.into_inner()) = f;
    }

    fn emit(message: &str) {
        let g = func().read().unwrap_or_else(|p| p.into_inner());
        g(message);
    }

    /// C# `Log.Info(string)`.
    pub fn info(message: &str) { Self::emit(message); }

    /// C# `Log.Warn(string)`.
    pub fn warn(message: &str) { Self::emit(&format!("WARN: {message}")); }

    /// C# `Log.Error(string)`.
    pub fn error(message: &str) { Self::emit(&format!("ERROR: {message}")); }

    /// C# `Log.Trace(string)`.
    pub fn trace(message: &str) { Self::emit(&format!("TRACE: {message}")); }

    /// C# `Log.Exception(Exception)`.
    pub fn exception(e: &dyn std::error::Error) { Self::emit(&format!("{e}")); }

    /// C# `Log.Assert(bool, string)`. The body is empty.
    pub fn assert(_condition: bool, _message: &str) {}
}

/// C# `class LogFile`.
pub struct LogFile {
    file: Mutex<std::fs::File>,
    path: std::path::PathBuf,
}

impl LogFile {
    /// C# `LogFile(string directory, string file)`.
    ///
    /// The C# timestamp is `yyyy-MM-dd_hh-mm-ss`, 12-hour with no AM/PM, so two
    /// files twelve hours apart share a name. `timestamp` is passed in so a
    /// test can pin it; `Engine` does not construct one. Pass `%H` (24-hour)
    /// when formatting the clock.
    pub fn new(directory: &str, file: &str, timestamp: &str) -> std::io::Result<Self> {
        let path = std::path::Path::new(directory).join(format!("{timestamp}_{file}"));
        let f = std::fs::OpenOptions::new().create(true).append(true).open(&path)?;
        Ok(Self { file: Mutex::new(f), path })
    }

    /// C# `Write(string)` — one line, flushed. The byte count is the UTF-8 length.
    pub fn write(&self, message: &str) -> std::io::Result<()> {
        let mut g = self.file.lock().unwrap_or_else(|p| p.into_inner());
        g.write_all(message.as_bytes())?;
        g.write_all(b"\n")?;
        g.flush()
    }

    /// C# `ToString() => logStream.Name`.
    pub fn path(&self) -> &std::path::Path { &self.path }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn infoBeforeActivateDoesNotPanic() {
        Log::info("no sink installed yet");
    }

    #[test]
    fn assertDoesNothing() {
        Log::assert(false, "this would fire");
    }
}
