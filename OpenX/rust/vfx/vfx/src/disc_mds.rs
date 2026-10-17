// PORT-SOURCE: Vfx/OpenX.Vfx/Disc.cs (MdsFormat)
// PORT-SHA: SHARED
// PORT-SHARED: yes  (extracted from the source file above, which has its own .rs)
// PORT-STATUS: partial
//
// Alcohol 120% `.mds` descriptor, paired with an `.mdf` data file.
//
// Ported: the descriptor types, `Parse`, `LoadResults` and `LoadMdsPath`.
// `LoadMdsPath` does not fill `RawTOCEntries`. Not yet ported: `MountBlobs`,
// `EmitRawTOCEntry` and `LoadMdsToDisc`, which need `Disc`, `Blob_Raw`,
// `RawTOCEntry`, `SubchannelQ`, `SS_Gap` and `Synthesize_DiscTOCFromRawTOCEntries`
// from `disc.rs`. Those types are not in the Rust tree, so this file stays partial.
//
// Not checked against a real `.mds` file. C# exceptions are `io::Error`s.

use std::collections::BTreeMap;
use std::io::{Cursor, Error, ErrorKind, Read, Result, Seek, SeekFrom};
use std::path::Path;

use openx_poly::prelude::BinaryReaderExt;

fn invalidOperation(message: String) -> Error { Error::new(ErrorKind::InvalidData, message) }

/// C# `MdsFormat.AFile` — a loose representation of an Alcohol 120 .mds file (with a few extras).
#[derive(Debug, Clone, Default)]
pub struct AFile {
    /// Full path to the MDS file
    pub mdsPath: String,
    /// MDS Header
    pub header: AHeader,
    /// List of MDS session blocks
    pub sessions: Vec<ASession>,
    /// List of track blocks
    pub tracks: Vec<ATrack>,
    /// Current parsed session objects
    pub parsedSession: Vec<Session>,
    /// Calculated MDS TOC entries (still to be parsed into BizHawk)
    pub tocEntries: Vec<ATocEntry>,
}

/// C# `MdsFormat.AHeader`.
#[derive(Debug, Clone, Default)]
pub struct AHeader {
    /// Standard alcohol 120% signature - usually "MEDIA DESCRIPTOR"
    pub signature: String,
    /// Alcohol version?
    pub version: (u8, u8),
    /// The medium type: 0x00 CD, 0x01 CD-R, 0x02 CD-RW, 0x10 DVD, 0x12 DVD-R
    pub medium: i32,
    /// Number of sessions
    pub sessionCount: i32,
    /// Burst Cutting Area length
    pub bcaLength: i32,
    /// Burst Cutting Area data offset
    pub bcaOffset: i64,
    /// Offset to disc (DVD?) structures
    pub structureOffset: i64,
    /// Offset to the first session block
    pub sessionOffset: i64,
    /// Data Position Measurement offset
    pub dpmOffset: i64,
}

impl AHeader {
    /// Parse mds stream for the header
    pub fn parse<R: Read + Seek>(&mut self, r: &mut R) -> Result<()> {
        // C# Encoding.ASCII: bytes above 0x7F decode as '?'
        self.signature = r.readBytes(16)?.iter().map(|&b| if b < 0x80 { b as char } else { '?' }).collect();
        self.version = (r.readByte()?, r.readByte()?);
        self.medium = r.readInt16()? as i32;
        self.sessionCount = r.readInt16()? as i32;
        r.skip(4)?;
        self.bcaLength = r.readInt16()? as i32;
        r.skip(8)?;
        self.bcaOffset = r.readInt32()? as i64;
        r.skip(24)?;
        self.structureOffset = r.readInt32()? as i64;
        r.skip(12)?;
        self.sessionOffset = r.readInt32()? as i64;
        self.dpmOffset = r.readInt32()? as i64;
        Ok(())
    }
}

