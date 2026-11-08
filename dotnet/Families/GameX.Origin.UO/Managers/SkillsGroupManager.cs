// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO SkillsGroup. Names stay the strings ClassicUO writes.
/// Sort uses skill index order. skills.mul name order is not in this file.
/// </summary>
public sealed class SkillsGroup {
    // FileManager.Skills.SkillsCount is not in this file. The list holds 60.
    const int SkillsCount = 60;
    readonly byte[] _list = new byte[60];

    public SkillsGroup() {
        for (var i = 0; i < _list.Length; i++)
            _list[i] = 0xFF;
    }

    public SkillsGroup Left { get; set; }
    public SkillsGroup Right { get; set; }
    public int Count;
    public bool IsMaximized;
    public string Name = "No name";

    public byte GetSkill(int index) {
        if (index < 0 || index >= Count) return 0xFF;
        return _list[index];
    }

    public void Add(byte item) {
        if (!Contains(item))
            _list[Count++] = item;
    }

    public void Remove(byte item) {
        var removed = false;
        for (var i = 0; i < Count; i++) {
            if (_list[i] == item) {
                removed = true;
                for (; i < Count - 1; i++)
                    _list[i] = _list[i + 1];
                break;
            }
        }
        if (removed) {
            Count--;
            if (Count < 0) Count = 0;
            _list[Count] = 0xFF;
        }
    }

    public bool Contains(byte item) {
        for (var i = 0; i < Count; i++)
            if (_list[i] == item) return true;
        return false;
    }

    public unsafe void Sort() {
        byte* table = stackalloc byte[60];
        var index = 0;
        for (var i = 0; i < SkillsCount; i++) {
            for (var j = 0; j < Count; j++) {
                if (i == _list[j]) {
                    table[index++] = _list[j];
                    break;
                }
            }
        }
        for (var j = 0; j < Count; j++)
            _list[j] = table[j];
    }

    public void TransferTo(SkillsGroup group) {
        for (var i = 0; i < Count; i++)
            group.Add(_list[i]);
        group.Sort();
    }

    public void Save(XmlTextWriter xml) {
        xml.WriteStartElement("group");
        xml.WriteAttributeString("name", Name);
        xml.WriteStartElement("skillids");
        for (var i = 0; i < Count; i++) {
            var idx = GetSkill(i);
            if (idx != 0xFF) {
                xml.WriteStartElement("skill");
                xml.WriteAttributeString("id", idx.ToString());
                xml.WriteEndElement();
            }
        }
        xml.WriteEndElement();
        xml.WriteEndElement();
    }
}

/// <summary>
/// ClassicUO SkillsGroupManager. The profile file is not read or written.
/// A missing file keeps the default groups in memory.
/// </summary>
public sealed class SkillsGroupManager {
    // FileManager.Skills.SkillsCount is not in this file. The group list holds 60.
    const int SkillsCount = 60;
    string _xml;

    public SkillsGroupManager(World world) => _ = world;

    public readonly List<SkillsGroup> Groups = new List<SkillsGroup>();

    public void Add(SkillsGroup g) => Groups.Add(g);

    public bool Remove(SkillsGroup g) {
        if (Groups[0] == g) {
            // MessageBoxGump is not in this file.
            return false;
        }
        Groups.Remove(g);
        g.TransferTo(Groups[0]);
        return true;
    }

    public void Load() {
        Groups.Clear();
        // Profile path is not ported.
        if (string.IsNullOrEmpty(_xml)) {
            MakeDefault();
            return;
        }
        var doc = new XmlDocument();
        try {
            doc.LoadXml(_xml);
        }
        catch (Exception) {
            MakeDefault();
            return;
        }
        var root = doc["skillsgroups"];
        if (root == null) return;
        foreach (XmlElement xml in root.GetElementsByTagName("group")) {
            var g = new SkillsGroup();
            g.Name = xml.GetAttribute("name");
            var xmlIdsRoot = xml["skillids"];
            if (xmlIdsRoot != null) {
                foreach (XmlElement xmlIds in xmlIdsRoot.GetElementsByTagName("skill"))
                    g.Add(byte.Parse(xmlIds.GetAttribute("id")));
            }
            g.Sort();
            Add(g);
        }
    }

