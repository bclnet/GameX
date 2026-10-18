// PORT-SOURCE: Vfx/OpenX.Vfx/Disc.cs (MdsFormat)
// PORT-SHA: SHARED
// PORT-SHARED: yes  (extracted from the source file above, which has its own .rs)
// PORT-STATUS: done
//
// Alcohol 120% `.mds` descriptor, paired with an `.mdf` data file.
//
// MdsFormat is ported: `Parse`, `LoadResults`, `LoadMdsPath`, `MountBlobs`,
// `EmitRawTOCEntry` and `LoadMdsToDisc`. `LoadMdsPath` leaves `RawTOCEntries`
// null, as the C# does.
//
// The Disc types those methods construct live here (`Disc`, `Blob_Raw`,
// `RawTOCEntry`, `SubchannelQ`, `SS_Gap`, the other `SS_*` jobs the loader
// builds, and `Synthesize_DiscTOCFromRawTOCEntries`). They are Disc.cs types,
// not MdsFormat. They are not in `disc.rs`. Not ported, because this path does
// not call them: `SectorSynthProvider`, `DiscSectorReader`, `SS_2364_DeinterleavedQ`,
// `SS_PatchQ`, `DiscTrack.NextTrack` linking, and the other image formats.
// No Python twin (`OpenX/python` has no disc reader).
//
// C# bugs kept:
//   * `MountBlobs` compares `Blob.ToString()`, which is the runtime type name,
//     so only the first image file is mounted.
//   * The data-sector loop walks every sector of the session for every track,
//     and it advances the blob when the sector number equals the blob's byte length.
//   * `SubchannelQ.Timestamp` stores `new MSF(value)` and drops `Negative`, so a
//     negative pregap timestamp is stored as its absolute value.
//   * Session-format detection compares `DecimalValue` with `0x10`/`0x20`.
//     `FromDecimal(0x10)` round-trips to decimal 16, which is what the switch matches.
//
// `Blob_Raw` reads the image when the path is set. C# opens the stream on the
// first `Read`. The bytes match for a file that does not change.
// `ECC_Populate` is run from `ECM_Mode1` / `ECM_Mode2_Form1` (`ecm::calc_ecc`).
//
// Not checked against a real `.mds` file. C# exceptions are `io::Error`s.

use std::cell::RefCell;
use std::collections::BTreeMap;
use std::io::{Cursor, Error, ErrorKind, Read, Result, Seek, SeekFrom};
use std::path::Path;
use std::rc::Rc;

use crate::disc::crc16_ccitt;
use crate::disc_cue::CueTrackType;
use crate::disc_synth::{deinterleave_subcode, ecm, interleave_subcode, SynthPart};
use openx_poly::log::Log;

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
    /// `RawTOCEntries` stays null.
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

    /// C# `MountBlobs`. `Blob.ToString()` is the type name, so every `Blob_Raw`
    /// compares equal and only the first image is mounted.
    pub fn mountBlobs(vfx: &dyn crate::vfx::FileSystem, mdsf: &AFile, disc: &mut Disc) -> Result<BTreeMap<i32, Rc<RefCell<Blob_Raw>>>> {
        let mut blobIndex = BTreeMap::new();
        let mut count = 0i32;
        for track in &mdsf.tracks {
            let mut seen = Vec::new();
            for file in &track.imageFileNamePaths {
                if seen.contains(file) { continue; }
                seen.push(file.clone());
                if !vfx.fileExists(file) { return Err(invalidOperation(format!("Malformed MDS format: nonexistent image file: {file}"))); }
                let blob = Rc::new(RefCell::new(Blob_Raw::open(vfx, file)?));
                let mut dupe = false;
                let name = blob.borrow().toString();
                for re in &disc.disposableResources {
                    if re.borrow().toString() == name { dupe = true; }
                }
                if !dupe {
                    disc.disposableResources.push(blob.clone());
                    blobIndex.insert(count, blob);
                    count += 1;
                }
            }
        }
        Ok(blobIndex)
    }

    /// C# `EmitRawTOCEntry`. The low nibble of `ADR_Control` is CONTROL and the
    /// high nibble is ADR; `ComputeStatus` swaps them into the Q status byte.
    /// POINT `0xA0`/`0xA1`/`0xA2` is stored as that raw byte, not as BCD.
    pub fn emitRawTOCEntry(entry: &ATocEntry) -> Result<RawTOCEntry> {
        let tno = BCD2::fromDecimal(entry.trackNo);
        let mut ino = BCD2::fromDecimal(entry.point);
        ino.bcdValue = match entry.point {
            0xA0 | 0xA1 | 0xA2 => entry.point as u8,
            _ => ino.bcdValue,
        };
        let adrc = u8::try_from(entry.adrControl).map_err(|_| invalidOperation("Value was either too large or too small for an unsigned byte.".into()))?;
        let control = adrc & 0x0F;
        let adr = adrc >> 4;
        let q = SubchannelQ {
            qStatus: SubchannelQ::computeStatus(adr as i32, EControlQ(control as i32)),
            qTno: tno,
            qIndex: ino,
            min: BCD2::fromDecimal(entry.aMin),
            sec: BCD2::fromDecimal(entry.aSec),
            frame: BCD2::fromDecimal(entry.aFrame),
            zero: entry.zero as u8,
            apMin: BCD2::fromDecimal(entry.pMin),
            apSec: BCD2::fromDecimal(entry.pSec),
            apFrame: BCD2::fromDecimal(entry.pFrame),
            qCrc: 0, // meaningless
        };
        Ok(RawTOCEntry { qData: q })
    }

    /// C# `LoadMdsToDisc`.
    pub fn loadMdsToDisc(vfx: &dyn crate::vfx::FileSystem, mdsPath: &str, discMountPolicy: DiscMountPolicy) -> Result<Disc> {
        let loadResults = Self::loadMdsPath(vfx, mdsPath);
        if !loadResults.valid {
            return Err(invalidOperation(loadResults.failureException.unwrap_or_else(|| "Object reference not set to an instance of an object.".into())));
        }
        let mut disc = Disc::default();
        let mdsf = loadResults.parsedMDSFile.ok_or_else(|| invalidOperation("Object reference not set to an instance of an object.".into()))?;
        let blobIndex = Self::mountBlobs(vfx, &mdsf, &mut disc)?;
        // generate DiscTOCRaw items from the ones specified in the MDS file
        let mut curSession = 1i32;
        disc.sessions.push(Some(DiscSession { number: curSession, ..Default::default() }));
        for entry in &mdsf.tocEntries {
            if entry.session != curSession {
                if entry.session != curSession + 1 { return Err(invalidOperation("Session incremented more than one!".into())); }
                curSession = entry.session;
                disc.sessions.push(Some(DiscSession { number: curSession, ..Default::default() }));
            }
            let raw = Self::emitRawTOCEntry(entry)?;
            let ses = disc.sessions.get_mut(curSession as usize).and_then(|s| s.as_mut()).ok_or_else(|| invalidOperation("Index was out of range.".into()))?;
            ses.rawTOCEntries.push(raw);
        }
        // analyze the RAWTocEntries to figure out what type of track track 1 is
        let toc = {
            let entries = &disc.session1().rawTOCEntries;
            Synthesize_DiscTOCFromRawTOCEntries::new(entries).run()?
        };

        let mut currBlobIndex = 0i32;
        for session in &mdsf.parsedSession {
            // leadin track
            let leadinSize = if session.sessionSequence == 1 { 0 } else { 4500 };
            let leadinType = pregapTrackType(&toc);
            for _ in 0..leadinSize {
                disc.sectors.push(SectorSynthJob2448::SS_Gap(SS_Gap {
                    base: SS_Base { policy: discMountPolicy, ..SS_Base::default() },
                    trackType: leadinType,
                }));
            }

            for i in session.startTrack..=session.endTrack {
                let mut relMSF = -1i32;
                let Some(track) = mdsf.tocEntries.iter().find(|t| t.point == i) else { break };
                // ignore the info entries
                if matches!(track.point, 0xA0 | 0xA1 | 0xA2) { continue; }
                // get the blob(s) for this track
                let tr = mdsf.tocEntries.iter().find(|a| a.point == i).ok_or_else(|| invalidOperation("BLOB Error!".into()))?;
                if tr.imageFileNamePaths.is_empty() { return Err(invalidOperation("BLOB Error!".into())); }
                let pregap = i32::try_from(track.extraBlock.pregap).map_err(|_| invalidOperation("Value was either too large or too small for an Int32.".into()))?;

                // check for track pregap and create if necessary
                if track.extraBlock.pregap > 0 {
                    let pregapType = pregapTrackType(&toc);
                    let mut pre = 0i32;
                    while (pre as i64) < track.extraBlock.pregap {
                        relMSF += 1;
                        let mut qRelMSF = pre - pregap;
                        // tweak relMSF due to ambiguity/contradiction in yellowbook docs
                        if !discMountPolicy.cuePregapContradictionModeA { qRelMSF += 1; }
                        let mut ssGap = SS_Gap {
                            base: SS_Base { policy: discMountPolicy, pause: true, ..SS_Base::default() },
                            trackType: pregapType,
                        };
                        ssGap.base.sq.setStatus(SubchannelQ::kADR, toc.tocItems[1].control);
                        ssGap.base.sq.qTno = BCD2::fromDecimal(1);
                        ssGap.base.sq.qIndex = BCD2::fromDecimal(0);
                        ssGap.base.sq.setApTimestamp(pre);
                        ssGap.base.sq.setTimestamp(qRelMSF);
                        disc.sectors.push(SectorSynthJob2448::SS_Gap(ssGap));
                        pre += 1;
                    }
                }

                // create track sectors. C# walks the whole session once per track.
                let mut currBlobOffset = track.trackOffset;
                for sector in session.startSector..=session.endSector {
                    let currBlob = blobIndex.get(&currBlobIndex).ok_or_else(|| invalidOperation(format!("The given key '{currBlobIndex}' was not present in the dictionary.")))?;
                    let currBlobLength = currBlob.borrow().length;
                    if sector == currBlobLength { currBlobIndex += 1; }
                    let mdfBlob = disc.disposableResources.get(currBlobIndex as usize).cloned().ok_or_else(|| invalidOperation("Index was out of range. Must be non-negative and less than the size of the collection.".into()))?;

                    let mut base = SS_Base { policy: discMountPolicy, blob: Some(mdfBlob), blobOffset: currBlobOffset, ..SS_Base::default() };
                    currBlobOffset += track.sectorSize as i64;

                    relMSF += 1;
                    let mut ino = BCD2::fromDecimal(track.point);
                    ino.bcdValue = match track.point {
                        0xA0 | 0xA1 | 0xA2 => track.point as u8,
                        _ => ino.bcdValue,
                    };
                    let adrc = u8::try_from(track.adrControl).map_err(|_| invalidOperation("Value was either too large or too small for an unsigned byte.".into()))?;
                    let control = adrc & 0x0F;
                    let adr = adrc >> 4;
                    let ap = i32::try_from(disc.sectors.len()).map_err(|_| invalidOperation("Value was either too large or too small for an Int32.".into()))?;
                    let mut sq = SubchannelQ {
                        qStatus: SubchannelQ::computeStatus(adr as i32, EControlQ(control as i32)),
                        qTno: BCD2::fromDecimal(track.point),
                        qIndex: ino,
                        ..SubchannelQ::default()
                    };
                    sq.setApTimestamp(ap);
                    sq.setTimestamp(relMSF - pregap);
                    base.sq = sq;
                    disc.sectors.push(dataSector(track.sectorSize, track.trackMode, base)?);
                }
            }

            // leadout track, first leadout is 6750 sectors, later ones are 2250 sectors
            let leadoutSize = if session.sessionSequence == 1 { 6750 } else { 2250 };
            for _ in 0..leadoutSize {
                disc.sectors.push(SectorSynthJob2448::SS_Leadout(SS_Leadout { sessionNumber: session.sessionSequence, policy: discMountPolicy }));
            }
        }
        Ok(disc)
    }
}