/// C# `MdsFormat.ASession` — MDS session block representation.
#[derive(Debug, Clone, Default)]
pub struct ASession {
    pub sessionStart: i32,   // Session's start address
    pub sessionEnd: i32,     // Session's end address
    pub sessionNumber: i32,  // Session number
    pub allBlocks: u8,       // Number of all data blocks
    pub nonTrackBlocks: u8,  // Number of lead-in data blocks
    pub firstTrack: i32,     // First track in session
    pub lastTrack: i32,      // Last track in session
    pub trackOffset: i64,    // Offset of lead-in+regular track data blocks
}

/// C# `MdsFormat.ATrack` — representation of an MDS track block.
/// For convenience (and extra confusion) this also holds the track extrablock, filename(footer) block infos
/// as well as the calculated image filepath as specified in the MDS file
#[derive(Debug, Clone, Default)]
pub struct ATrack {
    /// The specified data mode (only lower 3 bits are actually meaningful)
    /// 0x00 None, 0x02 DVD (when header specifies DVD, Mode1 otherwise), 0xA9 Audio, 0xAA Mode1,
    /// 0xAB Mode2, 0xAC Mode2 Form1, 0xAD Mode2 Form2
    pub mode: u8,
    /// Subchannel mode for the track (0x00 = None, 0x08 = Interleaved)
    pub subMode: u8,
    // These are the fields from Sub-channel Q information, which are also returned in full TOC by READ TOC/PMA/ATIP command
    pub adrControl: i32,     // Adr/Ctl
    pub trackNo: i32,        // Track number field
    pub point: i32,          // Point field (= track number for track entries)
    pub aMin: i32,           // Min
    pub aSec: i32,           // Sec
    pub aFrame: i32,         // Frame
    pub zero: i32,           // Zero
    pub pMin: i32,           // PMin
    pub pSec: i32,           // PSec
    pub pFrame: i32,         // PFrame
    //
    pub extraOffset: i64,    // Start offset of this track's extra block
    pub sectorSize: i32,     // Sector size
    pub plba: i64,           // Track start sector (PLBA)
    pub startOffset: u64,    // Track start offset (from beginning of MDS file)
    pub files: i64,          // Number of filenames for this track
    pub footerOffset: i64,   // Start offset of footer (from beginning of MDS file)
    /// Track extra block
    pub extraBlock: ATrackExtra,
    /// List of footer(filename) blocks for this track
    pub footerBlocks: Vec<AFooter>,
    /// List of the calculated full paths to this track's image file
    /// The MDS file itself may contain a filename, or just an *.extension
    pub imageFileNamePaths: Vec<String>,
    pub blobIndex: i32,
}

/// C# `MdsFormat.ATrackExtra` — extra track block.
#[derive(Debug, Clone, Copy, Default)]
pub struct ATrackExtra {
    pub pregap: i64,         // Number of sectors in pregap
    pub sectors: i64,        // Number of sectors in track
}

/// C# `MdsFormat.AFooter` — footer (filename) block, potentially one for every track.
#[derive(Debug, Clone, Default)]
pub struct AFooter {
    pub filenameOffset: i64, // Start offset of image filename string (from beginning of mds file)
    pub wideChar: i64,       // Seems to be set to 1 if widechar filename is used
}

/// C# `MdsFormat.ATocEntry` — represents a parsed MDS TOC entry.
#[derive(Debug, Clone, Default)]
pub struct ATocEntry {
    /// these should be 0-indexed
    pub entryNum: i32,
    /// 1-indexed - the session that this entry belongs to
    pub session: i32,
    /// this seems just to be the LBA corresponding to PMIN:PSEC:PFRAME (give or take 150).
    pub plba: i32,
    // these correspond pretty directly to values in the Q subchannel fields
    // NOTE: they're specified as absolute MSF. That means, they're 2 seconds off from what they should be when viewed as final TOC values
    pub adrControl: i32,
    pub trackNo: i32,
    pub point: i32,
    pub aMin: i32,
    pub aSec: i32,
    pub aFrame: i32,
    pub zero: i32,
    pub pMin: i32,
    pub pSec: i32,
    pub pFrame: i32,
    /// Lower 3 bits of ATrack Mode
    /// 0x0 None or Mode2 (depends on sector size), 0x1 Audio, 0x2 DVD or Mode1 (depends on medium),
    /// 0x3 Mode2, 0x4 Mode2 Form1, 0x5 Mode2 Form2, 0x6 UNKNOWN, 0x7 Mode2
    pub trackMode: i32,
    pub sectorSize: i32,
    pub trackOffset: i64,
    /// List of the calculated full paths to this track's image file
    /// The MDS file itself may contain a filename, or just an *.extension
    pub imageFileNamePaths: Vec<String>,
    /// Track extra block
    pub extraBlock: ATrackExtra,
    pub blobIndex: i32,
}

