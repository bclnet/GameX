// PORT-SOURCE: Core/OpenX.Poly/System.IO/IStream.cs
// PORT-STATUS: partial
//
// Ported: `IWriteToStream`, brought out of the parked
// `OpenX/rust/_/core/polyio/io/i_stream.rs` with C#'s names. Not ported:
// `IStream` (`Stream GetStream()`); nothing in Rust needs it yet.
//
// C#'s `Stream` parameter is `&mut dyn Write`: every implementer only writes to it.

use std::io::{Result, Write};

/// C# `IWriteToStream`.
pub trait IWriteToStream {
    fn writeToStream(&self, stream: &mut dyn Write) -> Result<()>;
}