/// C# lead-in / pregap track type from TOC item 1 and the session format.
fn pregapTrackType(toc: &DiscTOC) -> CueTrackType {
    let mut pregapTrackType = CueTrackType::Audio;
    if toc.tocItems[1].isData() {
        pregapTrackType = match toc.sessionFormat {
            DiscSessionFormat::Type20_CDXA => CueTrackType::Mode2_2352,
            DiscSessionFormat::Type10_CDI => CueTrackType::Cdi_2352,
            DiscSessionFormat::Type00_CDROM_CDDA => CueTrackType::Mode1_2352,
            _ => pregapTrackType,
        };
    }
    pregapTrackType
}

fn dataSector(sectorSize: i32, trackMode: i32, base: SS_Base) -> Result<SectorSynthJob2448> {
    Ok(match (sectorSize, trackMode) {
        (2352, 1) => SectorSynthJob2448::SS_2352(SS_2352 { base }),
        (2048, 2) => SectorSynthJob2448::SS_Mode1_2048(SS_Mode1_2048 { base }),
        (2336, 0 | 3 | 7) => SectorSynthJob2448::SS_Mode2_2336(SS_Mode2_2336 { base }),
        (2048, 4) => SectorSynthJob2448::SS_Mode2_Form1_2048(SS_Mode2_Form1_2048 { base }),
        (2324, 5) => SectorSynthJob2448::SS_Mode2_Form2_2324(SS_Mode2_Form2_2324 { base }),
        (2328, 5) => SectorSynthJob2448::SS_Mode2_Form2_2328(SS_Mode2_Form2_2328 { base }),
        (2048, _) => SectorSynthJob2448::SS_Mode1_2048(SS_Mode1_2048 { base }),
        (2336, _) => SectorSynthJob2448::SS_Mode2_2336(SS_Mode2_2336 { base }),
        (2352, _) => SectorSynthJob2448::SS_2352(SS_2352 { base }),
        (2448, _) => SectorSynthJob2448::SS_2448_Interleaved(SS_2448_Interleaved { base }),
        _ => return Err(invalidOperation(format!("Not supported: Sector Size {sectorSize}, Track Mode {trackMode}"))),
    })
}

/// C# `MdsFormat.LoadResults`. `LoadMdsPath` does not fill the raw TOC; it stays null.
#[derive(Debug, Default)]
pub struct LoadResults {
    pub rawTOCEntries: Option<Vec<RawTOCEntry>>,
    pub parsedMDSFile: Option<AFile>,
    pub valid: bool,
    pub failureException: Option<String>,
    pub mdsPath: String,
}

/// C# `class DiscMountPolicy`. The field initialisers are `true`, not `false`.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct DiscMountPolicy {
    pub cuePregapContradictionModeA: bool,
    pub cuePregapMode2AsXAForm2: bool,
    pub sbiAsMednafen: bool,
}

impl Default for DiscMountPolicy {
    fn default() -> Self {
        Self { cuePregapContradictionModeA: true, cuePregapMode2AsXAForm2: true, sbiAsMednafen: true }
    }
}

impl DiscMountPolicy {
    /// C# `SetForPSX`.
    pub fn setForPSX(&mut self) {
        self.cuePregapContradictionModeA = false;
        self.cuePregapMode2AsXAForm2 = true;
        self.sbiAsMednafen = true;
    }
}

