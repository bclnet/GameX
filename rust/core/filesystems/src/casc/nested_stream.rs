// PORT-SOURCE: Core/GameX.FileSystems/Casc/NestedStream.cs
// PORT-SHA: 5fab69a740a6a985
// PORT-STATUS: done
//
// A length-limited view over another stream: reads at most `length` bytes and
// then reports EOF, without closing the underlying stream unless asked.
//
// Same shape as `PartialInputStream` in the OpenStack port, and the same design
// decision applies: **ownership is explicit in the type.** `NestedStream` owns
// its source by default; `SharedSource` is the borrowing variant. The C# takes
// a `Stream` reference and a `leaveOpen` flag, which leaves who-owns-what to a
// bool.
//
// ===================== FOUR C#-SIDE BUGS =================================
//
//   1. **`offset + count > buffer.Length` can overflow.** Both are `int`, so a
//      large pair wraps negative and passes the guard; the read then throws
//      from the underlying stream instead, with a less useful message. The
//      checked form is `count > buffer.Length - offset`.
//
//   2. **It tracks only `remainingBytes`, not where the window started.**
//      `Position` returns `length - remainingBytes`, which is the count of
//      bytes *this view* has consumed — correct only while nothing else moves
//      the underlying stream. Two `NestedStream`s over one source silently
//      interleave, and each reports a plausible `Position` throughout. Nothing
//      in the type prevents constructing them.
//
//   3. **The `Memory<byte>` overload checks `remainingBytes < 0`, the array
//      overload checks `count <= 0`.** Two read paths with different
//      end-of-stream conditions: the first returns 0 only once `remainingBytes`
//      has gone *negative*, which it cannot on its own arithmetic, so that
//      guard is dead and the empty-buffer case below it does the work instead.
//
//   4. **A short read from the underlying stream ends the window early and
//      silently.** `remainingBytes -= bytesRead` with no check that
//      `bytesRead == count`, so a source that returns fewer bytes leaves the
//      view reporting data remaining while the source is exhausted. Callers
//      see a truncated file rather than an error — the same defect as
//      `Util.CopyFile` in the OpenStack port.

use std::io::{self, Read, Seek, SeekFrom};

/// C# `NestedStream`, owning its source.
#[derive(Debug)]
pub struct NestedStream<S> {
    source: S,
    /// Total window length.
    length: u64,
    /// Bytes consumed so far.
    consumed: u64,
    /// Where the window begins in the source, so `Position` is meaningful even
    /// if something else has moved it. The C# does not record this (bug 2).
    start: Option<u64>,
}

impl<S: Read> NestedStream<S> {
    /// C# `NestedStream(Stream underlyingStream, long length, bool leaveOpen)`.
    ///
    /// `leaveOpen` has no counterpart: this owns the source, so dropping it
    /// closes it. Use [`NestedStream::shared`] for the borrowing case.
    pub fn new(source: S, length: u64) -> Self {
        Self { source, length, consumed: 0, start: None }
    }

    /// C# `Length`.
    #[inline]
    pub fn len(&self) -> u64 {
        self.length
    }

    #[inline]
    pub fn is_empty(&self) -> bool {
        self.length == 0
    }

    /// C# `Position` — bytes consumed from this window.
    #[inline]
    pub fn position(&self) -> u64 {
        self.consumed
    }

    /// Bytes left in the window.
    #[inline]
    pub fn remaining(&self) -> u64 {
        self.length - self.consumed
    }

    /// Consume the view and hand back the source.
    pub fn into_inner(self) -> S {
        self.source
    }

    /// Read exactly `remaining()` bytes, or fail.
    ///
    /// The plain `read` follows the C# and tolerates a short read (bug 4);
    /// this is the version to use when the window length came from a header
    /// and a short read means the archive is truncated.
    pub fn read_to_end_exact(&mut self) -> io::Result<Vec<u8>> {
        let n = self.remaining() as usize;
        let mut v = vec![0u8; n];
        self.read_exact(&mut v)?;
        Ok(v)
    }
}

impl<S: Read + Seek> NestedStream<S> {
    /// Record the window's start, so `source_position` can be checked.
    pub fn anchored(mut source: S, length: u64) -> io::Result<Self> {
        let start = source.stream_position()?;
        Ok(Self { source, length, consumed: 0, start: Some(start) })
    }

    /// Whether the underlying stream is where this view expects it to be.
    ///
    /// `None` when the view was not anchored. This is the check the C# cannot
    /// make, and it is what turns bug 2 from silent corruption into an error.
    pub fn is_consistent(&mut self) -> io::Result<Option<bool>> {
        let Some(start) = self.start else { return Ok(None) };
        let here = self.source.stream_position()?;
        Ok(Some(here == start + self.consumed))
    }
}

impl<S: Read> Read for NestedStream<S> {
    fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
        let left = self.remaining();
        if left == 0 || buf.is_empty() {
            return Ok(0);
        }
        // `min` on u64 then narrow: the C#'s `(int)Math.Min(count, remaining)`
        // casts a long to int, which truncates for a window over 2 GiB.
        let n = (buf.len() as u64).min(left) as usize;
        let read = self.source.read(&mut buf[..n])?;
        self.consumed += read as u64;
        Ok(read)
    }
}

/// The borrowing variant: a window over a stream someone else owns.
///
/// The C# expresses this with `leaveOpen: true`, which is a runtime flag rather
/// than a type distinction — so a caller cannot tell from a signature whether
/// passing a stream hands over ownership.
#[derive(Debug)]
pub struct SharedSource<'a, S>(pub &'a mut S);