    public void Save() {
        // Profile path is not ported.
        var sb = new StringBuilder();
        using (var xml = new XmlTextWriter(new StringWriter(sb)) {
            Formatting = Formatting.Indented,
            IndentChar = '\t',
            Indentation = 1
        }) {
            xml.WriteStartDocument(true);
            xml.WriteStartElement("skillsgroups");
            foreach (var k in Groups)
                k.Save(xml);
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
        _xml = sb.ToString();
    }

    public void MakeDefault() {
        Groups.Clear();
        // skillgrp.mul is not opened. A missing file uses the in-memory groups.
        if (!LoadMULFile(null)) {
            MakeDefaultMiscellaneous();
            MakeDefaultCombat();
            MakeDefaultTradeSkills();
            MakeDefaultMagic();
            MakeDefaultWilderness();
            MakeDefaultThieving();
            MakeDefaultBard();
        }
        foreach (var g in Groups)
            g.Sort();
        Save();
    }

    void MakeDefaultMiscellaneous() {
        var g = new SkillsGroup();
        g.Name = "Miscellaneous";
        g.Add(4);
        g.Add(6);
        g.Add(10);
        g.Add(12);
        g.Add(19);
        g.Add(3);
        g.Add(36);
        Add(g);
    }

    void MakeDefaultCombat() {
        var count = SkillsCount;
        var g = new SkillsGroup();
        g.Name = "Combat";
        g.Add(1);
        g.Add(31);
        g.Add(42);
        g.Add(17);
        g.Add(41);
        g.Add(5);
        g.Add(40);
        g.Add(27);
        if (count > 57) g.Add(57);
        g.Add(43);
        if (count > 50) g.Add(50);
        if (count > 51) g.Add(51);
        if (count > 52) g.Add(52);
        if (count > 53) g.Add(53);
        Add(g);
    }

    void MakeDefaultTradeSkills() {
        var g = new SkillsGroup();
        g.Name = "Trade Skills";
        g.Add(0);
        g.Add(7);
        g.Add(8);
        g.Add(11);
        g.Add(13);
        g.Add(23);
        g.Add(44);
        g.Add(45);
        g.Add(34);
        g.Add(37);
        Add(g);
    }

    void MakeDefaultMagic() {
        var count = SkillsCount;
        var g = new SkillsGroup();
        g.Name = "Magic";
        g.Add(16);
        if (count > 56) g.Add(56);
        g.Add(25);
        g.Add(46);
        if (count > 55) g.Add(55);
        g.Add(26);
        if (count > 54) g.Add(54);
        g.Add(32);
        if (count > 49) g.Add(49);
        Add(g);
    }

    void MakeDefaultWilderness() {
        var g = new SkillsGroup();
        g.Name = "Wilderness";
        g.Add(2);
        g.Add(35);
        g.Add(18);
        g.Add(20);
        g.Add(38);
        g.Add(39);
        Add(g);
    }

    void MakeDefaultThieving() {
        var g = new SkillsGroup();
        g.Name = "Thieving";
        g.Add(14);
        g.Add(21);
        g.Add(24);
        g.Add(30);
        g.Add(48);
        g.Add(28);
        g.Add(33);
        g.Add(47);
        Add(g);
    }

    void MakeDefaultBard() {
        var g = new SkillsGroup();
        g.Name = "Bard";
        g.Add(15);
        g.Add(29);
        g.Add(9);
        g.Add(22);
        Add(g);
    }

    bool LoadMULFile(string path) {
        if (string.IsNullOrEmpty(path)) return false;
        var info = new FileInfo(path);
        if (!info.Exists) return false;
        try {
            byte skillidx = 0;
            var unicode = false;
            using (var bin = new BinaryReader(File.OpenRead(info.FullName))) {
                var start = 4;
                var strlen = 17;
                var count = bin.ReadInt32();
                if (count == -1) {
                    unicode = true;
                    count = bin.ReadInt32();
                    start *= 2;
                    strlen *= 2;
                }
                var sb = new StringBuilder(17);
                var g = new SkillsGroup();
                g.Name = "Miscellaneous";
                var groups = new SkillsGroup[count];
                groups[0] = g;
                for (var i = 0; i < count - 1; ++i) {
                    short strbuild;
                    bin.BaseStream.Seek(start + i * strlen, SeekOrigin.Begin);
                    if (unicode) {
                        while ((strbuild = bin.ReadInt16()) != 0)
                            sb.Append((char)strbuild);
                    }
                    else {
                        while ((strbuild = bin.ReadByte()) != 0)
                            sb.Append((char)strbuild);
                    }
                    groups[i + 1] = new SkillsGroup { Name = sb.ToString() };
                    sb.Clear();
                }
                bin.BaseStream.Seek(start + (count - 1) * strlen, SeekOrigin.Begin);
                while (bin.BaseStream.Length != bin.BaseStream.Position) {
                    var grp = bin.ReadInt32();
                    if (grp < groups.Length && skillidx < SkillsCount)
                        groups[grp].Add(skillidx++);
                }
                for (var i = 0; i < groups.Length; i++)
                    Add(groups[i]);
            }
        }
        catch (Exception) {
            return false;
        }
        return Groups.Count != 0;
    }
}
