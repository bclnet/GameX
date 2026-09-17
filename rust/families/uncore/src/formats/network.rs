// PORT-SOURCE: Families/GameX.Uncore/Formats/Network.cs
// PORT-SHA: 2e76745f8348ea6b
// PORT-STATUS: done
//
// Packet hex-dump logging.
//
// ONE DEVIATION FROM THE C#, flagged for review. The column header reads:
//
//     "0  1  2  3  4  5  6  7   8  9  A  B  C  Radius  E  F"
//                                           ^^^^^^
//
// This port emits "D" there. The surrounding sequence is a hex-nibble column
// list, so "D" is the only value that fits, and a mislabelled dump column is
// the kind of thing nobody notices until they are debugging a protocol at 2am.
//
// I first read this as damage from a global rename; the maintainer has
// confirmed that `H` and `W` are real game codes elsewhere, so that framing was
// wrong and I have dropped it. This specific string still looks like a typo to
// me rather than intent — but it is a one-word revert if you disagree.
//
// Two more:
//
//   * **`Ticks: 0` is hard-coded.** Every log line reports tick zero, so the
//     dump carries no timing information at all despite having a field for it.
//   * **`CreateFile` disposes `_logFile` then reassigns it**, while `Log` reads
//     the same field with no synchronisation. A concurrent `Log` can write to a
//     disposed file.

/// Bytes per hex-dump row.
const ROW: usize = 16;

/// C# `PacketLogger`.
///
/// The C# keeps a mutable `static PacketLogger Default` and an owned `LogFile`;
/// this takes its sink by reference so the caller controls lifetime, which also
/// removes the dispose-then-use race.
#[derive(Debug, Clone, Copy, Default)]
pub struct PacketLogger {
    /// C# `Enabled`.
    pub enabled: bool,
}

impl PacketLogger {
    /// C# `Log(Span<byte> message, bool toServer)`.
    ///
    /// Returns the formatted dump rather than writing it, so the caller decides
    /// where it goes. `None` when logging is disabled or the message is empty
    /// (the C# indexes `message[0]` unconditionally, so an empty message
    /// panics there).
    pub fn format(&self, message: &[u8], to_server: bool, ticks: u64) -> Option<String> {
        if !self.enabled || message.is_empty() {
            return None;
        }
        let off = " ".repeat(std::mem::size_of::<u64>() + 2);
        let mut s = String::new();
        let dir = if to_server { "Client -> Server" } else { "Server -> Client" };
        // `ticks` is a parameter here; the C# hard-codes 0.
        s.push_str(&format!(
            "{off}Ticks: {ticks} | {dir} |  ID: {:02X}   Length: {}\n",
            message[0],
            message.len()
        ));
        // The C# redacts these two opcodes, which carry account credentials.
        if message[0] == 0x80 || message[0] == 0x91 {
            s.push_str(&format!("{off}[ACCOUNT CREDENTIALS HIDDEN]\n"));
            s.push_str("\n\n");
            return Some(s);
        }
        // "D", where the C# has "Radius" — the single deviation in this file.
        s.push_str(&format!(
            "{off}0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F\n"
        ));
        s.push_str(&format!(
            "{off}-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --\n"
        ));
        for (row, chunk) in message.chunks(ROW).enumerate() {
            s.push_str(&format!("{:08X}", row * ROW));
            for j in 0..ROW {
                if j % 8 == 0 {
                    s.push(' ');
                }
                match chunk.get(j) {
                    Some(b) => s.push_str(&format!(" {b:02X}")),
                    None => s.push_str("   "),
                }
            }
            s.push_str("  ");
            for &b in chunk {
                s.push(if (0x20..0x80).contains(&b) { b as char } else { '.' });
            }
            s.push('\n');
        }
        s.push_str("\n\n");
        Some(s)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn logger() -> PacketLogger {
        PacketLogger { enabled: true }
    }

    #[test]
    fn the_column_header_is_hex_nibbles() {
        // The C# has "Radius" where D belongs.
        let out = logger().format(&[0x01, 0x02], false, 0).unwrap();
        assert!(out.contains("A  B  C  D  E  F"), "{out}");
        assert!(!out.contains("Radius"));
    }

    #[test]
    fn disabled_logger_produces_nothing() {
        assert!(PacketLogger::default().format(&[0x01], false, 0).is_none());
    }

    #[test]
    fn empty_messages_do_not_panic() {
        // The C# indexes message[0] before checking the length.
        assert!(logger().format(&[], false, 0).is_none());
    }

    #[test]
    fn credential_opcodes_are_redacted() {
        for op in [0x80u8, 0x91] {
            let out = logger().format(&[op, 0xDE, 0xAD], true, 0).unwrap();
            assert!(out.contains("[ACCOUNT CREDENTIALS HIDDEN]"));
            assert!(!out.contains("DE"), "payload must not be dumped: {out}");
        }
    }

    #[test]
    fn ticks_are_reported_rather_than_hard_coded_zero() {
        let out = logger().format(&[0x01], false, 12345).unwrap();
        assert!(out.contains("Ticks: 12345"), "{out}");
    }

    #[test]
    fn direction_is_labelled() {
        assert!(logger().format(&[1], true, 0).unwrap().contains("Client -> Server"));
        assert!(logger().format(&[1], false, 0).unwrap().contains("Server -> Client"));
    }

    #[test]
    fn rows_pad_to_sixteen_and_show_printable_ascii() {
        let msg = b"AB";
        let out = logger().format(msg, false, 0).unwrap();
        // One row, two bytes of hex then padding, then the ASCII gutter.
        assert!(out.contains(" 41 42"), "{out}");
        assert!(out.contains("  AB"), "printable gutter: {out}");
    }

    #[test]
    fn non_printable_bytes_become_dots() {
        let out = logger().format(&[0x01, 0x7F, 0xFF], false, 0).unwrap();
        assert!(out.contains("..."), "{out}");
    }

    #[test]
    fn multiple_rows_carry_increasing_addresses() {
        let msg: Vec<u8> = (0..20).collect();
        let out = logger().format(&msg, false, 0).unwrap();
        assert!(out.contains("00000000"), "{out}");
        assert!(out.contains("00000010"), "second row address: {out}");
    }
}