/// C# `MdsFormat.Session` — custom session object.
#[derive(Debug, Clone, Default)]
pub struct Session {
    pub startSector: i64,
    pub startTrack: i32,
    pub sessionSequence: i32,
    pub endSector: i64,
    pub endTrack: i32,
}

/// C# `class MdsFormat`.
pub struct MdsFormat;

impl MdsFormat {
    /// C# `Parse(Stream s, string path)`.
    pub fn parse<R: Read + Seek>(s: &mut R, path: &str) -> Result<AFile> {
        let mut file = AFile { mdsPath: path.to_string(), ..Default::default() };
        let length = s.seek(SeekFrom::End(0))?;
        s.seek(SeekFrom::Start(0))?;
        // check whether the header in the mds file is long enough
        if length < 88 { return Err(invalidOperation("Malformed MDS format: The descriptor file does not appear to be long enough.".into())); }
        // parse header
        file.header.parse(s)?;
        // check version to make sure this is only v1.x
        // currently NO support for version 2.x
        if file.header.version.0 > 1 { return Err(invalidOperation(format!("MDS Parse Error: Only MDS version 1.x is supported!\nDetected version: {}.{}", file.header.version.0, file.header.version.1))); }

        let isDvd = matches!(file.header.medium, 0x10 | 0x12);
        if isDvd { return Err(invalidOperation("DVD Detected. Not currently supported!".into())); }

        // parse sessions (C# Dictionary<int, ASession>: kept in insertion order, Add throws on a duplicate key)
        let mut sessions: Vec<ASession> = Vec::new();

        s.seek(SeekFrom::Start(file.header.sessionOffset as u64))?;
        for _ in 0..file.header.sessionCount {
            let session = ASession {
                sessionStart: s.readInt32()?,
                sessionEnd: s.readInt32()?,
                sessionNumber: s.readInt16()? as i32,
                allBlocks: s.readByte()?,
                nonTrackBlocks: s.readByte()?,
                firstTrack: s.readInt16()? as i32,
                lastTrack: s.readInt16()? as i32,
                trackOffset: { s.skip(4)?; s.readInt32()? as i64 },
            };
            if sessions.iter().any(|x| x.sessionNumber == session.sessionNumber) { return Err(invalidOperation(format!("An item with the same key has already been added. Key: {}", session.sessionNumber))); }
            sessions.push(session);
        }

        let mut footerOffset = 0i64;

        // parse track blocks
        let mut aTracks: BTreeMap<i32, ATrack> = BTreeMap::new();

        // iterate through each session block
        for session in &sessions {
            s.seek(SeekFrom::Start(session.trackOffset as u64))?;
            // iterate through every block specified in each session
            for _ in 0..session.allBlocks {
                let mut trackHeader = [0u8; 80];
                let mut track = ATrack::default();
                let bytesRead = s.read(&mut trackHeader)?;
                debug_assert!(bytesRead == trackHeader.len(), "reached end-of-file while reading track header");
                // As in the C#, the fields below are read from the stream, not from `trackHeader`, so each block consumes 160 bytes.
                track.mode = s.readByte()?;
                track.subMode = s.readByte()?;
                track.adrControl = s.readByte()? as i32;
                track.trackNo = s.readByte()? as i32;
                track.point = s.readByte()? as i32;
                track.aMin = s.readByte()? as i32;
                track.aSec = s.readByte()? as i32;
                track.aFrame = s.readByte()? as i32;
                track.zero = s.readByte()? as i32;
                track.pMin = s.readByte()? as i32;
                track.pSec = s.readByte()? as i32;
                track.pFrame = s.readByte()? as i32;
                track.extraOffset = s.readInt32()? as i64;
                track.sectorSize = s.readInt16()? as i32;
                s.skip(18)?;
                track.plba = s.readInt32()? as i64;
                track.startOffset = s.readUInt64()?;
                track.files = s.readInt32()? as i64;
                track.footerOffset = s.readInt32()? as i64;
                s.skip(24)?;
                let mut currPos = s.stream_position()?;
                // Only CDs have extra blocks - for DVDs ExtraOffset = track length
                if track.extraOffset > 0 && !isDvd {
                    s.seek(SeekFrom::Start(track.extraOffset as u64))?;
                    track.extraBlock.pregap = s.readInt32()? as i64;
                    track.extraBlock.sectors = s.readInt32()? as i64;
                    s.seek(SeekFrom::Start(currPos))?;
                }
                else if isDvd { track.extraBlock.sectors = track.extraOffset; }

                // read the footer/filename block for this track
                currPos = s.stream_position()?;
                let numOfFilenames = track.files;
                for _ in 1..=numOfFilenames {
                    // skip leadin/out info tracks
                    if track.footerOffset == 0 { continue; }

                    s.seek(SeekFrom::Start(track.footerOffset as u64))?;
                    let f = AFooter {
                        filenameOffset: s.readInt32()? as i64,
                        wideChar: s.readInt32()? as i64,
                    };
                    // C# also applies Distinct() here, which compares AFooter by reference, so it removes nothing.
                    track.footerBlocks.push(f.clone());

                    // parse the filename string
                    let mut fileName = "*.mdf".to_string();
                    if f.filenameOffset > 0 {
                        // filename offset is present
                        let position = s.seek(SeekFrom::Start(f.filenameOffset as u64))? as i64;
                        let mut fname = vec![0u8; if numOfFilenames == 1 {
                            if file.header.dpmOffset == 0 { length as i64 - position } // filename is in the remaining space to EOF
                            else { file.header.dpmOffset - position } // filename is in the remaining space to EOF + dpm offset
                        } else { 6 } as usize]; // looks like each filename string is 6 bytes with a trailing \0

                        // read the filename
                        let bytesRead2 = s.read(&mut fname)?;
                        debug_assert!(bytesRead2 == fname.len(), "reached end-of-file while reading track filename");

                        // if widechar is 1 filename is stored using 16-bit, otherwise 8-bit is used
                        fileName = if f.wideChar == 1 { String::from_utf16_lossy(&fname.chunks_exact(2).map(|c| u16::from_le_bytes([c[0], c[1]])).collect::<Vec<_>>()) }
                            else { String::from_utf8_lossy(&fname).into_owned() }
                            .trim_end_matches('\0').to_string();
                    }
                    //else { } // assume an MDF file with the same name as the MDS

                    let mdsPath = Path::new(&file.mdsPath);
                    let (dir, fileNoExt) = (mdsPath.parent().and_then(|x| x.to_str()).unwrap_or(""), mdsPath.file_stem().and_then(|x| x.to_str()).unwrap_or(""));
                    fileName = if f.filenameOffset == 0 || fileName.eq_ignore_ascii_case("*.mdf") { format!("{dir}\\{fileNoExt}.mdf") } else { format!("{dir}\\{fileName}") };
                    if !track.imageFileNamePaths.contains(&fileName) { track.imageFileNamePaths.push(fileName); }
                }

                s.seek(SeekFrom::Start(currPos))?;

                let mut point = track.point;
                // each session has its own 0xA0/0xA1/0xA3 track so this can't be used directly as a key
                if matches!(point, 0xA0 | 0xA1 | 0xA2) { point |= session.sessionNumber << 8; }

                if aTracks.insert(point, track.clone()).is_some() { return Err(invalidOperation(format!("An item with the same key has already been added. Key: {point}"))); }
                file.tracks.push(track);

                if footerOffset == 0 { footerOffset = file.tracks.last().unwrap().footerOffset; }
            }
        }

        // build custom session object
        file.parsedSession = Vec::new();
        for t in &sessions {
            let Some(startTrack) = aTracks.get(&t.firstTrack) else { break };
            let Some(endTrack) = aTracks.get(&t.lastTrack) else { break };
            file.parsedSession.push(Session {
                startSector: startTrack.plba,
                startTrack: t.firstTrack,
                sessionSequence: t.sessionNumber,
                endSector: endTrack.plba + endTrack.extraBlock.sectors - 1,
                endTrack: t.lastTrack,
            });
        }

        // now build the TOC object
        for se in &file.parsedSession {
            let createTOCEntryFromTrack = |track: &ATrack| ATocEntry {
                adrControl: track.adrControl,
                aFrame: track.aFrame,
                aMin: track.aMin,
                aSec: track.aSec,
                blobIndex: track.blobIndex,
                entryNum: track.trackNo,
                extraBlock: track.extraBlock,
                imageFileNamePaths: track.imageFileNamePaths.clone(),
                pFrame: track.pFrame,
                plba: track.plba as i32,
                pMin: track.pMin,
                point: track.point,
                pSec: track.pSec,
                trackMode: (track.mode & 0x7) as i32,
                sectorSize: track.sectorSize,
                session: se.sessionSequence,
                trackOffset: track.startOffset as i64,
                zero: track.zero,
                ..Default::default()
            };
            // add in the 0xA0/0xA1/0xA2 tracks
            for x in 0..3 {
                if let Some(axTrack) = aTracks.get(&(se.sessionSequence << 8 | 0xA0 | x)) { file.tocEntries.push(createTOCEntryFromTrack(axTrack)); }
            }
            // add in the rest of the tracks
            for (_, t) in aTracks.iter().filter(|(k, _)| se.startTrack <= **k && **k <= se.endTrack) { file.tocEntries.push(createTOCEntryFromTrack(t)); }
        }
        Ok(file)
    }

