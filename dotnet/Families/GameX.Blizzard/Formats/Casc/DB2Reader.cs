using GameX.FileSystems.Casc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace GameX.Blizzard.Formats.Casc;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ArraySizeAttribute : Attribute {
    public int Size { get; private set; }

    public ArraySizeAttribute(int size) {
        Size = size;
    }
}

public class FieldCache {
    public FieldInfo Field;
    public int ArraySize;
    public bool IsIndex;
    public bool IsArray;
}

public class FieldCache<T, V> : FieldCache {
    public readonly Action<T, V> Setter;
    public readonly Func<T, V> Getter;

    public FieldCache(FieldInfo field) {
        Field = field;
        IsArray = field.FieldType.IsArray;

        if (IsArray) {
            ArraySizeAttribute atr = (ArraySizeAttribute)field.GetCustomAttribute(typeof(ArraySizeAttribute));

            if (atr == null)
                throw new Exception(typeof(T).Name + "." + field.Name + " missing ArraySizeAttribute");

            ArraySize = atr.Size;
        }

        Setter = field.GetSetter<T, V>();
        Getter = field.GetGetter<T, V>();
    }
}

public abstract class ClientDBRow : IDB2Row {
    public abstract int GetId();

    public void Read<T>(FieldCache[] fields, T entry, BitReader r, int recordOffset, Dictionary<long, string> stringsTable, FieldMetaData[] fieldMeta, ColumnMetaData[] columnMeta, Value32[][] palletData, Dictionary<int, Value32>[] commonData, int id, int refId, bool isSparse = false) where T : ClientDBRow {
        int fieldIndex = 0;

        foreach (var f in fields) {
            if (f.IsIndex && id != -1) {
                ((FieldCache<T, int>)f).Setter(entry, id);
                continue;
            }

            if (fieldIndex >= fieldMeta.Length) {
                if (refId != -1)
                    ((FieldCache<T, int>)f).Setter(entry, refId);
                continue;
            }

            if (f.IsArray) {
                switch (f) {
                    case FieldCache<T, int[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<int>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, uint[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<uint>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, byte[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<byte>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, sbyte[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<sbyte>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, short[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<short>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, ushort[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<ushort>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, float[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<float>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, long[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<long>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, ulong[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueArray<ulong>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                        break;
                    case FieldCache<T, string[]> c1:
                        c1.Setter(entry, FieldReader.GetFieldValueStringsArray(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], stringsTable, isSparse, recordOffset, c1.ArraySize));
                        break;
                    default:
                        throw new Exception($"Unhandled DbcTable type: {f.Field.FieldType.FullName} in {f.Field.DeclaringType.FullName}.{f.Field.Name}");
                }
            }
            else {
                switch (f) {
                    case FieldCache<T, int> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<int>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, uint> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<uint>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, byte> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<byte>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, sbyte> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<sbyte>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, short> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<short>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, ushort> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<ushort>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, float> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<float>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, long> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<long>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, ulong> c1:
                        c1.Setter(entry, FieldReader.GetFieldValue<ulong>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                        break;
                    case FieldCache<T, string> c1:
                        c1.Setter(entry, isSparse ? r.ReadCString() : stringsTable[(recordOffset + (r.Position >> 3)) + FieldReader.GetFieldValue<int>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex])]);
                        break;
                    default:
                        throw new Exception($"Unhandled DbcTable type: {f.Field.FieldType.FullName} in {f.Field.DeclaringType.FullName}.{f.Field.Name}");
                }
            }

            fieldIndex++;
        }
    }

    public void SetId(int id) => throw new InvalidOperationException(nameof(SetId));
    public T GetField<T>(int fieldIndex, int arrayIndex = -1) => throw new InvalidOperationException(nameof(GetField));
    public IDB2Row Clone() => (IDB2Row)MemberwiseClone();
}

public interface IDB2Row {
    int GetId();
    void SetId(int id);
    T GetField<T>(int fieldIndex, int arrayIndex = -1);
    IDB2Row Clone();
}

public abstract class DB2Reader<T> : IEnumerable<KeyValuePair<int, T>> where T : IDB2Row {
    public int RecordsCount { get; protected set; }
    public int FieldsCount { get; protected set; }
    public int RecordSize { get; protected set; }
    public int StringTableSize { get; protected set; }
    public uint TableHash { get; protected set; }
    public uint LayoutHash { get; protected set; }
    public int MinIndex { get; protected set; }
    public int MaxIndex { get; protected set; }
    public int IdFieldIndex { get; protected set; }

    protected FieldMetaData[] m_meta;
    public FieldMetaData[] Meta => m_meta;

    protected ColumnMetaData[] m_columnMeta;
    public ColumnMetaData[] ColumnMeta => m_columnMeta;

    protected Value32[][] m_palletData;
    public Value32[][] PalletData => m_palletData;

    protected Dictionary<int, Value32>[] m_commonData;
    public Dictionary<int, Value32>[] CommonData => m_commonData;

    protected SortedDictionary<int, T> _Records = new SortedDictionary<int, T>();

    public bool HasRow(int id) => _Records.ContainsKey(id);

    public T GetRow(int id) { _Records.TryGetValue(id, out T row); return row; }

    public IEnumerator<KeyValuePair<int, T>> GetEnumerator() => _Records.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public struct FieldMetaData {
    public short Bits;
    public short Offset;
}

[StructLayout(LayoutKind.Explicit)]
public struct ColumnMetaData {
    [FieldOffset(0)] public ushort RecordOffset;
    [FieldOffset(2)] public ushort Size;
    [FieldOffset(4)] public uint AdditionalDataSize;
    [FieldOffset(8)] public CompressionType CompressionType;
    [FieldOffset(12)] public ColumnCompressionData_Immediate Immediate;
    [FieldOffset(12)] public ColumnCompressionData_Pallet Pallet;
    [FieldOffset(12)] public ColumnCompressionData_Common Common;
}

public struct ColumnCompressionData_Immediate {
    public int BitOffset;
    public int BitWidth;
    public int Flags; // 0x1 signed
}

public struct ColumnCompressionData_Pallet {
    public int BitOffset;
    public int BitWidth;
    public int Cardinality;
}

public struct ColumnCompressionData_Common {
    public Value32 DefaultValue;
    public int B;
    public int C;
}

public struct Value32 {
    uint Value;
    public T As<T>() where T : unmanaged => Unsafe.As<uint, T>(ref Value);
}

public enum CompressionType {
    None = 0,
    Immediate = 1,
    Common = 2,
    Pallet = 3,
    PalletArray = 4,
    SignedImmediate = 5
}

public struct ReferenceEntry {
    public int Id;
    public int Index;
}

public class ReferenceData {
    public int NumRecords { get; set; }
    public int MinId { get; set; }
    public int MaxId { get; set; }
    public Dictionary<int, int> Entries { get; set; }
}

[Flags]
public enum DB2Flags {
    None = 0x0,
    Sparse = 0x1,
    SecondaryKey = 0x2,
    Index = 0x4,
    Unknown1 = 0x8,
    Unknown2 = 0x10
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SparseEntry {
    public int Offset;
    public ushort Size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SectionHeader_WDC2 {
    public ulong TactKeyLookup;
    public int FileOffset;
    public int NumRecords;
    public int StringTableSize;
    public int CopyTableSize;
    public int SparseTableOffset; // CatalogDataOffset, absolute value, {uint offset, ushort size}[MaxId - MinId + 1]
    public int IndexDataSize; // int indexData[IndexDataSize / 4]
    public int ParentLookupDataSize; // uint NumRecords, uint minId, uint maxId, {uint id, uint index}[NumRecords], questionable usefulness...
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SectionHeader_WDC3 {
    public ulong TactKeyLookup;
    public int FileOffset;
    public int NumRecords;
    public int StringTableSize;
    public int SparseDataEndOffset; // CatalogDataOffset, absolute value, {uint offset, ushort size}[MaxId - MinId + 1]
    public int IndexDataSize; // int indexData[IndexDataSize / 4]
    public int ParentLookupDataSize; // uint NumRecords, uint minId, uint maxId, {uint id, uint index}[NumRecords], questionable usefulness...
    public int NumSparseRecords;
    public int NumCopyRecords;
}

public class BitReader {
    byte[] _data;
    int _bitPosition;
    int _offset;

    public int Position { get => _bitPosition; set => _bitPosition = value; }
    public int Offset { get => _offset; set => _offset = value; }
    public byte[] Data { get => _data; set => _data = value; }

    public BitReader(byte[] data) { _data = data; }
    public BitReader(byte[] data, int offset) { _data = data; _offset = offset; }

    public T Read<T>(int numBits) where T : unmanaged {
        var result = Unsafe.As<byte, ulong>(ref _data[_offset + (_bitPosition >> 3)]) << (64 - numBits - (_bitPosition & 7)) >> (64 - numBits);
        _bitPosition += numBits;
        return Unsafe.As<ulong, T>(ref result);
    }

    public T ReadSigned<T>(int numBits) where T : unmanaged {
        var result = Unsafe.As<byte, ulong>(ref _data[_offset + (_bitPosition >> 3)]) << (64 - numBits - (_bitPosition & 7)) >> (64 - numBits);
        _bitPosition += numBits;
        var signedShift = (1UL << (numBits - 1));
        result = (signedShift ^ result) - signedShift;
        return Unsafe.As<ulong, T>(ref result);
    }

    public string ReadCString() {
        var start = _bitPosition;
        while (_data[_offset + (_bitPosition >> 3)] != 0) _bitPosition += 8;
        var result = Encoding.UTF8.GetString(_data, _offset + (start >> 3), (_bitPosition - start) >> 3);
        _bitPosition += 8;
        return result;
    }
}

public class FieldReader {
    public static T GetFieldValue<T>(int id, BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData) where T : unmanaged {
        switch (columnMeta.CompressionType) {
            case CompressionType.None: var bitSize = 32 - fieldMeta.Bits; return r.Read<T>(bitSize > 0 ? bitSize : columnMeta.Immediate.BitWidth);
            case CompressionType.Immediate: return r.Read<T>(columnMeta.Immediate.BitWidth);
            case CompressionType.SignedImmediate: return r.ReadSigned<T>(columnMeta.Immediate.BitWidth);
            case CompressionType.Common: return commonData.TryGetValue(id, out var val) ? val.As<T>() : columnMeta.Common.DefaultValue.As<T>();
            case CompressionType.Pallet: var palletIndex = r.Read<uint>(columnMeta.Pallet.BitWidth); return palletData[palletIndex].As<T>();
            default: throw new Exception($"Unexpected compression type {columnMeta.CompressionType}");
        }
    }

    public static T[] GetFieldValueArray<T>(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, int arraySize) where T : unmanaged {
        switch (columnMeta.CompressionType) {
            case CompressionType.None:
                var bitSize = 32 - fieldMeta.Bits;
                var arr1 = new T[arraySize];
                for (var i = 0; i < arr1.Length; i++) arr1[i] = r.Read<T>(bitSize > 0 ? bitSize : columnMeta.Immediate.BitWidth);
                return arr1;
            case CompressionType.Immediate:
                var arr2 = new T[arraySize];
                for (var i = 0; i < arr2.Length; i++) arr2[i] = r.Read<T>(columnMeta.Immediate.BitWidth);
                return arr2;
            case CompressionType.SignedImmediate:
                var arr3 = new T[arraySize];
                for (var i = 0; i < arr3.Length; i++) arr3[i] = r.ReadSigned<T>(columnMeta.Immediate.BitWidth);
                return arr3;
            case CompressionType.PalletArray:
                var cardinality = columnMeta.Pallet.Cardinality;
                if (arraySize != cardinality) throw new Exception("Struct missmatch for pallet array field?");
                var palletArrayIndex = r.Read<uint>(columnMeta.Pallet.BitWidth);
                var arr4 = new T[cardinality];
                for (var i = 0; i < arr4.Length; i++) arr4[i] = palletData[i + cardinality * (int)palletArrayIndex].As<T>();
                return arr4;
            default: throw new Exception($"Unexpected compression type {columnMeta.CompressionType}");
        }
    }

    public static string[] GetFieldValueStringsArray(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringsTable, bool isSparse, int recordOffset, int arraySize) {
        var array = new string[arraySize];
        if (isSparse)
            for (var i = 0; i < array.Length; i++) array[i] = r.ReadCString();
        else {
            var pos = recordOffset + (r.Position >> 3);
            var strIdx = GetFieldValueArray<int>(r, fieldMeta, columnMeta, palletData, commonData, arraySize);
            for (var i = 0; i < array.Length; i++) array[i] = stringsTable[pos + i * 4 + strIdx[i]];
        }
        return array;
    }
}

public class FieldsCache<T> {
    private static readonly FieldCache[] fieldsCache;

    static FieldsCache() {
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f.MetadataToken).ToArray();

        fieldsCache = new FieldCache[fields.Length];

        for (var i = 0; i < fields.Length; i++) fieldsCache[i] = (FieldCache)Activator.CreateInstance(typeof(FieldCache<,>).MakeGenericType(typeof(T), fields[i].FieldType), fields[i]);

        //Console.WriteLine($"FieldsCache<{typeof(T).Name}> created");
    }

    public static FieldCache[] Cache => fieldsCache;
}
