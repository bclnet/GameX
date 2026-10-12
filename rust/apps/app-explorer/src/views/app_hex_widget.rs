// Port of python/gamex/app/explorer/views/AppHexWidget.py. No C# twin: C#'s
// THex template uses the WPFHexaEditor library, so there is no PORT-SOURCE.
//
// Audio and video payloads, which Python hands to an external player, show as
// a hexdump here: audio is a placeholder in the Rust Explorer.

use std::io::{Read, Seek, SeekFrom};
use eframe::egui;
use gamex::meta::Tag;

/// AppHexWidget
pub struct AppHexWidget {
    text: String,
}

impl AppHexWidget {
    const rowlen: usize = 0x10;
    const maxHexdumpByteLen: usize = 8192;

    pub fn new(name: &str, value: &Tag) -> Self {
        let mut s = Self { text: String::new() };
        match value {
            Tag::Stream(z) => {
                let mut z = z.lock().unwrap();
                let fileSize = z.seek(SeekFrom::End(0)).and_then(|n| z.seek(SeekFrom::Start(0)).map(|_| n)).unwrap_or_default() as usize;
                let mut content = vec![];
                let _ = z.by_ref().take(Self::maxHexdumpByteLen as u64 + 1).read_to_end(&mut content);
                s.viewFile(name, &content, fileSize);
            }
            Tag::Bytes(z) => s.viewFile(name, z, z.len()),
            Tag::Str(z) => s.viewFile(name, z.as_bytes(), z.len()),
            _ => s.viewFile(name, &[], 0),
        }
        s
    }

    fn showText(&mut self, content: &[u8], encoding: &str) {
        self.text = match encoding {
            "utf-32" => content[4..].chunks_exact(4).filter_map(|c| char::from_u32(u32::from_le_bytes([c[0], c[1], c[2], c[3]]))).collect(),
            "utf-16" => String::from_utf16_lossy(&content[2..].chunks_exact(2).map(|c| u16::from_le_bytes([c[0], c[1]])).collect::<Vec<_>>()),
            _ => String::from_utf8_lossy(content).into_owned(),
        };
    }

    fn showHexdump(&mut self, content: &[u8], fileSize: usize) {
        let mut t = String::new();
        let hexstrlen = Self::rowlen * 3;
        let numbytestoprint = (Self::maxHexdumpByteLen + 1).min(content.len());
        for (i, section) in content[..numbytestoprint].chunks(Self::rowlen).enumerate() {
            let hexstr = section.iter().map(|y| format!("{y:02x}")).collect::<Vec<_>>().join(" ");
            let charstr: String = section.iter().map(|&y| if (0x20..=0x7E).contains(&y) { y as char } else { '.' }).collect();
            t += &format!("{:06x} {hexstr:hexstrlen$} {charstr:rowlen$}\n", i * Self::rowlen, rowlen = Self::rowlen);
        }
        if numbytestoprint < fileSize { t += &format!("... ({} bytes truncated) ...", fileSize - numbytestoprint); }
        self.text = t;
    }

    fn getFileInfo(content: &[u8]) -> (&'static str, &'static str) {
        if content.starts_with(b"\xff\xfe\0\0") { ("utf-32", "txt") }
        else if content.starts_with(b"\xff\xfe") { ("utf-16", "txt") }
        else if content.iter().all(|&x| (0x20..=0x7E).contains(&x) || matches!(x, 0xd | 0xa | 0x9)) { ("utf-8", "txt") }
        else { ("utf-8", "") }
    }

    fn viewFile(&mut self, filename: &str, content: &[u8], fileSize: usize) {
        match Self::getFileInfo(content) {
            (encoding, "txt") => self.showText(content, encoding), // show strings as normal text files
            _ => self.showHexdump(content, fileSize),              // show binary data in hexview
        }
    }

    pub fn ui(&self, ui: &mut egui::Ui) {
        egui::ScrollArea::both().auto_shrink(false).show(ui, |ui| {
            ui.add(egui::Label::new(egui::RichText::new(&self.text).monospace()).extend());
        });
    }
}