    /// C# `LoadMdsPath`. A parse error becomes the failure string. C# catches
    /// only `InvalidOperationException`; `parse` already returns those as `io::Error`.
    /// `RawTOCEntries` is left empty.
    pub fn loadMdsPath(vfx: &dyn crate::vfx::FileSystem, path: &str) -> LoadResults {
        let mut ret = LoadResults { mdsPath: path.to_string(), ..Default::default() };
        if !vfx.fileExists(path) {
            ret.failureException = Some("Malformed MDS format: nonexistent MDS file!".into());
            return ret;
        }
        let Some(mut s) = vfx.open(path) else {
            ret.failureException = Some("Malformed MDS format: nonexistent MDS file!".into());
            return ret;
        };
        let mut data = Vec::new();
        if let Err(e) = s.read_to_end(&mut data) {
            ret.failureException = Some(e.to_string());
            return ret;
        }
        match Self::parse(&mut Cursor::new(data), path) {
            Ok(file) => { ret.parsedMDSFile = Some(file); ret.valid = true; }
            Err(e) => ret.failureException = Some(e.to_string()),
        }
        ret
    }
}

/// C# `MdsFormat.LoadResults`. `LoadMdsPath` does not fill the raw TOC.
#[derive(Debug, Default)]
pub struct LoadResults {
    pub parsedMDSFile: Option<AFile>,
    pub valid: bool,
    pub failureException: Option<String>,
    pub mdsPath: String,
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::Cursor;

