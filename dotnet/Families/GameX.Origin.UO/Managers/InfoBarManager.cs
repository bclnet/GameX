// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO InfoBarManager. The profile file is not read or written.
/// A missing file keeps the default rows in memory.
/// </summary>
public sealed class InfoBarManager {
    readonly List<InfoBarItem> infoBarItems;
    string _xml;

    public InfoBarManager(World world) {
        _ = world;
        infoBarItems = new List<InfoBarItem>();
    }

    public List<InfoBarItem> GetInfoBars() => infoBarItems;

    public static string[] GetVars() => Enum.GetNames(typeof(InfoBarVars));

    public void AddItem(InfoBarItem ibi) => infoBarItems.Add(ibi);

    public void RemoveItem(InfoBarItem item) => infoBarItems.Remove(item);

    public void Clear() => infoBarItems.Clear();

    public void Save() {
        // Profile path is not ported.
        var sb = new StringBuilder();
        using (var xml = new XmlTextWriter(new StringWriter(sb)) {
            Formatting = Formatting.Indented,
            IndentChar = '\t',
            Indentation = 1
        }) {
            xml.WriteStartDocument(true);
            xml.WriteStartElement("infos");
            foreach (var info in infoBarItems)
                info.Save(xml);
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
        _xml = sb.ToString();
    }

    public void Load() {
        // Profile path is not ported.
        if (string.IsNullOrEmpty(_xml)) {
            CreateDefault();
            Save();
            return;
        }
        var doc = new XmlDocument();
        try {
            doc.LoadXml(_xml);
        }
        catch (Exception) {
            return;
        }
        infoBarItems.Clear();
        var root = doc["infos"];
        if (root == null) return;
        foreach (XmlElement xml in root.GetElementsByTagName("info"))
            infoBarItems.Add(new InfoBarItem(xml));
    }

    public void CreateDefault() {
        infoBarItems.Clear();
        infoBarItems.Add(new InfoBarItem("", InfoBarVars.NameNotoriety, 0x3D2));
        infoBarItems.Add(new InfoBarItem("Hits", InfoBarVars.HP, 0x1B6));
        infoBarItems.Add(new InfoBarItem("Mana", InfoBarVars.Mana, 0x1ED));
        infoBarItems.Add(new InfoBarItem("Stam", InfoBarVars.Stamina, 0x22E));
        infoBarItems.Add(new InfoBarItem("Weight", InfoBarVars.Weight, 0x3D2));
    }
}

/// <summary>ClassicUO InfoBarVars.</summary>
public enum InfoBarVars {
    HP = 0,
    Mana,
    Stamina,
    Weight,
    Followers,
    Gold,
    Damage,
    Armor,
    Luck,
    FireResist,
    ColdResist,
    PoisonResist,
    EnergyResist,
    LowerReagentCost,
    SpellDamageInc,
    FasterCasting,
    FasterCastRecovery,
    HitChanceInc,
    DefenseChanceInc,
    LowerManaCost,
    DamageChanceInc,
    SwingSpeedInc,
    StatsCap,
    NameNotoriety,
    TithingPoints
}

/// <summary>ClassicUO InfoBarItem.</summary>
public class InfoBarItem {
    public InfoBarItem(string label, InfoBarVars var, ushort labelColor) {
        this.label = label;
        this.var = var;
        hue = labelColor;
    }

    public InfoBarItem(XmlElement xml) {
        if (xml == null) return;
        label = xml.GetAttribute("text");
        var = (InfoBarVars)int.Parse(xml.GetAttribute("var"));
        hue = ushort.Parse(xml.GetAttribute("hue"));
    }

    public ushort hue;
    public string label;
    public InfoBarVars var;

    public void Save(XmlTextWriter writer) {
        writer.WriteStartElement("info");
        writer.WriteAttributeString("text", label);
        writer.WriteAttributeString("var", ((int)var).ToString());
        writer.WriteAttributeString("hue", hue.ToString());
        writer.WriteEndElement();
    }
}