/// C# `enum DiscSessionFormat`.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum DiscSessionFormat {
    None = -1,
    Type00_CDROM_CDDA = 0x00,
    Type10_CDI = 0x10,
    Type20_CDXA = 0x20,
}

impl Default for DiscSessionFormat {
    fn default() -> Self { Self::None }
}

/// C# `[Flags] enum EControlQ`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct EControlQ(pub i32);

impl EControlQ {
    pub const NONE: Self = Self(0);
    /// Pre-emphasis enabled (audio tracks only).
    pub const PRE: Self = Self(1);
    /// Digital copy permitted.
    pub const DCP: Self = Self(2);
    /// Set for data tracks, clear for audio tracks.
    pub const DATA: Self = Self(4);
    /// Four channel audio.
    pub const _4CH: Self = Self(8);
}

impl std::ops::BitAnd for EControlQ {
    type Output = Self;
    fn bitand(self, rhs: Self) -> Self { Self(self.0 & rhs.0) }
}

impl std::ops::BitOr for EControlQ {
    type Output = Self;
    fn bitor(self, rhs: Self) -> Self { Self(self.0 | rhs.0) }
}

/// C# `IntToBCD`. Values above 99 truncate through the `(byte)` cast.
fn intToBcd(n: i32) -> u8 {
    let tens = n / 10;
    let ones = n % 10;
    ((tens << 4) | ones) as u8
}

/// C# `struct BCD2`. Named `BCD2` so it is not the range-checked `Bcd2` in `disc_addressing`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct BCD2 {
    pub bcdValue: u8,
}

impl BCD2 {
    /// C# `FromDecimal`.
    pub fn fromDecimal(d: i32) -> Self { Self { bcdValue: intToBcd(d) } }

    /// C# `FromBCD`.
    pub fn fromBcd(b: u8) -> Self { Self { bcdValue: b } }

    /// C# `DecimalValue` getter. A nibble above 9 is accepted.
    pub fn decimalValue(self) -> i32 {
        (self.bcdValue & 0xF) as i32 + ((self.bcdValue >> 4) & 0xF) as i32 * 10
    }

    /// C# `DecimalValue` setter.
    pub fn setDecimalValue(&mut self, value: i32) { self.bcdValue = intToBcd(value); }
}

/// C# `MSF(int SectorNumber)` components. `Negative` is not returned: the Timestamp setter drops it.
fn msfParts(sectorNumber: i32) -> (i32, i32, i32) {
    let n = if sectorNumber < 0 { sectorNumber.wrapping_neg() } else { sectorNumber };
    ((n / 4500) as u8 as i32, ((n / 75) % 60) as i32, (n % 75) as i32)
}

/// C# `struct SubchannelQ`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct SubchannelQ {
    pub qStatus: u8,
    pub qTno: BCD2,
    pub qIndex: BCD2,
    pub min: BCD2,
    pub sec: BCD2,
    pub frame: BCD2,
    pub zero: u8,
    pub apMin: BCD2,
    pub apSec: BCD2,
    pub apFrame: BCD2,
    pub qCrc: u16,
}

impl SubchannelQ {
    /// C# `kADR`. ADR (q-Mode) is necessarily 0x01 for a RawTOCEntry.
    pub const kADR: i32 = 1;
    /// C# `kUnknownControl`.
    pub const kUnknownControl: EControlQ = EControlQ::NONE;

    /// C# `SetStatus`.
    pub fn setStatus(&mut self, adrQmode: i32, control: EControlQ) { self.qStatus = Self::computeStatus(adrQmode, control); }

    /// C# `ComputeStatus`. ADR goes in the low nibble, CONTROL in the high nibble.
    pub fn computeStatus(adrQmode: i32, control: EControlQ) -> u8 { (adrQmode | (control.0 << 4)) as u8 }

    /// C# `ADR`.
    pub fn adr(self) -> i32 { (self.qStatus & 0xF) as i32 }

    /// C# `CONTROL`.
    pub fn control(self) -> EControlQ { EControlQ(((self.qStatus >> 4) & 0xF) as i32) }

    /// C# `Timestamp` getter.
    pub fn timestamp(self) -> i32 { self.min.decimalValue() * 4500 + self.sec.decimalValue() * 75 + self.frame.decimalValue() }

    /// C# `Timestamp` setter. `new MSF(value)` records a sign, and this assignment ignores it.
    pub fn setTimestamp(&mut self, value: i32) {
        let (min, sec, frac) = msfParts(value);
        self.min.setDecimalValue(min);
        self.sec.setDecimalValue(sec);
        self.frame.setDecimalValue(frac);
    }

    /// C# `AP_Timestamp` getter.
    pub fn apTimestamp(self) -> i32 { self.apMin.decimalValue() * 4500 + self.apSec.decimalValue() * 75 + self.apFrame.decimalValue() }

    /// C# `AP_Timestamp` setter.
    pub fn setApTimestamp(&mut self, value: i32) {
        let (min, sec, frac) = msfParts(value);
        self.apMin.setDecimalValue(min);
        self.apSec.setDecimalValue(sec);
        self.apFrame.setDecimalValue(frac);
    }
}

/// C# `class RawTOCEntry`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct RawTOCEntry {
    pub qData: SubchannelQ,
}

/// C# `DiscTrack.ETrackType`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum ETrackType {
    #[default]
    Unknown,
    Data,
    Audio,
}

/// C# `class DiscTrack`. `nextTrack` is not linked: `LoadMdsToDisc` never builds the track list.
#[derive(Debug, Clone, Default)]
pub struct DiscTrack {
    pub number: i32,
    pub mode: i32,
    pub control: EControlQ,
    pub lba: i32,
    pub nextTrack: Option<Rc<RefCell<DiscTrack>>>,
}

impl DiscTrack {
    /// C# `IsAudio`.
    pub fn isAudio(&self) -> bool { self.mode == 0 }
    /// C# `IsData`.
    pub fn isData(&self) -> bool { !self.isAudio() }
}

/// C# `DiscTOC.TOCItem`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub struct TOCItem {
    pub control: EControlQ,
    pub lba: i32,
    pub exists: bool,
}

impl TOCItem {
    /// C# `IsData`.
    pub fn isData(self) -> bool { (self.control & EControlQ::DATA).0 != 0 }
}

/// C# `class DiscTOC`. `FirstRecordedTrackNumber` and `LastRecordedTrackNumber` start at -1, and the session format starts at `None`.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct DiscTOC {
    pub firstRecordedTrackNumber: i32,
    pub lastRecordedTrackNumber: i32,
    pub sessionFormat: DiscSessionFormat,
    pub tocItems: [TOCItem; 101],
}

impl Default for DiscTOC {
    fn default() -> Self {
        Self {
            firstRecordedTrackNumber: -1,
            lastRecordedTrackNumber: -1,
            sessionFormat: DiscSessionFormat::None,
            tocItems: [TOCItem::default(); 101],
        }
    }
}

impl DiscTOC {
    /// C# `LeadoutLBA`.
    pub fn leadoutLBA(&self) -> i32 { self.tocItems[100].lba }
}

/// C# `class DiscSession`.
#[derive(Debug, Clone, Default)]
pub struct DiscSession {
    pub toc: Option<DiscTOC>,
    pub rawTOCEntries: Vec<RawTOCEntry>,
    pub number: i32,
    pub tracks: Vec<DiscTrack>,
}