    /// 88-byte header, one session at 88, its track blocks at 112.
    fn synth(medium: i16, blocks: &[[u8; 80]]) -> Vec<u8> {
        let mut v = b"MEDIA DESCRIPTOR".to_vec();
        v.extend_from_slice(&[1, 3]);                           // version 1.3
        v.extend_from_slice(&medium.to_le_bytes());
        v.extend_from_slice(&1i16.to_le_bytes());               // session count
        v.extend_from_slice(&[0; 4 + 2 + 8 + 4 + 24 + 4 + 12]); // through structure offset
        v.extend_from_slice(&88i32.to_le_bytes());              // session offset
        v.extend_from_slice(&0i32.to_le_bytes());               // dpm offset
        v.extend_from_slice(&0i32.to_le_bytes());               // session start
        v.extend_from_slice(&1000i32.to_le_bytes());            // session end
        v.extend_from_slice(&1i16.to_le_bytes());               // session number
        v.extend_from_slice(&[(blocks.len() / 2) as u8, 0]);    // all blocks (two 80-byte blocks each, see Parse), non-track blocks
        v.extend_from_slice(&1i16.to_le_bytes());               // first track
        v.extend_from_slice(&1i16.to_le_bytes());               // last track
        v.extend_from_slice(&[0; 4]);
        v.extend_from_slice(&112i32.to_le_bytes());             // track offset
        for b in blocks { v.extend_from_slice(b); }
        v
    }

