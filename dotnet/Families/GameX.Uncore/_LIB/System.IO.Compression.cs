using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Bzip2;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static System.IO.Compression.CompressionX;
using static System.IO.Compression.ZipArchiveEncrypt;
using static System.IO.Compression.ZipEndOfCentralDirectoryBlock;

namespace System.IO.Compression;

#region ZipArchiveX

public enum ZipArchiveKind { Cry3, P4k }

public partial class ZipArchiveX : ZipArchive {
    static readonly byte[] DEFAULT_CUSTOMIV = { 0x70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    EHeaderEncryptionType _encryptedHeaders;
    EHeaderSignatureType _signedHeaders;
    CrySignedCDRHeader _headerSignature;
    CryCustomExtendedHeader _headerExtended;
    CryCustomEncryptionHeader _headerEncryption;
    CryCustomTeaEncryptionHeader _headerTeaEncryption;
    byte[] _customIV;
    byte[][] _customKeys;
    long _offsetOfFirstEntry = 0L;

    /// <summary>
    /// Size of end of central record (excluding variable fields)
    /// </summary>
    const int EndOfCentralRecordBaseSize = 22;

    internal readonly ZipArchiveKind _kind;
    readonly string _name;
    readonly byte[] _key;
    public ZipArchiveX(ZipArchiveKind kind, Stream stream, string name, byte[] key) : this(kind, stream, name, key, ZipArchiveMode.Read, leaveOpen: false, entryNameEncoding: null) { }
    public ZipArchiveX(ZipArchiveKind kind, Stream stream, string name, byte[] key, ZipArchiveMode mode) : this(kind, stream, name, key, mode, leaveOpen: false, entryNameEncoding: null) { }
    public ZipArchiveX(ZipArchiveKind kind, Stream stream, string name, byte[] key, ZipArchiveMode mode, bool leaveOpen) : this(kind, stream, name, key, mode, leaveOpen, entryNameEncoding: null) { }
    public ZipArchiveX(ZipArchiveKind kind, Stream stream, string name, byte[] key, ZipArchiveMode mode, bool leaveOpen, Encoding entryNameEncoding) : base(new MemoryStream(), ZipArchiveMode.Create, leaveOpen, entryNameEncoding) {
        _readEntries = false;
        _kind = kind;
        _archiveStream = DecideArchiveStream(mode, stream);
        _name = name;
        _key = key;
        _mode = mode;
        //ArgumentNullException.ThrowIfNull(stream);
        Stream extraTempStream = null;
        try {
            _backingStream = null;
            if (ValidateMode(mode, stream)) {
                _backingStream = stream;
                extraTempStream = stream = new MemoryStream();
                _backingStream.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            _archiveStream = DecideArchiveStream(mode, stream);
            switch (mode) {
                case ZipArchiveMode.Create:
                    _readEntries = true;
                    break;
                case ZipArchiveMode.Read:
                    ReadEndOfCentralDirectory();
                    break;
                case ZipArchiveMode.Update:
                default:
                    Debug.Assert(mode == ZipArchiveMode.Update);
                    if (_archiveStream.Length == 0)
                        _readEntries = true;
                    else {
                        ReadEndOfCentralDirectory();
                        EnsureCentralDirectoryRead();
                        foreach (var entry in _entries.Select(s => new ZipArchiveEntryX(s)))
                            entry.ThrowIfNotOpenable(needToUncompress: false, needToLoadIntoMemory: true);
                    }
                    break;
            }
            //_mode = ZipArchiveMode.Update;
        }
        catch (Exception) { extraTempStream?.Dispose(); throw; }
    }

    public static uint GetReferenceCRCForPak() => 0;

    void ReadEndOfCentralDirectory() {
        try {
            // This seeks backwards almost to the beginning of the EOCD, one byte after where the signature would be
            // located if the EOCD had the minimum possible size (no file zip comment)
            _archiveStream.Seek(-ZipEndOfCentralDirectoryBlock.SizeOfBlockWithoutSignature, SeekOrigin.End);
            // If the EOCD has the minimum possible size (no zip file comment), then exactly the previous 4 bytes will contain the signature
            // But if the EOCD has max possible size, the signature should be found somewhere in the previous 64K + 4 bytes
            if (!ZipHelper.SeekBackwardsToSignature(_archiveStream, ZipEndOfCentralDirectoryBlock.SignatureConstantBytes, ZipEndOfCentralDirectoryBlock.ZipFileCommentMaxLength + ZipEndOfCentralDirectoryBlock.FieldLengths.Signature)) throw new InvalidDataException(SR.EOCDNotFound);
            var eocdStart = _archiveStream.Position;
            // read the EOCD
            var eocd = ZipEndOfCentralDirectoryBlock.ReadBlock(_archiveStream); var eocd2 = new ZipEndOfCentralDirectoryBlock(eocd);
            if (_kind == ZipArchiveKind.Cry3) DecodeHeader(eocd2);
            ReadEndOfCentralDirectoryInnerWork(eocd);
            TryReadZip64EndOfCentralDirectory(eocd, eocdStart);
            if (_centralDirectoryStart > _archiveStream.Length) throw new InvalidDataException(SR.FieldTooBigOffsetToCD);
            if (_kind == ZipArchiveKind.Cry3) {
                TrySfxEmbedded(eocd2, eocdStart);
                DecodeHeaderData(eocd2);
            }
        }
        catch (EndOfStreamException ex) { throw new InvalidDataException(SR.CDCorrupt, ex); }
        catch (IOException ex) { throw new InvalidDataException(SR.CDCorrupt, ex); }
    }

    void DecodeHeader(ZipEndOfCentralDirectoryBlock s) {
        // Earlier pak file encryption techniques stored the encryption type in the disk number of the CDREnd.
        // This works, but can't be used by the more recent techniques that require signed paks to be readable by 7-Zip during dev.
        var headerEnc = (EHeaderEncryptionType)(s.NumberOfThisDisk >> 14);
        if (headerEnc == EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA || headerEnc == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER) _encryptedHeaders = headerEnc;
        s.NumberOfThisDisk &= 0x3fff;

        // Pak may be encrypted with CryCustom technique and/or signed. Being signed is compatible (in principle) with the earlier encryption methods.
        // The information for this exists in some custom headers at the end of the archive (in the comment section)
        var commentSize = s._archiveComment.Length;
        if (commentSize >= CryCustomExtendedHeader.SizeOf) {
            _archiveStream.Seek(s.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber + (long)s.SizeOfCentralDirectory + EndOfCentralRecordBaseSize, SeekOrigin.Begin);
            var r = new BinaryReader(_archiveStream);
            _headerExtended = new CryCustomExtendedHeader {
                nHeaderSize = r.ReadUInt32(),
                nEncryption = r.ReadUInt16(),
                nSigning = r.ReadUInt16(),
            };
            if (_headerExtended.nHeaderSize != CryCustomExtendedHeader.SizeOf) throw new InvalidDataException("Bad extended header");

            // We have the header, so read the encryption and signing techniques
            _signedHeaders = (EHeaderSignatureType)_headerExtended.nSigning;

            // Prepare for a quick sanity check on the size of the comment field now that we know what it should contain
            // Also check that the techniques are supported
            var expectedCommentLength = CryCustomExtendedHeader.SizeOf;

            // Encryption technique has been specified in both the disk number (old technique) and the custom header (new technique).
            if (_encryptedHeaders != EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA && _headerExtended.nEncryption != (ushort)EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED && _encryptedHeaders != 0) throw new InvalidDataException("Unexpected encryption technique in header");
            else {
                // The encryption technique has been specified only in the custom header
                _encryptedHeaders = (EHeaderEncryptionType)_headerExtended.nEncryption;
                switch (_encryptedHeaders) {
                    case EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED: break;
                    case EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA or EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE or EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                        if (_encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA) expectedCommentLength += CryCustomTeaEncryptionHeader.SizeOf;
                        var hasSize2 = _encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2;
                        expectedCommentLength += CryCustomEncryptionHeader.SizeOf;
                        if (hasSize2) expectedCommentLength += CryCustomEncryptionHeader.SizeOf2;
                        break;
                    default: throw new InvalidDataException("Unexpected encryption technique in header");
                }
            }

            // Add the signature header to the expected size
            switch (_signedHeaders) {
                case EHeaderSignatureType.HEADERS_NOT_SIGNED: break;
                case EHeaderSignatureType.HEADERS_CDR_SIGNED or EHeaderSignatureType.HEADERS_CDR_SIGNED2: expectedCommentLength += CrySignedCDRHeader.SizeOf; break;
                default: throw new InvalidDataException("Bad signing technique in header");
            }

            if (commentSize == expectedCommentLength) {
                if (_signedHeaders == EHeaderSignatureType.HEADERS_CDR_SIGNED || _signedHeaders == EHeaderSignatureType.HEADERS_CDR_SIGNED2) {
                    _headerSignature = new CrySignedCDRHeader {
                        nHeaderSize = r.ReadUInt32(),
                        CDR_signed = r.ReadBytes(RSA_KEY_MESSAGE_LENGTH)
                    };
                    if (_headerSignature.nHeaderSize != CrySignedCDRHeader.SizeOf) throw new InvalidDataException("Bad signature header");
                }
                if (_encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA) {
                    _headerTeaEncryption = new CryCustomTeaEncryptionHeader {
                        nHeaderSize = r.ReadUInt32(),
                        Unknown1 = r.ReadBytes(172),
                    };
                    if (_headerTeaEncryption.nHeaderSize != CryCustomTeaEncryptionHeader.SizeOf + CryCustomEncryptionHeader.SizeOf) throw new InvalidDataException("Bad encryption header");
                    _encryptedHeaders = EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE;
                }
                if (_encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE || _encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2) {
                    var hasSize2 = _encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2;
                    _headerEncryption = new CryCustomEncryptionHeader {
                        nHeaderSize = r.ReadUInt32(),
                        Unknown1 = hasSize2 ? r.ReadUInt32() : 0,
                        Unknown2 = hasSize2 ? r.ReadUInt32() : 0,
                        CDR_IV = r.ReadBytes(RSA_KEY_MESSAGE_LENGTH),
                        Keys_Table = r.ReadBytes(BLOCK_CIPHER_NUM_KEYS * RSA_KEY_MESSAGE_LENGTH),
                    };
                    if (_headerEncryption.nHeaderSize != 0 && _headerEncryption.nHeaderSize != CryCustomEncryptionHeader.SizeOf + (hasSize2 ? CryCustomEncryptionHeader.SizeOf2 : 0)) throw new InvalidDataException("Bad encryption header");
                    // We have a table of symmetric keys to decrypt
                    var digestSize = _headerTeaEncryption.nHeaderSize != 0 ? 1 : 256;
                    DecryptKeysTable(_key, _headerEncryption.CDR_IV, _headerEncryption.Keys_Table, digestSize, out _customIV, out _customKeys);
                }
            }
            else throw new InvalidDataException("Comment field is the wrong length");
        }

        // HACK: Hardcoded check for PAK location before enforcing encryption requirement. For C2 Mod SDK Release.
        if (_encryptedHeaders == EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED) {
            if (GetReferenceCRCForPak() != 0) _encryptedHeaders = EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER;
        }
    }

    void TrySfxEmbedded(ZipEndOfCentralDirectoryBlock s, long eocdStart) {
        // SFX/embedded support, find the offset of the first entry vis the start of the stream
        // This applies to Zip files that are appended to the end of an SFX stub.
        // Or are appended as a resource to an executable.
        // Zip files created by some archivers have the offsets altered to reflect the true offsets
        // and so dont require any adjustment here...
        // TODO: Difficulty with Zip64 and SFX offset handling needs resolution - maths?
        var isZip64 = false;
        if (!isZip64 && (s.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber < eocdStart - (4 + (long)s.SizeOfCentralDirectory))) {
            _offsetOfFirstEntry = eocdStart - (4 + (long)s.SizeOfCentralDirectory + s.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber);
            if (_offsetOfFirstEntry <= 0) throw new InvalidDataException("Invalid embedded zip archive");
        }
    }

    bool DecodeHeaderData(ZipEndOfCentralDirectoryBlock s) {
        var nSize = (int)s.SizeOfCentralDirectory;
        _archiveStream.Seek(_offsetOfFirstEntry + s.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber, SeekOrigin.Begin);
        if (_encryptedHeaders != EHeaderEncryptionType.HEADERS_NOT_ENCRYPTED) {
            var bytes = new byte[nSize]; _archiveStream.ReadAtLeast(bytes, nSize);
            switch (_encryptedHeaders) {
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_TEA: XXTeaDecrypt(ref bytes, nSize); break;
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER: StreamCipher(ref bytes, nSize, GetReferenceCRCForPak()); break;
                case EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE or EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2:
                    var engineId = _encryptedHeaders == EHeaderEncryptionType.HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2 ? 'A' : '2';
                    var iv = _headerTeaEncryption.nHeaderSize != 0 ? DEFAULT_CUSTOMIV : _customIV;
                    if (!DecryptBufferWithStreamCipher(engineId, ref bytes, nSize, _customKeys[0], iv)) { Console.WriteLine("Failed to decrypt pak header"); return false; }
                    break;
                default: Console.WriteLine("Attempting to load encrypted pak by unsupported method"); return false;
            }
            _archiveStream = new MemoryStream(bytes);
            _centralDirectoryStart = 0;
        }
        var stream = _archiveStream;
        switch (_signedHeaders) {
            case EHeaderSignatureType.HEADERS_CDR_SIGNED or EHeaderSignatureType.HEADERS_CDR_SIGNED2:
                if (_name == null) break;
                // Verify CDR signature & pak name
                var pathSepIdx = Math.Max(_name.LastIndexOf('\\'), _name.LastIndexOf('/'));
                var pathSep = _name[(pathSepIdx + 1)..];
                var position = stream.Position; var bytes = new byte[nSize]; stream.ReadAtLeast(bytes, nSize); stream.Position = position;
                var dataToVerify = new byte[][] { bytes, Encoding.ASCII.GetBytes(pathSep) };
                var sizesToVerify = new int[] { nSize, pathSep.Length };
                // Could not verify signature
                if (!RsaVerifyData(dataToVerify, sizesToVerify, 2, _headerSignature.CDR_signed, 128, _key)) { Console.WriteLine("Failed to verify RSA signature of pak header"); return false; }
                break;
            case EHeaderSignatureType.HEADERS_NOT_SIGNED: break;
        }
        return true;
    }
}

internal class CompressionX {
    public static readonly Type SRType = typeof(ZipArchive).Assembly.GetType("System.SR");
    public static readonly Type ZipEndOfCentralDirectoryBlockType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.ZipEndOfCentralDirectoryBlock");
    public static readonly Type ZipHelperType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.ZipHelper");
    public static readonly Type SubReadStreamType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.SubReadStream");
    public static readonly Type CrcValidatingReadStreamType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.CrcValidatingReadStream");
    public static readonly Type DeflateStreamType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.DeflateStream");
    public static readonly Type DeflateManagedStreamType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.DeflateManagedStream");
    public static readonly Type ZipLocalFileHeaderType = typeof(ZipArchive).Assembly.GetType("System.IO.Compression.ZipLocalFileHeader");
}

public static class ZipArchiveX2 {
    public static Stream OpenX(this ZipArchiveEntry source) => new ZipArchiveEntryX(source).Open();
}

public partial class ZipArchiveX {
    internal static readonly FieldInfo _archiveStreamField = typeof(ZipArchive).GetField("_archiveStream", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _backingStreamField = typeof(ZipArchive).GetField("_backingStream", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _modeField = typeof(ZipArchive).GetField("_mode", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _entriesField = typeof(ZipArchive).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _readEntriesField = typeof(ZipArchive).GetField("_readEntries", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _centralDirectoryStartField = typeof(ZipArchive).GetField("_centralDirectoryStart", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly PropertyInfo ArchiveStreamProperty = typeof(ZipArchive).GetProperty("ArchiveStream", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo DecideArchiveStreamMethod = typeof(ZipArchive).GetMethod("DecideArchiveStream", BindingFlags.NonPublic | BindingFlags.Static); public delegate Stream DecideArchiveStreamDelegate(ZipArchiveMode mode, Stream stream);
    internal static readonly MethodInfo ValidateModeMethod = typeof(ZipArchive).GetMethod("ValidateMode", BindingFlags.NonPublic | BindingFlags.Static); public delegate bool ValidateModeDelegate(ZipArchiveMode mode, Stream stream);
    internal static readonly MethodInfo ReadEndOfCentralDirectoryInnerWorkMethod = typeof(ZipArchive).GetMethod("ReadEndOfCentralDirectoryInnerWork", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo TryReadZip64EndOfCentralDirectoryMethod = typeof(ZipArchive).GetMethod("TryReadZip64EndOfCentralDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo EnsureCentralDirectoryReadMethod = typeof(ZipArchive).GetMethod("EnsureCentralDirectoryRead", BindingFlags.NonPublic | BindingFlags.Instance);

    Stream _archiveStream {
        get => (Stream)_archiveStreamField.GetValue(this);
        set => _archiveStreamField.SetValue(this, value);
    }

    Stream _backingStream {
        get => (Stream)_backingStreamField.GetValue(this);
        set => _backingStreamField.SetValue(this, value);
    }

    ZipArchiveMode _mode {
        get => (ZipArchiveMode)_modeField.GetValue(this);
        set => _modeField.SetValue(this, value);
    }

    bool _readEntries {
        get => (bool)_readEntriesField.GetValue(this);
        set => _readEntriesField.SetValue(this, value);
    }

    List<ZipArchiveEntry> _entries => (List<ZipArchiveEntry>)_entriesField.GetValue(this);

    long _centralDirectoryStart {
        get => (long)_centralDirectoryStartField.GetValue(this);
        set => _centralDirectoryStartField.SetValue(this, value);
    }

    public Stream ArchiveStream => (Stream)ArchiveStreamProperty.GetValue(this);

    public static readonly DecideArchiveStreamDelegate DecideArchiveStream = DecideArchiveStreamMethod.CreateDelegate<DecideArchiveStreamDelegate>();
    public static readonly ValidateModeDelegate ValidateMode = ValidateModeMethod.CreateDelegate<ValidateModeDelegate>();
    public void ReadEndOfCentralDirectoryInnerWork(object eocd) => ReadEndOfCentralDirectoryInnerWorkMethod.Invoke(this, [eocd]);
    public void TryReadZip64EndOfCentralDirectory(object eocd, long eocdStart) => TryReadZip64EndOfCentralDirectoryMethod.Invoke(this, [eocd, eocdStart]);
    public void EnsureCentralDirectoryRead() => EnsureCentralDirectoryReadMethod.Invoke(this, null);

    internal Stream CreateAndInitDecryptionStream(Stream baseStream, ZipArchiveEntry entry) => throw new NotImplementedException();

    internal Stream CreateAndInitAesDecryptionStream(Stream baseStream, ZipArchiveEntry entry) {
        using var aes = new AesManaged { Key = _key, IV = new byte[16], Mode = CipherMode.CBC, Padding = PaddingMode.None };
        var crypto = new CryptoStream(baseStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        var b = new MemoryStream();
        crypto.CopyTo(b);
        b.Seek(-1, SeekOrigin.End); // Trim NULL off end of stream
        while (b.Position > 1 && b.ReadByte() == 0) b.Seek(-2, SeekOrigin.Current);
        b.SetLength(b.Position);
        b.Seek(0, SeekOrigin.Begin);
        return b;
    }
}

public class ZipArchiveEntryX(ZipArchiveEntry source) {
    readonly ZipArchiveEntry this_ = source;
    ZipArchiveKind _kind = ((ZipArchiveX)source.Archive)._kind;

    public enum CompressionMethodValues : ushort {
        Stored = 0,
        Deflate = 8,
        Deflate64 = 9,
        BZip2 = 12, // 0x000C
        LZMA = 14, // 0x000E
        ZStd = 100,
    }

    internal static readonly FieldInfo _archiveField = typeof(ZipArchiveEntry).GetField("_archive", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _compressedSizeField = typeof(ZipArchiveEntry).GetField("_compressedSize", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _uncompressedSizeField = typeof(ZipArchiveEntry).GetField("_uncompressedSize", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _crc32Field = typeof(ZipArchiveEntry).GetField("_crc32", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _offsetOfLocalHeaderField = typeof(ZipArchiveEntry).GetField("_offsetOfLocalHeader", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly FieldInfo _storedOffsetOfCompressedDataField = typeof(ZipArchiveEntry).GetField("_storedOffsetOfCompressedData", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly PropertyInfo CompressionMethodProperty = typeof(ZipArchiveEntry).GetProperty("CompressionMethod", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo ThrowIfNotOpenableMethod = typeof(ZipArchiveEntry).GetMethod("ThrowIfNotOpenable", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo ThrowIfInvalidArchiveMethod = typeof(ZipArchiveEntry).GetMethod("ThrowIfInvalidArchive", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo GetOffsetOfCompressedDataMethod = typeof(ZipArchiveEntry).GetMethod("GetOffsetOfCompressedData", BindingFlags.NonPublic | BindingFlags.Instance);

    ZipArchiveX _archive => (ZipArchiveX)_archiveField.GetValue(this_);
    long _compressedSize => (long)_compressedSizeField.GetValue(this_);
    long _offsetOfLocalHeader => (long)_offsetOfLocalHeaderField.GetValue(this_);
    long? _storedOffsetOfCompressedData {
        get => (long?)_storedOffsetOfCompressedDataField.GetValue(this_);
        set => _storedOffsetOfCompressedDataField.SetValue(this_, value);
    }
    //long _uncompressedSize => (long)_uncompressedSizeField.GetValue(this_);
    //uint _crc32 => (uint)_crc32Field.GetValue(this_);

    public bool IsEncrypted => false; // Flags
    public bool IsAesCrypted => false; // ExtraData != null && ExtraData.Length >= 168 && ExtraData[168] > 0x00;

    internal void ThrowIfNotOpenable(bool needToUncompress, bool needToLoadIntoMemory) => ThrowIfNotOpenableMethod.Invoke(this_, [needToUncompress, needToLoadIntoMemory]);
    internal void ThrowIfInvalidArchive() => ThrowIfInvalidArchiveMethod.Invoke(this_, null);
    internal long GetOffsetOfCompressedData() {
        if (_kind == ZipArchiveKind.Cry3) return (long)GetOffsetOfCompressedDataMethod.Invoke(this_, null);
        if (_storedOffsetOfCompressedData == null) {
            _archive.ArchiveStream.Seek(_offsetOfLocalHeader, SeekOrigin.Begin);
            // by calling this, we are using local header _storedEntryNameBytes.Length and extraFieldLength
            // to find start of data, but still using central directory size information
            if (!ZipLocalFileHeader.TrySkipBlock(_kind, _archive.ArchiveStream)) throw new InvalidDataException(SR.LocalFileHeaderCorrupt);
            _storedOffsetOfCompressedData = _archive.ArchiveStream.Position;
        }
        return _storedOffsetOfCompressedData.Value;
    }

    //internal byte[] ExtraData => (CompressionMethodValues)CompressionMethodProperty.GetValue(this_);

    public CompressionMethodValues CompressionMethod => (CompressionMethodValues)CompressionMethodProperty.GetValue(this_);

    public Stream Open() {
        ThrowIfInvalidArchive();
        switch (_archive.Mode) {
            case ZipArchiveMode.Read:
                return OpenInReadMode(checkOpenable: true);
            case ZipArchiveMode.Create:
                return OpenInWriteMode();
            case ZipArchiveMode.Update:
            default:
                Debug.Assert(_archive.Mode == ZipArchiveMode.Update);
                return OpenInUpdateMode();
        }
    }

    //0xFD2FB528 LE
    static bool IsZstdStream(byte[] bytes, long length) => bytes.Length > 3 && bytes[0] == 0x28 && bytes[1] == 0xB5 && bytes[2] == 0x2F && bytes[3] == 0xFD;

    Stream GetDataDecompressor(Stream compressedStreamToRead) {
        Stream uncompressedStream;
        switch (CompressionMethod) {
            case CompressionMethodValues.Stored: uncompressedStream = compressedStreamToRead; break;
            case CompressionMethodValues.Deflate:
                uncompressedStream = (Stream)Activator.CreateInstance(DeflateStreamType, compressedStreamToRead, CompressionMode.Decompress); //_uncompressedSize
                break;
            case CompressionMethodValues.Deflate64:
                uncompressedStream = (Stream)Activator.CreateInstance(DeflateManagedStreamType, compressedStreamToRead, CompressionMethodValues.Deflate64); //_uncompressedSize
                break;
            case CompressionMethodValues.BZip2: uncompressedStream = new CBZip2InputStream(compressedStreamToRead); break; // new BZip2InputStream(compressedStreamToRead); break;
            case CompressionMethodValues.ZStd:
                var b = new byte[4];
                if (compressedStreamToRead.CanSeek && compressedStreamToRead.Read(b, 0, 4) > 0) {
                    if (IsZstdStream(b, compressedStreamToRead.Length)) {
                        compressedStreamToRead.Seek(-4, SeekOrigin.Current);
                        uncompressedStream = new ZstdNet.DecompressionStream(compressedStreamToRead);
                    }
                    else (uncompressedStream = compressedStreamToRead).Seek(-4, SeekOrigin.Current);
                }
                else uncompressedStream = new ZstdNet.DecompressionStream(compressedStreamToRead);
                break;
            default: throw new InvalidDataException($"Unsupported compression method {CompressionMethod}");
        }
        return uncompressedStream;
    }

    Stream OpenInReadModeGetDataCompressor(long offsetOfCompressedData) {
        Stream compressedStream = (Stream)Activator.CreateInstance(SubReadStreamType, _archive.ArchiveStream, offsetOfCompressedData, _compressedSize);
        if (IsEncrypted) compressedStream = _archive.CreateAndInitDecryptionStream(compressedStream, this_) ?? throw new InvalidDataException("Unable to decrypt this entry");
        if (_kind == ZipArchiveKind.P4k && IsAesCrypted) compressedStream = _archive.CreateAndInitAesDecryptionStream(compressedStream, this_) ?? throw new InvalidDataException("Unable to decrypt this entry");
        Stream decompressedStream = GetDataDecompressor(compressedStream);
        return decompressedStream;
        //return (Stream)Activator.CreateInstance(CrcValidatingReadStreamType, decompressedStream, _crc32, _uncompressedSize);
    }

    Stream OpenInReadMode(bool checkOpenable) {
        //if (checkOpenable)
        //    ThrowIfNotOpenable(needToUncompress: true, needToLoadIntoMemory: false);
        return OpenInReadModeGetDataCompressor(GetOffsetOfCompressedData());
    }
    Stream OpenInWriteMode() => throw new NotImplementedException();
    Stream OpenInUpdateMode(bool loadExistingContent = true) => throw new NotImplementedException();
}

internal struct ZipLocalFileHeader {
    internal static class FieldLengths {
        // Must match the signature constant bytes length, but should stay a const int or sometimes
        // static initialization of FieldLengths and NullReferenceException occurs.
        public const int Signature = 4;
        public const int VersionNeededToExtract = sizeof(ushort);
        public const int GeneralPurposeBitFlags = sizeof(ushort);
        public const int CompressionMethod = sizeof(ushort);
        public const int LastModified = sizeof(ushort) + sizeof(ushort);
        public const int Crc32 = sizeof(uint);
        public const int CompressedSize = sizeof(uint);
        public const int UncompressedSize = sizeof(uint);
        public const int FilenameLength = sizeof(ushort);
        public const int ExtraFieldLength = sizeof(ushort);
    }

    internal static class FieldLocations {
        public const int Signature = 0;
        public const int VersionNeededToExtract = Signature + FieldLengths.Signature;
        public const int GeneralPurposeBitFlags = VersionNeededToExtract + FieldLengths.VersionNeededToExtract;
        public const int CompressionMethod = GeneralPurposeBitFlags + FieldLengths.GeneralPurposeBitFlags;
        public const int LastModified = CompressionMethod + FieldLengths.CompressionMethod;
        public const int Crc32 = LastModified + FieldLengths.LastModified;
        public const int CompressedSize = Crc32 + FieldLengths.Crc32;
        public const int UncompressedSize = CompressedSize + FieldLengths.CompressedSize;
        public const int FilenameLength = UncompressedSize + FieldLengths.UncompressedSize;
        public const int ExtraFieldLength = FilenameLength + FieldLengths.FilenameLength;
        public const int DynamicData = ExtraFieldLength + FieldLengths.ExtraFieldLength;
    }

    internal static readonly MethodInfo TrySkipBlockFinalizeMethod = ZipLocalFileHeaderType.GetMethod("TrySkipBlockFinalize", BindingFlags.NonPublic | BindingFlags.Static); public delegate bool TrySkipBlockFinalizeDelegate(Stream stream, Span<byte> blockBytes, int bytesRead);

    public static readonly TrySkipBlockFinalizeDelegate TrySkipBlockFinalize = TrySkipBlockFinalizeMethod.CreateDelegate<TrySkipBlockFinalizeDelegate>();

    static bool TrySkipBlockCore(ZipArchiveKind kind, Stream stream, Span<byte> blockBytes, int bytesRead, long currPosition) {
        if (kind == ZipArchiveKind.P4k) { if (bytesRead != FieldLengths.Signature || !(blockBytes.SequenceEqual(P4kSignatureConstantBytes) || blockBytes.SequenceEqual(P4kSignatureConstantEncryptedBytes))) return false; }
        else { if (bytesRead != FieldLengths.Signature || !blockBytes.SequenceEqual(SignatureConstantBytes)) return false; }
        if (stream.Length < currPosition + FieldLocations.FilenameLength) return false;
        // Already read the signature, so make the filename length field location relative to that
        stream.Seek(FieldLocations.FilenameLength - FieldLengths.Signature, SeekOrigin.Current);
        // Reuse blockBytes to read the filename length and the extra field length - these two consecutive fields fit inside blockBytes.
        Debug.Assert(blockBytes.Length == FieldLengths.FilenameLength + FieldLengths.ExtraFieldLength);
        return true;
    }

    internal static bool TrySkipBlock(ZipArchiveKind kind, Stream stream) {
        Span<byte> blockBytes = stackalloc byte[FieldLengths.Signature];
        var currPosition = stream.Position;
        var bytesRead = stream.ReadAtLeast(blockBytes, blockBytes.Length, throwOnEndOfStream: false);
        if (!TrySkipBlockCore(kind, stream, blockBytes, bytesRead, currPosition)) return false;
        bytesRead = stream.ReadAtLeast(blockBytes, blockBytes.Length, throwOnEndOfStream: false);
        return TrySkipBlockFinalize(stream, blockBytes, bytesRead);
    }
}

internal class ZipEndOfCentralDirectoryBlock(object source) {
    internal static class FieldLengths {
        // Must match the signature constant bytes length, but should stay a const int or sometimes
        // static initialization of FieldLengths and NullReferenceException occurs.
        public const int Signature = 4;
        public const int NumberOfThisDisk = sizeof(ushort);
        public const int NumberOfTheDiskWithTheStartOfTheCentralDirectory = sizeof(ushort);
        public const int NumberOfEntriesInTheCentralDirectoryOnThisDisk = sizeof(ushort);
        public const int NumberOfEntriesInTheCentralDirectory = sizeof(ushort);
        public const int SizeOfCentralDirectory = sizeof(uint);
        public const int OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = sizeof(uint);
        public const int ArchiveCommentLength = sizeof(ushort);
    }
    internal static class FieldLocations {
        public const int Signature = 0;
        public const int NumberOfThisDisk = Signature + FieldLengths.Signature;
        public const int NumberOfTheDiskWithTheStartOfTheCentralDirectory = NumberOfThisDisk + FieldLengths.NumberOfThisDisk;
        public const int NumberOfEntriesInTheCentralDirectoryOnThisDisk = NumberOfTheDiskWithTheStartOfTheCentralDirectory + FieldLengths.NumberOfTheDiskWithTheStartOfTheCentralDirectory;
        public const int NumberOfEntriesInTheCentralDirectory = NumberOfEntriesInTheCentralDirectoryOnThisDisk + FieldLengths.NumberOfEntriesInTheCentralDirectoryOnThisDisk;
        public const int SizeOfCentralDirectory = NumberOfEntriesInTheCentralDirectory + FieldLengths.NumberOfEntriesInTheCentralDirectory;
        public const int OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = SizeOfCentralDirectory + FieldLengths.SizeOfCentralDirectory;
        public const int ArchiveCommentLength = OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber + FieldLengths.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
        public const int DynamicData = ArchiveCommentLength + FieldLengths.ArchiveCommentLength;
    }

    // The Zip File Format Specification references 0x06054B50, this is a big endian representation.
    // ZIP files store values in little endian, so this is reversed.
    public static readonly byte[] SignatureConstantBytes = [0x50, 0x4B, 0x05, 0x06];
    public static readonly byte[] P4kSignatureConstantBytes = [0x50, 0x4B, 0x03, 0x4];
    public static readonly byte[] P4kSignatureConstantEncryptedBytes = [0x50, 0x4B, 0x03, 0x14];


    // This also assumes a zero-length comment.
    public const int TotalSize = FieldLocations.ArchiveCommentLength + FieldLengths.ArchiveCommentLength;
    // These are the minimum possible size, assuming the zip file comments variable section is empty
    public const int SizeOfBlockWithoutSignature = TotalSize - FieldLengths.Signature;

    // The end of central directory can have a variable size zip file comment at the end, but its max length can be 64K
    // The Zip File Format Specification does not explicitly mention a max size for this field, but we are assuming this
    // max size because that is the maximum value an ushort can hold.
    public const int ZipFileCommentMaxLength = ushort.MaxValue;

    readonly object this_ = source;
    internal static readonly FieldInfo NumberOfThisDiskField = ZipEndOfCentralDirectoryBlockType.GetField("NumberOfThisDisk", BindingFlags.Public | BindingFlags.Instance);
    internal static readonly FieldInfo SizeOfCentralDirectoryField = ZipEndOfCentralDirectoryBlockType.GetField("SizeOfCentralDirectory", BindingFlags.Public | BindingFlags.Instance);
    internal static readonly FieldInfo OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumberField = ZipEndOfCentralDirectoryBlockType.GetField("OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber", BindingFlags.Public | BindingFlags.Instance);
    internal static readonly FieldInfo _archiveCommentField = ZipEndOfCentralDirectoryBlockType.GetField("_archiveComment", BindingFlags.NonPublic | BindingFlags.Instance);
    internal static readonly MethodInfo ReadBlockMethod = ZipEndOfCentralDirectoryBlockType.GetMethod("ReadBlock", BindingFlags.Public | BindingFlags.Static); public delegate object ReadBlockDelegate(Stream stream);

    public ushort NumberOfThisDisk {
        get => (ushort)NumberOfThisDiskField.GetValue(this_);
        set => NumberOfThisDiskField.SetValue(this_, value);
    }

    public uint SizeOfCentralDirectory {
        get => (uint)SizeOfCentralDirectoryField.GetValue(this_);
        set => SizeOfCentralDirectoryField.SetValue(this_, value);
    }

    public uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber {
        get => (uint)OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumberField.GetValue(this_);
        set => OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumberField.SetValue(this_, value);
    }

    internal byte[] _archiveComment {
        get => (byte[])_archiveCommentField.GetValue(this_);
        set => _archiveCommentField.SetValue(this_, value);
    }

    public static readonly ReadBlockDelegate ReadBlock = ReadBlockMethod.CreateDelegate<ReadBlockDelegate>();
}

internal class ZipHelper {
    internal static readonly MethodInfo SeekBackwardsToSignatureMethod = ZipHelperType.GetMethod("SeekBackwardsToSignature", BindingFlags.NonPublic | BindingFlags.Static); internal delegate bool SeekBackwardsToSignatureDelegate(Stream stream, ReadOnlySpan<byte> signatureToFind, int maxBytesToRead);
    internal static readonly SeekBackwardsToSignatureDelegate SeekBackwardsToSignature = SeekBackwardsToSignatureMethod.CreateDelegate<SeekBackwardsToSignatureDelegate>();
}

internal static class SR {
    public static string CDCorrupt = (string)SRType.GetProperty("CDCorrupt", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    public static string EOCDNotFound = (string)SRType.GetProperty("EOCDNotFound", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    public static string FieldTooBigOffsetToCD = (string)SRType.GetProperty("FieldTooBigOffsetToCD", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    public static string LocalFileHeaderCorrupt = (string)SRType.GetProperty("LocalFileHeaderCorrupt", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
}

#endregion

#region Encrypt

/// <summary>
/// Implements the Segmented Integer Counter (SIC) mode on top of a simple block cipher.
/// </summary>
internal class SicRevBlockCipher : IBlockCipherMode {
    readonly IBlockCipher cipher;
    readonly int blockSize;
    readonly byte[] counter;
    readonly byte[] counterOut;
    byte[] IV;

    /// <summary>
    /// Basic constructor.
    /// </summary>
    /// <param name="cipher">the block cipher to be used.</param>
    public SicRevBlockCipher(IBlockCipher cipher) {
        this.cipher = cipher;
        blockSize = cipher.GetBlockSize();
        counter = new byte[blockSize];
        counterOut = new byte[blockSize];
        IV = new byte[blockSize];
    }

    /// <summary>
    /// return the underlying block cipher that we are wrapping.
    /// </summary>
    /// <returns>the underlying block cipher that we are wrapping.</returns>
    public IBlockCipher UnderlyingCipher => cipher;
    public void Init(bool forEncryption, ICipherParameters parameters) {
        ParametersWithIV ivParam = parameters as ParametersWithIV ?? throw new ArgumentException("CTR/SIC mode requires ParametersWithIV", nameof(parameters));
        IV = Arrays.Clone(ivParam.GetIV()); // Arrays.Fill(counter, 0); Array.Copy(IV, 0, counter, 0, IV.Length);
        if (blockSize < IV.Length) throw new ArgumentException($"CTR/SIC mode requires IV no greater than: {blockSize} bytes.");
        var maxCounterSize = Math.Min(8, blockSize / 2);
        if (blockSize - IV.Length > maxCounterSize) throw new ArgumentException($"CTR/SIC mode requires IV of at least: {blockSize - maxCounterSize} bytes.");
        if (ivParam.Parameters != null) cipher.Init(true, ivParam.Parameters); // if null it's an IV changed only.
        Reset();
    }
    public string AlgorithmName => cipher.AlgorithmName + "/SIC";
    public bool IsPartialBlockOkay => true;
    public int GetBlockSize() => blockSize;
    public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff) {
        cipher.ProcessBlock(counter, 0, counterOut, 0);
        for (var i = 0; i < counterOut.Length; i++) output[outOff + i] = (byte)(counterOut[i] ^ input[inOff + i]); // XOR the counterOut with the plaintext producing the cipher text
        var j = 0; while (j <= counter.Length && ++counter[j++] == 0) { } // Increment the counter
        return counter.Length;
    }
    public int ProcessBlock(ReadOnlySpan<byte> input, Span<byte> output) {
        cipher.ProcessBlock(counter, 0, counterOut, 0);
        for (var i = 0; i < counterOut.Length; i++) output[i] = (byte)(counterOut[i] ^ input[i]); // XOR the counterOut with the plaintext producing the cipher text
        var j = 0; while (j <= counter.Length && ++counter[j++] == 0) { } // Increment the counter
        return counter.Length;
    }
    public void Reset() {
        Arrays.Fill(counter, 0);
        Array.Copy(IV, 0, counter, 0, IV.Length);
        //cipher.Reset();
    }
}

/// <summary>
/// ZipArchiveEncrypt
/// </summary>
internal unsafe static class ZipArchiveEncrypt {
    public const int BLOCK_CIPHER_NUM_KEYS = 16;
    public const int BLOCK_CIPHER_KEY_LENGTH = 16;
    public const int RSA_KEY_MESSAGE_LENGTH = 128;         // The modulus of our private/public key pair for signing, verification, encryption and decryption

    public const int METHOD_DEFLATE_AND_ENCRYPT = 11; // Deflate + Custom encryption (TEA)
    public const int METHOD_DEFLATE_AND_STREAMCIPHER = 12; // Deflate + stream cipher encryption on a per file basis
    public const int METHOD_STORE_AND_STREAMCIPHER_KEYTABLE = 13; // Store + Timur's encryption technique on a per file basis
    public const int METHOD_DEFLATE_AND_STREAMCIPHER_KEYTABLE = 14; // Deflate + Timur's encryption technique on a per file basis

    // encryption settings for zip header - stored in m_headerExtended struct
    public enum EHeaderEncryptionType {
        HEADERS_NOT_ENCRYPTED = 0,                  // (None)
        HEADERS_ENCRYPTED_STREAMCIPHER = 1,         // (StreamCipher)
        HEADERS_ENCRYPTED_TEA = 2,                  // (XXTEA) TEA = Tiny Encryption Algorithm
        HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE = 3, // (Twofish) Timur's technique. Encrypt each file and the CDR with one of 16 stream cipher keys. Encrypt the table of keys with an RSA key.
        HEADERS_ENCRYPTED_STREAMCIPHER_KEYTABLE2 = 4, // (Hunt) Hunt encryption
    }

    // Signature settings for zip header
    public enum EHeaderSignatureType {
        HEADERS_NOT_SIGNED = 0,
        HEADERS_CDR_SIGNED = 1, // Includes an RSA signature based on the hash of the archive's CDR. Verified in a console compatible way.
        HEADERS_CDR_SIGNED2 = 2, //
    }

    // Stores type of encryption and signing
    public struct CryCustomExtendedHeader {
        public const ushort SizeOf = 8;
        public uint nHeaderSize;                    // Size of the extended header.
        public ushort nEncryption;                  // Matches one of EHeaderEncryptionType: 0 = No encryption/extension
        public ushort nSigning;                     // Matches one of EHeaderSignatureType: 0 = No signing
    }

    // Header for HEADERS_SIGNED_CDR technique implemented on consoles. The comment section needs to contain the following in order:
    // CryCustomExtendedHeader, CrySignedCDRHeader
    public struct CrySignedCDRHeader {
        public const ushort SizeOf = 4 + RSA_KEY_MESSAGE_LENGTH;
        public uint nHeaderSize; // Size of the extended header.
        public byte[] CDR_signed; //[RSA_KEY_MESSAGE_LENGTH]
    }

    public struct CryCustomTeaEncryptionHeader {
        public const ushort SizeOf = 4 + 172;
        public uint nHeaderSize; // Size of the extended header.
        public byte[] Unknown1; //[172]
    }

    // Header for HEADERS_ENCRYPTED_CRYCUSTOM technique. Paired with a CrySignedCDRHeader to allow for signing as well as encryption.
    // i.e. the comment section for a file that uses this technique needs the following in order:
    // CryCustomExtendedHeader, CrySignedCDRHeader, CryCustomEncryptionHeader
    public struct CryCustomEncryptionHeader {
        public const ushort SizeOf = 2180; //4 + RSA_KEY_MESSAGE_LENGTH + (BLOCK_CIPHER_NUM_KEYS * RSA_KEY_MESSAGE_LENGTH);
        public const ushort SizeOf2 = 8;
        public uint nHeaderSize; // Size of the extended header.
        public uint Unknown1; // Hunt: Shadow
        public uint Unknown2; // Hunt: Shadow
        public byte[] CDR_IV; //[RSA_KEY_MESSAGE_LENGTH] Initial Vector is actually BLOCK_CIPHER_KEY_LENGTH bytes in length, but is encrypted as a RSA_KEY_MESSAGE_LENGTH byte message.
        public byte[] Keys_Table; //[BLOCK_CIPHER_NUM_KEYS * RSA_KEY_MESSAGE_LENGTH] As above, actually BLOCK_CIPHER_KEY_LENGTH but encrypted.
    }

    #region StreamCipher

    public static bool DecryptBufferWithStreamCipher(char engineId, ref byte[] data, int size, byte[] key, byte[] iv) {
        try {
            var cipher = new BufferedBlockCipher(new SicRevBlockCipher(engineId == 'A' ? new AesEngine() : new TwofishEngine()));
            cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
            data = cipher.DoFinal(data, 0, size);
        }
        catch (CryptoException ex) { Console.WriteLine(ex.Message); return false; }
        return true;
    }

    public static int GetEncryptionKeyIndex(ZipArchiveEntry entry) => (int)unchecked(~(entry.Crc32 >> 2) & 0xF);

    public static void GetEncryptionInitialVector(ZipArchiveEntry entry, out byte[] iv) {
        unchecked {
            var intIV = new[] {
                (uint)(entry.Length ^ (entry.CompressedLength << 12)),
                (uint)(entry.CompressedLength != 0 ? 0 : 1),
                (uint)(entry.Crc32 ^ (entry.CompressedLength << 12)),
                (uint)((entry.Length != 0 ? 0 : 1) ^ entry.CompressedLength)};
            iv = new byte[16];
            fixed (uint* ptr = intIV) Marshal.Copy((IntPtr)ptr, iv, 0, 16);
        }
    }

    public static bool RsaVerifyData(byte[][] data, int[] sizes, int numBuffers, byte[] signedHash, int signedHashSize, byte[] publicKey) => true;

    internal static void StreamCipher(ref byte[] data, int size, uint inKey = 0) {
        //    StreamCipherState cipher;
        //    gEnv->pSystem->GetCrypto()->GetStreamCipher()->Init(cipher, (const uint8*)&inKey, sizeof(inKey));
        //    gEnv->pSystem->GetCrypto()->GetStreamCipher()->EncryptStream(cipher, (uint8*)buffer, size, (uint8*)buffer);
    }

    #endregion

    #region RSA

    // cry
    static readonly byte[] RsaKey = [
        0x30, 0x81, 0x9F, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01,
        0x05, 0x00, 0x03, 0x81, 0x8D, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xA9, 0xD5, 0x90,
        0xA4, 0xBC, 0x92, 0xDB, 0x8C, 0xF1, 0xFC, 0x5A, 0xD5, 0x8F, 0x46, 0x05, 0x52, 0x16, 0xEE, 0xF3,
        0xC3, 0xBE, 0x86, 0xDE, 0x70, 0x1F, 0x4E, 0x2D, 0x18, 0xD3, 0x01, 0x92, 0x46, 0xBE, 0xFA, 0xAD,
        0x66, 0x04, 0x7B, 0x8C, 0xDD, 0x0D, 0x24, 0x8D, 0xA7, 0x23, 0xCA, 0x52, 0xC8, 0xE5, 0x01, 0xE0,
        0xB7, 0x2B, 0xEB, 0x55, 0xCF, 0x0D, 0xF7, 0x97, 0x77, 0xDC, 0x11, 0xE8, 0x7B, 0x18, 0xCC, 0xDB,
        0x90, 0x07, 0x2D, 0x9D, 0xC4, 0xAD, 0x80, 0x7C, 0x50, 0x23, 0x85, 0x46, 0xF3, 0xE9, 0x2C, 0x54,
        0x81, 0x11, 0x7B, 0x6D, 0xE2, 0x57, 0x87, 0x8E, 0x65, 0xE1, 0xD3, 0x16, 0xC4, 0x54, 0xED, 0x29,
        0xED, 0x51, 0xFD, 0xB1, 0xEF, 0xE4, 0x95, 0x01, 0x24, 0xAE, 0xC0, 0x6A, 0xFA, 0xE0, 0x5B, 0x19,
        0xD2, 0xE6, 0xF0, 0x22, 0x3B, 0xC3, 0xE7, 0xDD, 0x17, 0x1A, 0x8C, 0xF8, 0xE1, 0x02, 0x03, 0x01,
        0x00, 0x01 ];

    static AsymmetricKeyParameter GetPublicKey(byte[] keyInfoData) {
        if (new Asn1InputStream(keyInfoData).ReadObject() is not DerSequence sequence) throw new Exception("Invalid PrivateKey Data");
        AlgorithmIdentifier algId = null; DerBitString keyData = null;
        foreach (var value in sequence) {
            if (value is AlgorithmIdentifier || value is DerSequence) algId = AlgorithmIdentifier.GetInstance(value);
            else if (value is DerBitString) keyData = DerBitString.GetInstance(value);
            else if (value is DerInteger && keyData == null) keyData = new DerBitString(sequence);
        }
        if (keyData == null) throw new Exception("Invalid PrivateKey Data");
        return PublicKeyFactory.CreateKey(new SubjectPublicKeyInfo(algId ?? new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption), keyData.GetBytes()));
    }

    public static bool DecryptKeysTable(byte[] aesKey, byte[] CDR_IV, byte[] keys_table, int digestSize, out byte[] cdrIV, out byte[][] keysTable) {
        var digest = digestSize == 257 ? new Blake2bDigest()
            : digestSize == 256 ? (IDigest)new Sha256Digest()
            : new Sha1Digest();
        try {
            var publicKey = GetPublicKey(aesKey ?? RsaKey);
            var cipher = new OaepEncoding(new RsaEngine(), digest);

            // cdr iv
            cipher.Init(false, publicKey);
            cdrIV = cipher.ProcessBlock(CDR_IV, 0, RSA_KEY_MESSAGE_LENGTH);

            // Decrypt the table of cipher keys.
            keysTable = new byte[BLOCK_CIPHER_NUM_KEYS][];
            for (int i = 0, offset = 0; i < BLOCK_CIPHER_NUM_KEYS; i++, offset += RSA_KEY_MESSAGE_LENGTH) {
                cipher.Init(false, publicKey);
                keysTable[i] = cipher.ProcessBlock(keys_table, offset, RSA_KEY_MESSAGE_LENGTH);
            }
            return true;
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            cdrIV = default;
            keysTable = default;
            return false;
        }
    }

    #endregion

    #region TEA

    static readonly uint[] TEA_DEFAULTKEY = { 0xc968fb67, 0x8f9b4267, 0x85399e84, 0xf9b99dc4 };
    const uint TEA_DELTA = 0x9e3779b9;
    const uint TEA_DELTA2 = 0x61C88647;

    static void Btea(uint* v, int n, uint[] k) {
        uint y, z, sum;
        uint p, rounds, e;
        uint MX() => ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));

        if (n > 1) // Coding Part
        {
            rounds = (uint)(6 + 52 / n);
            sum = 0;
            z = v[n - 1];
            do {
                sum += TEA_DELTA;
                e = (sum >> 2) & 3;
                for (p = 0; p < (uint)(n - 1); p++) {
                    y = v[p + 1];
                    z = v[p] += MX();
                }
                y = v[0];
                z = v[n - 1] += MX();
            } while (--rounds != 0);
        }
        else if (n < -1) // Decoding Part
        {
            n = -n;
            rounds = (uint)(6 + 52 / n);
            sum = rounds * TEA_DELTA;
            y = v[0];
            do {
                e = (sum >> 2) & 3;
                for (p = (uint)(n - 1); p > 0; p--) {
                    z = v[p - 1];
                    y = v[p] -= MX();
                }
                z = v[n - 1];
                y = v[0] -= MX();
                sum -= TEA_DELTA;
            } while (--rounds != 0);
        }
    }

    static void SwapByteOrder(uint* values, int count) {
        for (uint* w = values, e = values + count; w != e; ++w) *w = (*w >> 24) | ((*w >> 8) & 0xff00) | ((*w & 0xff00) << 8) | (*w << 24);
    }

    internal static void XXTeaEncrypt(ref byte[] data, int size) {
        fixed (byte* dataPtr = data) {
            var intBuffer = (uint*)dataPtr;
            var encryptedLen = size >> 2;
            SwapByteOrder(intBuffer, encryptedLen);
            Btea(intBuffer, encryptedLen, TEA_DEFAULTKEY);
            SwapByteOrder(intBuffer, encryptedLen);
        }
    }

    internal static void XXTeaDecrypt(ref byte[] data, int size) {
        fixed (byte* dataPtr = data) {
            var intBuffer = (uint*)dataPtr;
            var encryptedLen = size >> 2;
            SwapByteOrder(intBuffer, encryptedLen);
            Btea(intBuffer, -encryptedLen, TEA_DEFAULTKEY);
            SwapByteOrder(intBuffer, encryptedLen);
        }
    }

    #endregion
}

#endregion