impl DiscSession {
    /// C# `InformationTrackCount`.
    pub fn informationTrackCount(&self) -> i32 { self.tracks.len() as i32 - 2 }
    /// C# `LeadoutLBA`.
    pub fn leadoutLBA(&self) -> i32 { self.leadoutTrack().lba }
    /// C# `FirstInformationTrack`.
    pub fn firstInformationTrack(&self) -> &DiscTrack { &self.tracks[1] }
    /// C# `LastInformationTrack`.
    pub fn lastInformationTrack(&self) -> &DiscTrack { &self.tracks[self.informationTrackCount() as usize] }
    /// C# `LeadoutTrack`.
    pub fn leadoutTrack(&self) -> &DiscTrack { &self.tracks[self.tracks.len() - 1] }
    /// C# `LeadinTrack`.
    pub fn leadinTrack(&self) -> &DiscTrack { &self.tracks[0] }

    /// C# `SeekTrack`.
    pub fn seekTrack(&self, lba: i32) -> &DiscTrack {
        for i in 1..self.tracks.len() {
            if self.tracks[i].lba > lba { return &self.tracks[i - 1]; }
        }
        self.leadoutTrack()
    }
}

/// C# `class Disc`. `Sessions` is 1-indexed: index 0 is null.
/// `SynthProvider` and `SectorSynthParams` are not stored.
#[derive(Debug, Clone)]
pub struct Disc {
    pub sessions: Vec<Option<DiscSession>>,
    pub name: String,
    /// C# values are `object`. This path never reads the map.
    pub memos: BTreeMap<String, String>,
    pub disposableResources: Vec<Rc<RefCell<Blob_Raw>>>,
    pub sectors: Vec<SectorSynthJob2448>,
}

impl Default for Disc {
    fn default() -> Self {
        Self { sessions: vec![None], name: String::new(), memos: BTreeMap::new(), disposableResources: Vec::new(), sectors: Vec::new() }
    }
}

impl Disc {
    /// C# `Session1`.
    pub fn session1(&self) -> &DiscSession { self.sessions[1].as_ref().expect("Object reference not set to an instance of an object.") }
    /// C# `TOC`.
    pub fn toc(&self) -> &DiscTOC { self.session1().toc.as_ref().expect("Object reference not set to an instance of an object.") }

    /// C# `Dispose`. Closes every mounted blob. Later `Read` calls fail the way a disposed stream does.
    pub fn dispose(&mut self) {
        for s in &self.disposableResources { s.borrow_mut().dispose(); }
    }
}

/// C# `class Blob_Raw`.
#[derive(Debug, Clone)]
pub struct Blob_Raw {
    path: String,
    pub length: i64,
    data: Vec<u8>,
    live: bool,
}

impl Blob_Raw {
    /// C# `Path` setter: `Length` comes from `FileInfo`. The bytes are read here; C# waits for `Read`.
    pub fn open(vfx: &dyn crate::vfx::FileSystem, path: &str) -> Result<Self> {
        let info = vfx.fileInfo(path).ok_or_else(|| invalidOperation("Object reference not set to an instance of an object.".into()))?;
        let mut data = Vec::new();
        if let Some(mut s) = vfx.open(path) { s.read_to_end(&mut data)?; }
        Ok(Self { path: path.to_string(), length: info.length as i64, data, live: true })
    }

    /// C# `Path` getter.
    pub fn path(&self) -> &str { &self.path }

    /// C# `object.ToString()` for this class: the runtime type name, not the path.
    pub fn toString(&self) -> &'static str { "OpenX.Vfx.Disc.Blob_Raw" }

    /// C# `Dispose`.
    pub fn dispose(&mut self) { self.data.clear(); self.live = false; }

    /// C# `Read`. A disposed blob has a null stream.
    pub fn read(&self, bytePos: i64, buffer: &mut [u8]) -> usize {
        if !self.live { panic!("Object reference not set to an instance of an object."); }
        if bytePos < 0 { panic!("ArgumentOutOfRangeException"); }
        let start = bytePos as usize;
        if start >= self.data.len() { return 0; }
        let n = buffer.len().min(self.data.len() - start);
        buffer[..n].copy_from_slice(&self.data[start..start + n]);
        n
    }
}

/// C# `class SectorSynthJob`. `Params` is an empty struct and is not stored.
pub struct SectorSynthJob<'a> {
    pub lba: i32,
    pub parts: SynthPart,
    pub destBuffer2448: &'a mut [u8],
    pub destOffset: usize,
    pub disc: &'a Disc,
}

/// C# `abstract class SectorSynthJob2448`, as the list `Disc.Sectors` holds.
#[derive(Debug, Clone)]
pub enum SectorSynthJob2448 {
    SS_Gap(SS_Gap),
    SS_2352(SS_2352),
    SS_Mode1_2048(SS_Mode1_2048),
    SS_Mode2_2336(SS_Mode2_2336),
    SS_Mode2_Form1_2048(SS_Mode2_Form1_2048),
    SS_Mode2_Form2_2324(SS_Mode2_Form2_2324),
    SS_Mode2_Form2_2328(SS_Mode2_Form2_2328),
    SS_2448_Interleaved(SS_2448_Interleaved),
    SS_Leadout(SS_Leadout),
}

impl SectorSynthJob2448 {
    /// C# `Synth`.
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        match self {
            Self::SS_Gap(s) => s.synth(job),
            Self::SS_2352(s) => s.synth(job),
            Self::SS_Mode1_2048(s) => s.synth(job),
            Self::SS_Mode2_2336(s) => s.synth(job),
            Self::SS_Mode2_Form1_2048(s) => s.synth(job),
            Self::SS_Mode2_Form2_2324(s) => s.synth(job),
            Self::SS_Mode2_Form2_2328(s) => s.synth(job),
            Self::SS_2448_Interleaved(s) => s.synth(job),
            Self::SS_Leadout(s) => s.synth(job),
        }
    }
}

/// C# `abstract class SS_Base`.
#[derive(Debug, Clone, Default)]
pub struct SS_Base {
    pub blob: Option<Rc<RefCell<Blob_Raw>>>,
    pub blobOffset: i64,
    pub policy: DiscMountPolicy,
    pub sq: SubchannelQ,
    pub pause: bool,
}

impl SS_Base {
    /// C# `SynthSubchannelAsNeed`.
    fn synthSubchannel(&self, job: &mut SectorSynthJob) {
        let o = job.destOffset;
        if job.parts.contains(SynthPart::SUBCHANNEL_P) {
            let val = if self.pause { 0xFF } else { 0x00 };
            job.destBuffer2448[o + 2352..o + 2352 + 12].fill(val);
        }
        if job.parts.contains(SynthPart::SUBCHANNEL_Q) { subQSerialize(job.destBuffer2448, o + 2352 + 12, &self.sq); }
        if job.parts.contains(SynthPart::SUBCHANNEL_RSTUVW) { job.destBuffer2448[o + 2376..o + 2376 + 72].fill(0); }
        // subcode has been generated deinterleaved; we may still need to interleave it
        if job.parts.contains(SynthPart::SUBCODE_ANY) && !job.parts.contains(SynthPart::SUBCODE_DEINTERLEAVE) {
            interleave_subcode(job.destBuffer2448, o + 2352).expect("Index was out of range.");
        }
    }
}

fn readAt(blob: &Option<Rc<RefCell<Blob_Raw>>>, pos: i64, dest: &mut [u8]) {
    blob.as_ref().expect("Object reference not set to an instance of an object.").borrow().read(pos, dest);
}