impl<S: Read> Read for SharedSource<'_, S> {
    fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
        self.0.read(buf)
    }
}

impl<S: Seek> Seek for SharedSource<'_, S> {
    fn seek(&mut self, pos: SeekFrom) -> io::Result<u64> {
        self.0.seek(pos)
    }
}

impl<'a, S: Read> NestedStream<SharedSource<'a, S>> {
    /// A window over a borrowed source.
    pub fn shared(source: &'a mut S, length: u64) -> Self {
        Self::new(SharedSource(source), length)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::Cursor;

    #[test]
    fn reads_stop_at_the_window_length() {
        let mut n = NestedStream::new(Cursor::new(b"0123456789".to_vec()), 4);
        let mut buf = [0u8; 10];
        assert_eq!(n.read(&mut buf).unwrap(), 4);
        assert_eq!(&buf[..4], b"0123");
        assert_eq!(n.read(&mut buf).unwrap(), 0, "window exhausted");
    }

    #[test]
    fn position_and_remaining_track_consumption() {
        let mut n = NestedStream::new(Cursor::new(b"abcdef".to_vec()), 6);
        assert_eq!((n.position(), n.remaining()), (0, 6));
        let mut buf = [0u8; 2];
        n.read_exact(&mut buf).unwrap();
        assert_eq!((n.position(), n.remaining()), (2, 4));
        assert_eq!(n.len(), 6);
    }

    #[test]
    fn a_zero_length_window_reads_nothing() {
        let mut n = NestedStream::new(Cursor::new(b"abc".to_vec()), 0);
        assert!(n.is_empty());
        assert_eq!(n.read(&mut [0u8; 4]).unwrap(), 0);
    }

    #[test]
    fn an_empty_destination_reads_nothing() {
        // The C#'s two read paths disagree on this case (bug 3).
        let mut n = NestedStream::new(Cursor::new(b"abc".to_vec()), 3);
        assert_eq!(n.read(&mut []).unwrap(), 0);
        assert_eq!(n.remaining(), 3, "and consumes nothing");
    }

    #[test]
    fn a_window_longer_than_the_source_yields_what_exists() {
        let mut n = NestedStream::new(Cursor::new(b"ab".to_vec()), 100);
        let mut v = Vec::new();
        n.read_to_end(&mut v).unwrap();
        assert_eq!(v, b"ab");
        // The C# leaves `remainingBytes` at 98 here, so the view still claims
        // data remains (bug 4).
        assert_eq!(n.remaining(), 98, "faithful: the window still claims 98");
    }

    #[test]
    fn read_to_end_exact_reports_a_truncated_source() {
        // This is the version to use when the length came from a header.
        let mut n = NestedStream::new(Cursor::new(b"ab".to_vec()), 100);
        assert!(n.read_to_end_exact().is_err());
    }

    #[test]
    fn the_source_is_recoverable() {
        let mut n = NestedStream::new(Cursor::new(b"abcdef".to_vec()), 3);
        let mut buf = [0u8; 3];
        n.read_exact(&mut buf).unwrap();
        let mut cur = n.into_inner();
        // The underlying stream is positioned after what the window consumed.
        let mut rest = Vec::new();
        cur.read_to_end(&mut rest).unwrap();
        assert_eq!(rest, b"def");
    }

    #[test]
    fn anchoring_detects_an_externally_moved_source() {
        // This is the check the C# cannot make (bug 2).
        let mut cur = Cursor::new(b"0123456789".to_vec());
        cur.set_position(2);
        let mut n = NestedStream::anchored(SharedSource(&mut cur), 4).unwrap();
        let mut buf = [0u8; 2];
        n.read_exact(&mut buf).unwrap();
        assert_eq!(n.is_consistent().unwrap(), Some(true));
        // Something else moves the shared source.
        n.source.0.set_position(9);
        assert_eq!(n.is_consistent().unwrap(), Some(false));
    }

    #[test]
    fn an_unanchored_window_cannot_report_consistency() {
        let mut n = NestedStream::new(Cursor::new(b"abc".to_vec()), 3);
        assert_eq!(n.is_consistent().unwrap(), None);
    }

    #[test]
    fn two_windows_over_one_source_read_in_sequence() {
        // The C# permits this and each reports a plausible Position while
        // silently interleaving. Borrowing makes the sequencing explicit: the
        // first window must be finished with before the second exists.
        let mut cur = Cursor::new(b"AAAABBBB".to_vec());
        {
            let mut a = NestedStream::shared(&mut cur, 4);
            let mut buf = [0u8; 4];
            a.read_exact(&mut buf).unwrap();
            assert_eq!(&buf, b"AAAA");
        }
        let mut b = NestedStream::shared(&mut cur, 4);
        let mut buf = [0u8; 4];
        b.read_exact(&mut buf).unwrap();
        assert_eq!(&buf, b"BBBB");
    }

    #[test]
    fn a_window_over_two_gib_does_not_truncate_its_length() {
        // The C# does `(int)Math.Min(count, remainingBytes)`, casting a long to
        // int. The min is taken first so it is safe there, but the length
        // itself is a long throughout - this keeps u64 end to end.
        let n: NestedStream<Cursor<Vec<u8>>> =
            NestedStream::new(Cursor::new(Vec::new()), 5_000_000_000);
        assert_eq!(n.len(), 5_000_000_000);
        assert_eq!(n.remaining(), 5_000_000_000);
    }
}