    fn block(point: u8, plba: i32) -> [u8; 80] {
        let mut b = [0u8; 80];
        b[0] = 0xA9;
        b[4] = point;
        b[16..18].copy_from_slice(&2352i16.to_le_bytes());
        b[36..40].copy_from_slice(&plba.to_le_bytes());
        b
    }

    #[test]
    fn header_and_session_are_read() {
        let f = MdsFormat::parse(&mut Cursor::new(synth(0, &[])), "a/game.mds").unwrap();
        assert_eq!(f.header.signature, "MEDIA DESCRIPTOR");
        assert_eq!(f.header.version, (1, 3));
        assert_eq!(f.header.sessionCount, 1);
        assert_eq!(f.header.sessionOffset, 88);
        assert!(f.sessions.is_empty(), "C# never fills AFile.Sessions");
        assert!(f.tracks.is_empty());
    }

    #[test]
    fn track_fields_come_from_the_80_bytes_after_the_header_read() {
        let f = MdsFormat::parse(&mut Cursor::new(synth(0, &[block(9, 9), block(1, 150)])), "game.mds").unwrap();
        assert_eq!(f.tracks.len(), 1);
        assert_eq!((f.tracks[0].point, f.tracks[0].plba, f.tracks[0].sectorSize), (1, 150, 2352));
        assert_eq!(f.parsedSession.len(), 1);
        assert_eq!(f.tocEntries.len(), 1);
        assert_eq!((f.tocEntries[0].point, f.tocEntries[0].trackMode, f.tocEntries[0].session), (1, 1, 1));
    }

    #[test]
    fn short_dvd_and_v2_descriptors_are_refused() {
        assert!(MdsFormat::parse(&mut Cursor::new(vec![0u8; 40]), "x.mds").is_err());
        assert!(MdsFormat::parse(&mut Cursor::new(synth(0x12, &[])), "x.mds").is_err());
        let mut v = synth(0, &[]);
        v[16] = 2;
        assert!(MdsFormat::parse(&mut Cursor::new(v), "x.mds").is_err());
    }

    #[test]
    fn loadMdsPathReadsAVirtualDescriptor() {
        use crate::vfx::{FileSystem, VirtualFileSystem};
        use std::collections::HashMap;
        let mut files = HashMap::new();
        files.insert("game.mds".into(), synth(0, &[block(9, 9), block(1, 150)]));
        let vfx = VirtualFileSystem::new(files);
        let loaded = MdsFormat::loadMdsPath(&vfx, "game.mds");
        assert!(loaded.valid, "{:?}", loaded.failureException);
        assert_eq!(loaded.parsedMDSFile.unwrap().tracks.len(), 1);
        let missing = MdsFormat::loadMdsPath(&vfx, "nope.mds");
        assert!(!missing.valid);
        assert!(missing.failureException.unwrap().contains("nonexistent"));
    }
}