/// C# `SynthUtils.SubQ_Serialize`. The CRC is stored inverted and big-endian.
fn subQSerialize(buf: &mut [u8], offset: usize, sq: &SubchannelQ) -> u16 {
    buf[offset] = sq.qStatus;
    buf[offset + 1] = sq.qTno.bcdValue;
    buf[offset + 2] = sq.qIndex.bcdValue;
    buf[offset + 3] = sq.min.bcdValue;
    buf[offset + 4] = sq.sec.bcdValue;
    buf[offset + 5] = sq.frame.bcdValue;
    buf[offset + 6] = sq.zero;
    buf[offset + 7] = sq.apMin.bcdValue;
    buf[offset + 8] = sq.apSec.bcdValue;
    buf[offset + 9] = sq.apFrame.bcdValue;
    let crc = crc16_ccitt::calculate(&buf[offset..offset + 10]);
    buf[offset + 10] = !(crc >> 8) as u8;
    buf[offset + 11] = !(crc) as u8;
    crc
}

/// C# `SynthUtils.SectorHeader`. `new MSF(lba + 150)`, then `IntToBCD` of each component.
fn sectorHeader(buf: &mut [u8], offset: usize, lba: i32, mode: u8) {
    buf[offset] = 0x00;
    for i in 1..11 { buf[offset + i] = 0xFF; }
    buf[offset + 11] = 0x00;
    let (min, sec, frac) = msfParts(lba + 150);
    buf[offset + 12] = intToBcd(min);
    buf[offset + 13] = intToBcd(sec);
    buf[offset + 14] = intToBcd(frac);
    buf[offset + 15] = mode;
}

/// C# `SynthUtils.SectorSubHeader`. Form 2 writes `0x20` at bytes 2 and 5; form 1 stays zero.
fn sectorSubHeader(buf: &mut [u8], offset: usize, form: u8) {
    for i in 0..8 { buf[offset + i] = 0; }
    if form == 2 {
        buf[offset + 2] = 0x20;
        buf[offset + 5] = 0x20;
    }
}

fn pokeEdc(buf: &mut [u8], at: usize, edc: u32) { buf[at..at + 4].copy_from_slice(&edc.to_le_bytes()); }

/// C# `EDC_Mode1`.
fn edcMode1(buf: &mut [u8], offset: usize) {
    let edc = ecm::edc_calc(&buf[offset..offset + 2064]);
    pokeEdc(buf, offset + 2064, edc);
}

/// C# `EDC_Mode2_Form1`.
fn edcMode2Form1(buf: &mut [u8], offset: usize) {
    let edc = ecm::edc_calc(&buf[offset + 16..offset + 16 + 2048 + 8]);
    pokeEdc(buf, offset + 2072, edc);
}

/// C# `EDC_Mode2_Form2`.
fn edcMode2Form2(buf: &mut [u8], offset: usize) {
    let edc = ecm::edc_calc(&buf[offset + 16..offset + 16 + 2324 + 8]);
    pokeEdc(buf, offset + 2348, edc);
}

/// C# `ECM.ECC_Populate`. `src` and `dest` are the same buffer, as every caller here passes.
fn eccPopulate(buf: &mut [u8], offset: usize, zeroSectorAddress: bool) {
    let address = (buf[offset + 12] as u32) | ((buf[offset + 13] as u32) << 8) | ((buf[offset + 14] as u32) << 16);
    if zeroSectorAddress {
        buf[offset + 12] = 0;
        buf[offset + 13] = 0;
        buf[offset + 14] = 0;
    }
    let base = offset + 12;
    for col in 0..86 {
        let (p0, p1) = ecm::calc_ecc(buf, base, col, 86, 24).expect("Index was out of range.");
        buf[base + 1032 * 2 + col] = p0;
        buf[base + 1032 * 2 + col + 43 * 2] = p1;
    }
    for d in 0..26 {
        for w in 0..2 {
            let (p0, p1) = ecm::calc_ecc(buf, base, d * 86 + w, 88, 43).expect("Index was out of range.");
            buf[base + 1118 * 2 + d * 2 + w] = p0;
            buf[base + 1118 * 2 + d * 2 + w + 26 * 2] = p1;
        }
    }
    buf[offset + 12] = address as u8;
    buf[offset + 13] = (address >> 8) as u8;
    buf[offset + 14] = (address >> 16) as u8;
}

/// C# `ECM_Mode1`.
fn ecmMode1(buf: &mut [u8], offset: usize) {
    edcMode1(buf, offset);
    for i in 0..8 { buf[offset + 2068 + i] = 0; }
    eccPopulate(buf, offset, false);
}

/// C# `ECM_Mode2_Form1`.
fn ecmMode2Form1(buf: &mut [u8], offset: usize) {
    sectorSubHeader(buf, offset + 16, 1);
    edcMode2Form1(buf, offset);
    eccPopulate(buf, offset, false);
}

/// C# `ECM_Mode2_Form2`. Mode 2 Form 2 has no ECC.
fn ecmMode2Form2(buf: &mut [u8], offset: usize) {
    sectorSubHeader(buf, offset + 16, 2);
    edcMode2Form2(buf, offset);
}

/// C# `class SS_Mode1_2048`.
#[derive(Debug, Clone, Default)]
pub struct SS_Mode1_2048 { pub base: SS_Base }

impl SS_Mode1_2048 {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let ecm = job.parts.contains(SynthPart::ECM_ANY);
        if ecm { job.parts = job.parts | SynthPart::USER2048 | SynthPart::HEADER16; }
        let o = job.destOffset;
        if job.parts.contains(SynthPart::USER2048) { readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o + 16..o + 16 + 2048]); }
        if job.parts.contains(SynthPart::HEADER16) { sectorHeader(job.destBuffer2448, o, job.lba, 1); }
        if ecm { ecmMode1(job.destBuffer2448, o); }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_Mode2_2336`.
#[derive(Debug, Clone, Default)]
pub struct SS_Mode2_2336 { pub base: SS_Base }

impl SS_Mode2_2336 {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let o = job.destOffset;
        if job.parts.contains(SynthPart::USER2336) { readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o + 16..o + 16 + 2336]); }
        if job.parts.contains(SynthPart::HEADER16) { sectorHeader(job.destBuffer2448, o, job.lba, 2); }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_2352`.
#[derive(Debug, Clone, Default)]
pub struct SS_2352 { pub base: SS_Base }

impl SS_2352 {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let o = job.destOffset;
        if job.parts.contains(SynthPart::USER2352) { readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o..o + 2352]); }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_Gap`.
#[derive(Debug, Clone, Default)]
pub struct SS_Gap {
    pub base: SS_Base,
    pub trackType: CueTrackType,
}

impl SS_Gap {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let dest = job.destOffset;
        job.destBuffer2448[dest..dest + 2352].fill(0);
        let mut mode: i32;
        let mut form = -1i32;
        match self.trackType {
            CueTrackType::Audio => mode = 0,
            CueTrackType::Cdi_2352 | CueTrackType::Mode1_2352 => mode = 1,
            CueTrackType::Cdi_2336 | CueTrackType::Mode2_2336 | CueTrackType::Mode2_2352 => {
                mode = 2;
                if self.base.policy.cuePregapMode2AsXAForm2 {
                    // C# writes these two bytes itself. It is not `SectorSubHeader` (that one uses +2 and +5).
                    job.destBuffer2448[dest + 12 + 6] = 0x20;
                    job.destBuffer2448[dest + 12 + 10] = 0x20;
                }
                form = 2;
            }
            CueTrackType::Mode1_2048 => { mode = 1; self.base.pause = true; }
            _ => panic!("Not supported: {:?}", self.trackType),
        }
        if mode != 0 && job.parts.contains(SynthPart::HEADER16) { sectorHeader(job.destBuffer2448, dest, job.lba, mode as u8); }
        match mode {
            1 if job.parts.contains(SynthPart::ECM_ANY) => ecmMode1(job.destBuffer2448, dest),
            2 if form == 2 => edcMode2Form2(job.destBuffer2448, dest),
            _ => {}
        }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_Mode2_Form1_2048`.
