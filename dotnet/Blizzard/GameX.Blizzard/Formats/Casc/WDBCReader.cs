﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.Blizzard.Formats.Casc
{
    class DBCRow
    {
        byte[] m_data;
        WDBCReader m_reader;

        public byte[] Data { get { return m_data; } }

        public DBCRow(WDBCReader reader, byte[] data)
        {
            m_reader = reader;
            m_data = data;
        }

        public T GetField<T>(int field)
        {
            object retVal;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    int start = BitConverter.ToInt32(m_data, field * 4), len = 0;
                    while (m_reader.StringTable[start + len] != 0)
                        len++;
                    retVal = Encoding.UTF8.GetString(m_reader.StringTable, start, len);
                    return (T)retVal;
                case TypeCode.Int32:
                    retVal = BitConverter.ToInt32(m_data, field * 4);
                    return (T)retVal;
                case TypeCode.Single:
                    retVal = BitConverter.ToSingle(m_data, field * 4);
                    return (T)retVal;
                default:
                    return default(T);
            }
        }
    }

    class WDBCReader : IEnumerable<KeyValuePair<int, DBCRow>>
    {
        const uint HeaderSize = 20;
        const uint DBCFmtSig = 0x43424457;          // WDBC

        public int RecordsCount { get; set; }
        public int FieldsCount { get; set; }
        public int RecordSize { get; set; }
        public int StringTableSize { get; set; }
        public int MinIndex { get; set; } = int.MaxValue;
        public int MaxIndex { get; set; } = int.MinValue;

        readonly DBCRow[] m_rows;
        readonly byte[] m_stringTable;

        public byte[] StringTable { get { return m_stringTable; } }

        Dictionary<int, DBCRow> m_index = new Dictionary<int, DBCRow>();

        public WDBCReader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDBCReader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException("File DBC is corrupted!");
                }

                if (reader.ReadUInt32() != DBCFmtSig)
                {
                    throw new InvalidDataException("File DBC is corrupted!");
                }

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();

                m_rows = new DBCRow[RecordsCount];

                for (int i = 0; i < RecordsCount; i++)
                {
                    m_rows[i] = new DBCRow(this, reader.ReadBytes(RecordSize));

                    int idx = BitConverter.ToInt32(m_rows[i].Data, 0);

                    if (idx < MinIndex)
                        MinIndex = idx;

                    if (idx > MaxIndex)
                        MaxIndex = idx;

                    m_index[idx] = m_rows[i];
                }

                m_stringTable = reader.ReadBytes(StringTableSize);
            }
        }

        public bool HasRow(int index)
        {
            return m_index.ContainsKey(index);
        }

        public DBCRow GetRow(int index)
        {
            if (!m_index.ContainsKey(index))
                return null;

            return m_index[index];
        }

        public IEnumerator<KeyValuePair<int, DBCRow>> GetEnumerator()
        {
            return m_index.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_index.GetEnumerator();
        }
    }
}