#[derive(Debug, Clone, Default)]
pub struct SS_Mode2_Form1_2048 { pub base: SS_Base }

impl SS_Mode2_Form1_2048 {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let ecm = job.parts.contains(SynthPart::ECM_ANY);
        if ecm { job.parts = job.parts | SynthPart::USER2048 | SynthPart::HEADER16; }
        let o = job.destOffset;
        if job.parts.contains(SynthPart::USER2048) { readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o + 24..o + 24 + 2048]); }
        if job.parts.contains(SynthPart::HEADER16) { sectorHeader(job.destBuffer2448, o, job.lba, 2); }
        if ecm { ecmMode2Form1(job.destBuffer2448, o); }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_Mode2_Form2_2324`.
#[derive(Debug, Clone, Default)]
pub struct SS_Mode2_Form2_2324 { pub base: SS_Base }

impl SS_Mode2_Form2_2324 {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let o = job.destOffset;
        if job.parts.contains(SynthPart::USER2336) {
            readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o + 24..o + 24 + 2324]);
            ecmMode2Form2(job.destBuffer2448, o);
        }
        if job.parts.contains(SynthPart::HEADER16) { sectorHeader(job.destBuffer2448, o, job.lba, 2); }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_Mode2_Form2_2328`.
#[derive(Debug, Clone, Default)]
pub struct SS_Mode2_Form2_2328 { pub base: SS_Base }

impl SS_Mode2_Form2_2328 {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let o = job.destOffset;
        if job.parts.contains(SynthPart::USER2336) {
            readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o + 24..o + 24 + 2328]);
            sectorSubHeader(job.destBuffer2448, o + 16, 2);
        }
        if job.parts.contains(SynthPart::HEADER16) { sectorHeader(job.destBuffer2448, o, job.lba, 2); }
        self.base.synthSubchannel(job);
    }
}

/// C# `class SS_2448_Interleaved`.
#[derive(Debug, Clone, Default)]
pub struct SS_2448_Interleaved { pub base: SS_Base }

impl SS_2448_Interleaved {
    pub fn synth(&mut self, job: &mut SectorSynthJob) {
        let o = job.destOffset;
        readAt(&self.base.blob, self.base.blobOffset, &mut job.destBuffer2448[o..o + 2448]);
        if job.parts.contains(SynthPart::SUBCODE_DEINTERLEAVE) {
            deinterleave_subcode(job.destBuffer2448, o + 2352).expect("Index was out of range.");
        }
    }
}

/// C# `class SS_Leadout`.
#[derive(Debug, Clone, Copy, Default)]
pub struct SS_Leadout {
    pub sessionNumber: i32,
    pub policy: DiscMountPolicy,
}

impl SS_Leadout {
    pub fn synth(&self, job: &mut SectorSynthJob) {
        let (ts, control, isData, sessionFormat) = {
            let ses = job.disc.sessions.get(self.sessionNumber as usize).and_then(|s| s.as_ref()).expect("Object reference not set to an instance of an object.");
            let lead = ses.leadoutTrack();
            let last = ses.lastInformationTrack();
            let control = EControlQ(lead.control.0 | (last.control.0 & 4));
            let isData = lead.isData();
            (job.lba - lead.lba, control, isData, job.disc.toc().sessionFormat)
        };
        let mut sq = SubchannelQ::default();
        sq.setStatus(SubchannelQ::kADR, control);
        sq.qTno.bcdValue = 0xAA;
        sq.qIndex.bcdValue = 0x01;
        sq.setTimestamp(ts);
        sq.setApTimestamp(job.lba);
        sq.zero = 0;
        let mut trackType = CueTrackType::Audio;
        if isData {
            trackType = match sessionFormat {
                DiscSessionFormat::Type20_CDXA | DiscSessionFormat::Type10_CDI => CueTrackType::Mode2_2352,
                _ => CueTrackType::Mode1_2352,
            };
        }
        let mut gap = SS_Gap { base: SS_Base { policy: self.policy, sq, pause: true, ..SS_Base::default() }, trackType };
        gap.synth(job);
    }
}

/// C# `class Synthesize_DiscTOCFromRawTOCEntries`.
pub struct Synthesize_DiscTOCFromRawTOCEntries<'a> {
    entries: &'a [RawTOCEntry],
}

impl<'a> Synthesize_DiscTOCFromRawTOCEntries<'a> {
    pub fn new(entries: &'a [RawTOCEntry]) -> Self { Self { entries } }

    /// C# `Run`.
    pub fn run(&self) -> Result<DiscTOC> {
        let mut r = DiscTOC::default();
        r.tocItems[0].lba = 0; // arguably could be -150, but let's not just yet
        r.tocItems[0].control = EControlQ::NONE;
        r.tocItems[0].exists = false;
        let mut minFoundTrack = 100i32;
        let mut maxFoundTrack = 1i32;
        for te in self.entries {
            let q = te.qData;
            let point = q.qIndex.decimalValue();
            // see ECMD-394 page 5-14 for info about point = 0xA0, 0xA1, 0xA2
            match point {
                0x00 => Log::info("unexpected POINT=00 in lead-in Q-channel"),
                255 => return Err(invalidOperation("point == 255".into())),
                1..=99 => {
                    minFoundTrack = minFoundTrack.min(point);
                    maxFoundTrack = maxFoundTrack.max(point);
                    r.tocItems[point as usize].lba = q.apTimestamp() - 150;
                    r.tocItems[point as usize].control = q.control();
                    r.tocItems[point as usize].exists = true;
                }
                // 0xA0 bcd
                100 => {
                    r.firstRecordedTrackNumber = q.apMin.decimalValue();
                    if q.apFrame.decimalValue() != 0 { Log::info("PFRAME should be 0 for POINT=0xA0"); }
                    match q.apSec.decimalValue() {
                        0x00 => r.sessionFormat = DiscSessionFormat::Type00_CDROM_CDDA,
                        0x10 => r.sessionFormat = DiscSessionFormat::Type10_CDI,
                        0x20 => r.sessionFormat = DiscSessionFormat::Type20_CDXA,
                        _ => Log::info("Unrecognized session format: PSEC should be one of {0x00,0x10,0x20} for POINT=0xA0"),
                    }
                }
                // 0xA1 bcd
                101 => {
                    r.lastRecordedTrackNumber = q.apMin.decimalValue();
                    if q.apSec.decimalValue() != 0 { Log::info("PSEC should be 0 for POINT=0xA1"); }
                    if q.apFrame.decimalValue() != 0 { Log::info("PFRAME should be 0 for POINT=0xA1"); }
                }
                // 0xA2 bcd
                102 => {
                    r.tocItems[100].lba = q.apTimestamp() - 150;
                    r.tocItems[100].control = EControlQ::NONE;
                    r.tocItems[100].exists = true;
                }
                _ => {}
            }
        }
        if r.firstRecordedTrackNumber == -1 { r.firstRecordedTrackNumber = if minFoundTrack == 100 { 1 } else { minFoundTrack }; }
        if r.lastRecordedTrackNumber == -1 { r.lastRecordedTrackNumber = maxFoundTrack; }
        if r.sessionFormat == DiscSessionFormat::None { r.sessionFormat = DiscSessionFormat::Type00_CDROM_CDDA; }
        Ok(r)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::cell::RefCell;
    use std::io::Cursor;
    use std::rc::Rc;

    use crate::disc_cue::CueTrackType;
    use crate::disc_synth::{ecm, SynthPart};
    use crate::vfx::{FileSystem, VirtualFileSystem};

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
        assert!(loaded_raw_is_null(&vfx));
    }

    fn loaded_raw_is_null(vfx: &VirtualFileSystem) -> bool {
        MdsFormat::loadMdsPath(vfx, "game.mds").rawTOCEntries.is_none()
    }

    /// One session, one track, an 8-byte extra block, a footer, and `t.mdf`.
    fn imageMds(pregap: i32, sectors: i32, adr: u8, sectorSize: i16) -> Vec<u8> {
        let (extraAt, footerAt, nameAt) = (272i32, 280i32, 288i32);
        let mut fields = [0u8; 80];
        fields[0] = 0xA9;
        fields[2] = adr;
        fields[3] = 1;
        fields[4] = 1;
        fields[12..16].copy_from_slice(&extraAt.to_le_bytes());
        fields[16..18].copy_from_slice(&sectorSize.to_le_bytes());
        fields[48..52].copy_from_slice(&1i32.to_le_bytes());
        fields[52..56].copy_from_slice(&footerAt.to_le_bytes());
        let mut v = synth(0, &[[0u8; 80], fields]);
        assert_eq!(v.len(), 272, "track bytes must end where the extra block starts");
        v.extend_from_slice(&pregap.to_le_bytes());
        v.extend_from_slice(&sectors.to_le_bytes());
        v.extend_from_slice(&nameAt.to_le_bytes());
        v.extend_from_slice(&0i32.to_le_bytes());
        v.extend_from_slice(b"t.mdf\0");
        v
    }

    fn vfs_with(mds: Vec<u8>, mdf: Vec<u8>) -> (VirtualFileSystem, String) {
        use std::collections::HashMap;
        let mut files = HashMap::new();
        files.insert("game.mds".into(), mds);
        let image = "\\t.mdf".to_string();
        files.insert(image.clone(), mdf);
        (VirtualFileSystem::new(files), image)
    }

    #[test]
    fn emitRawTOCEntry_swaps_adr_and_stores_a0_as_a_raw_point() {
        let mut e = ATocEntry::default();
        e.trackNo = 1;
        e.point = 0xA0;
        e.adrControl = 0x14;
        e.aMin = 1;
        e.aSec = 2;
        e.aFrame = 3;
        e.zero = 7;
        e.pMin = 1;
        e.pSec = 0x10; // decimal 16, which FromDecimal stores as BCD 0x16 and reads back as 0x10
        e.pFrame = 0;
        let q = MdsFormat::emitRawTOCEntry(&e).unwrap().qData;
        assert_eq!(q.qStatus, 0x41, "ADR in the low nibble, CONTROL in the high");
        assert_eq!(q.qTno.bcdValue, 0x01);
        assert_eq!(q.qIndex.bcdValue, 0xA0, "POINT 0xA0 is not passed through FromDecimal");
        assert_eq!((q.min.bcdValue, q.sec.bcdValue, q.frame.bcdValue, q.zero), (0x01, 0x02, 0x03, 7));
        assert_eq!(q.qIndex.decimalValue(), 100);
        let toc = Synthesize_DiscTOCFromRawTOCEntries::new(&[RawTOCEntry { qData: q }]).run().unwrap();
        assert_eq!(toc.firstRecordedTrackNumber, 1);
        assert_eq!(toc.sessionFormat, DiscSessionFormat::Type10_CDI);
    }

    #[test]
    fn synthesize_reads_a_track_lba_as_ap_timestamp_minus_150() {
        let mut e = ATocEntry::default();
        e.trackNo = 1;
        e.point = 1;
        e.adrControl = 0x14;
        e.pMin = 0;
        e.pSec = 2;
        e.pFrame = 0;
        let raw = MdsFormat::emitRawTOCEntry(&e).unwrap();
        assert_eq!(raw.qData.apTimestamp(), 150);
        let toc = Synthesize_DiscTOCFromRawTOCEntries::new(&[raw]).run().unwrap();
        assert!(toc.tocItems[1].exists);
        assert_eq!(toc.tocItems[1].lba, 0);
        assert!(toc.tocItems[1].isData());
        assert_eq!(toc.sessionFormat, DiscSessionFormat::Type00_CDROM_CDDA, "no A0 entry falls back to CD-ROM");
        assert_eq!((toc.firstRecordedTrackNumber, toc.lastRecordedTrackNumber), (1, 1));
    }

    #[test]
    fn mountBlobs_keeps_only_the_first_image_because_toString_is_the_type_name() {
        use crate::vfx::VirtualFileSystem;
        use std::collections::HashMap;
        let mut files = HashMap::new();
        files.insert("a.mdf".into(), vec![1, 2, 3, 4]);
        files.insert("b.mdf".into(), vec![9, 9]);
        let vfx = VirtualFileSystem::new(files);
        let mut a = AFile::default();
        let mut t = ATrack::default();
        t.imageFileNamePaths = vec!["a.mdf".into(), "b.mdf".into()];
        a.tracks.push(t);
        let mut disc = Disc::default();
        let idx = MdsFormat::mountBlobs(&vfx, &a, &mut disc).unwrap();
        assert_eq!(idx.len(), 1);
        assert_eq!(disc.disposableResources.len(), 1);
        assert_eq!(idx[&0].borrow().path(), "a.mdf");
        assert_eq!(idx[&0].borrow().length, 4);
        assert_eq!(idx[&0].borrow().toString(), "OpenX.Vfx.Disc.Blob_Raw");
        let mut missing = AFile::default();
        let mut t2 = ATrack::default();
        t2.imageFileNamePaths = vec!["nope.mdf".into()];
        missing.tracks.push(t2);
        let err = MdsFormat::mountBlobs(&vfx, &missing, &mut Disc::default()).unwrap_err();
        assert!(err.to_string().contains("nonexistent image file"), "{err}");
    }

    #[test]
    fn loadMdsToDisc_builds_session_sectors_and_a_6750_leadout() {
        use crate::vfx::FileSystem;
        let mdf = vec![0x11u8; 16];
        let (vfx, _) = vfs_with(imageMds(0, 2, 0x14, 2352), mdf);
        let mut policy = DiscMountPolicy::default();
        policy.sbiAsMednafen = false;
        let disc = MdsFormat::loadMdsToDisc(&vfx, "game.mds", policy).unwrap();
        assert_eq!(disc.sessions.len(), 2, "index 0 stays null");
        assert!(disc.sessions[0].is_none());
        assert_eq!(disc.session1().number, 1);
        assert_eq!(disc.session1().rawTOCEntries.len(), 1);
        assert_eq!(disc.disposableResources.len(), 1);
        assert_eq!(disc.disposableResources[0].borrow().length, 16);
        assert_eq!(disc.sectors.len(), 2 + 6750);
        match &disc.sectors[0] {
            SectorSynthJob2448::SS_2352(s) => {
                assert!(!s.base.policy.sbiAsMednafen);
                assert_eq!(s.base.blobOffset, 0);
                assert_eq!(s.base.sq.qStatus, 0x41);
                assert_eq!(s.base.sq.qTno.bcdValue, 0x01);
                assert_eq!(s.base.sq.timestamp(), 0);
                assert_eq!(s.base.sq.apTimestamp(), 0);
                assert!(!s.base.pause);
            }
            other => panic!("sector 0 is {other:?}"),
        }
        match &disc.sectors[1] {
            SectorSynthJob2448::SS_2352(s) => {
                assert_eq!(s.base.blobOffset, 2352);
                assert_eq!(s.base.sq.timestamp(), 1);
                assert_eq!(s.base.sq.apTimestamp(), 1);
            }
            other => panic!("sector 1 is {other:?}"),
        }
        match disc.sectors.last() {
            Some(SectorSynthJob2448::SS_Leadout(s)) => assert_eq!(s.sessionNumber, 1),
            other => panic!("leadout is {other:?}"),
        }
        let err = MdsFormat::loadMdsToDisc(&vfx, "nope.mds", policy).unwrap_err();
        assert!(err.to_string().contains("nonexistent"), "{err}");
    }

    #[test]
    fn pregap_timestamp_drops_the_sign_unless_the_psx_tweak_is_on() {
        let (vfx, _) = vfs_with(imageMds(2, 1, 0x10, 2352), vec![0; 8]);
        let disc = MdsFormat::loadMdsToDisc(&vfx, "game.mds", DiscMountPolicy::default()).unwrap();
        // Two audio gaps, one data sector, then the 6750-sector lead-out.
        assert_eq!(disc.sectors.len(), 3 + 6750);
        match &disc.sectors[0] {
            SectorSynthJob2448::SS_Gap(s) => {
                assert_eq!(s.trackType, CueTrackType::Audio);
                assert!(s.base.pause);
                assert_eq!(s.base.sq.qStatus, 0x01);
                assert_eq!(s.base.sq.qTno.bcdValue, 0x01);
                assert_eq!(s.base.sq.qIndex.bcdValue, 0x00);
                assert_eq!(s.base.sq.timestamp(), 2, "qRelMSF -2 is stored as its absolute MSF");
                assert_eq!(s.base.sq.apTimestamp(), 0);
            }
            other => panic!("{other:?}"),
        }
        match &disc.sectors[1] {
            SectorSynthJob2448::SS_Gap(s) => assert_eq!(s.base.sq.timestamp(), 1),
            other => panic!("{other:?}"),
        }
        match &disc.sectors[2] {
            SectorSynthJob2448::SS_2352(s) => assert_eq!(s.base.sq.timestamp(), 0),
            other => panic!("{other:?}"),
        }
        let mut psx = DiscMountPolicy::default();
        psx.setForPSX();
        let disc = MdsFormat::loadMdsToDisc(&vfx, "game.mds", psx).unwrap();
        match &disc.sectors[0] {
            SectorSynthJob2448::SS_Gap(s) => assert_eq!(s.base.sq.timestamp(), 1, "qRelMSF -2 + 1"),
            other => panic!("{other:?}"),
        }
    }

    #[test]
    fn a_missing_image_and_a_bad_sector_size_and_a_session_jump_fail() {
        use crate::vfx::VirtualFileSystem;
        use std::collections::HashMap;
        let mut files = HashMap::new();
        files.insert("game.mds".into(), imageMds(0, 1, 0x14, 2352));
        let vfx = VirtualFileSystem::new(files);
        let err = MdsFormat::loadMdsToDisc(&vfx, "game.mds", DiscMountPolicy::default()).unwrap_err();
        assert!(err.to_string().contains("nonexistent image file"), "{err}");

        let (vfx, _) = vfs_with(imageMds(0, 1, 0x14, 100), vec![0; 4]);
        let err = MdsFormat::loadMdsToDisc(&vfx, "game.mds", DiscMountPolicy::default()).unwrap_err();
        assert!(err.to_string().contains("Not supported: Sector Size 100, Track Mode 1"), "{err}");

        // Two sessions numbered 1 and 3, each with its own track and no image name.
        let mut v = b"MEDIA DESCRIPTOR".to_vec();
        v.extend_from_slice(&[1, 3, 0, 0]);
        v.extend_from_slice(&2i16.to_le_bytes());
        v.extend_from_slice(&[0; 4 + 2 + 8 + 4 + 24 + 4 + 12]);
        v.extend_from_slice(&88i32.to_le_bytes());
        v.extend_from_slice(&0i32.to_le_bytes());
        let mut session = |number: i16, track: i16, at: i32| {
            v.extend_from_slice(&0i32.to_le_bytes());
            v.extend_from_slice(&0i32.to_le_bytes());
            v.extend_from_slice(&number.to_le_bytes());
            v.push(1);
            v.push(0);
            v.extend_from_slice(&track.to_le_bytes());
            v.extend_from_slice(&track.to_le_bytes());
            v.extend_from_slice(&[0; 4]);
            v.extend_from_slice(&at.to_le_bytes());
        };
        session(1, 1, 136);
        session(3, 2, 296);
        let mut block = |point: u8| {
            v.extend_from_slice(&[0u8; 80]);
            let mut f = [0u8; 80];
            f[4] = point;
            v.extend_from_slice(&f);
        };
        block(1);
        block(2);
        let mut files = HashMap::new();
        files.insert("jump.mds".into(), v);
        let vfx = VirtualFileSystem::new(files);
        let err = MdsFormat::loadMdsToDisc(&vfx, "jump.mds", DiscMountPolicy::default()).unwrap_err();
        assert!(err.to_string().contains("Session incremented more than one"), "{err}");
    }

    #[test]
    fn gap_synth_writes_the_mode1_header_edc_and_deinterleaved_q() {
        let disc = Disc::default();
        let mut buf = vec![0u8; 2448];
        let mut gap = SS_Gap { trackType: CueTrackType::Mode1_2352, ..SS_Gap::default() };
        gap.base.sq.qStatus = 0x41;
        gap.base.sq.qTno = BCD2::fromDecimal(1);
        gap.base.pause = true;
        let mut job = SectorSynthJob {
            lba: 0,
            parts: SynthPart::HEADER16 | SynthPart::ECM_ANY | SynthPart::SUBCHANNEL_P | SynthPart::SUBCHANNEL_Q | SynthPart::SUBCODE_DEINTERLEAVE,
            destBuffer2448: &mut buf,
            destOffset: 0,
            disc: &disc,
        };
        gap.synth(&mut job);
        assert_eq!(buf[0], 0x00);
        assert!(buf[1..11].iter().all(|&b| b == 0xFF));
        assert_eq!((buf[12], buf[13], buf[14], buf[15]), (0x00, 0x02, 0x00, 1));
        let edc = ecm::edc_calc(&buf[..2064]);
        assert_eq!(&buf[2064..2068], &edc.to_le_bytes());
        assert!(buf[2068..2076].iter().all(|&b| b == 0));
        assert!(buf[2076..2352].iter().any(|&b| b != 0), "ECC parity of a non-zero header");
        assert!(buf[2352..2364].iter().all(|&b| b == 0xFF));
        assert_eq!(buf[2364], 0x41);
        assert_eq!(buf[2365], 0x01);
    }

    #[test]
    fn mode1_2048_reads_the_blob_and_ecm_forces_the_user_and_header_parts() {
        use crate::vfx::VirtualFileSystem;
        use std::collections::HashMap;
        let mut files = HashMap::new();
        files.insert("a.bin".into(), vec![1, 2, 3, 4]);
        let vfx = VirtualFileSystem::new(files);
        let blob = Rc::new(RefCell::new(Blob_Raw::open(&vfx, "a.bin").unwrap()));
        let disc = Disc::default();
        let mut buf = vec![0xEEu8; 2448];
        let mut ss = SS_Mode1_2048 { base: SS_Base { blob: Some(blob), blobOffset: 1, ..SS_Base::default() } };
        let mut job = SectorSynthJob {
            lba: 0,
            parts: SynthPart::ECM_ANY,
            destBuffer2448: &mut buf,
            destOffset: 0,
            disc: &disc,
        };
        ss.synth(&mut job);
        assert!(job.parts.contains(SynthPart::USER2048));
        assert!(job.parts.contains(SynthPart::HEADER16));
        assert_eq!(&buf[16..19], &[2, 3, 4], "read starts at blob offset 1 and does not clear the tail");
        assert_eq!(buf[15], 1);
    }
}
