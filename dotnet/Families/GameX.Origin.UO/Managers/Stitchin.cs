// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Stitchin. Reads stitchin.def. coveredBy, covers, replace, and remove do not store a result.
/// </summary>
public sealed class Stitchin {
    static readonly char[] _split = { ' ', '\t' };

    public void Read(string filePath) {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;
        const string endDef = "# enddef";
        using (var reader = new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))) {
            var list = new List<string>();
            var started = false;
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;
                if (!started && line.StartsWith("# ") && !line.StartsWith(endDef))
                    started = true;
                if (started)
                    list.Add(line);
                if (started && line.StartsWith(endDef)) {
                    Work(list);
                    list.Clear();
                }
            }
        }
    }

    void Work(List<string> list) {
        if (list == null || list.Count == 0)
            return;
        foreach (var line in list) {
            var args = line.Split(_split, StringSplitOptions.RemoveEmptyEntries);
            BuildCommand(args);
        }
    }

    void BuildCommand(string[] arguments) {
        if (arguments == null || arguments.Length == 0)
            return;
        var cmd = arguments[0];
        switch (cmd) {
            case "coveredBy":
                for (int i = 1; i < arguments.Length; ++i)
                    GetLayer(arguments[i]);
                break;
            case "covers":
                for (int i = 1; i < arguments.Length; ++i)
                    GetLayer(arguments[i]);
                break;
            case "replace" when arguments.Length == 4:
                _ = arguments[1];
                _ = arguments[3];
                break;
            case "remove":
                break;
        }
    }

    Layer? GetLayer(string layerName) {
        switch (layerName.ToUpper()) {
            case "HEAD": return Layer.Helmet;
            case "FACE": return Layer.Face;
            case "EARS": return Layer.Earrings;
            case "NECK": return Layer.Necklace;
            case "TORSO": return Layer.Torso;
            case "UPPER_ARMS_TOP": return Layer.Invalid;
            case "UPPER_ARMS_BOTTOM": return Layer.Invalid;
            case "LOWER_ARMS_TOP": return Layer.Invalid;
            case "LOWER_ARMS_BOTTOM": return Layer.Invalid;
            case "HANDS": return Layer.Gloves;
            case "PELVIS": return Layer.Waist;
            case "UPPER_LEGS_TOP": return Layer.Invalid;
            case "UPPER_LEGS_BOTTOM": return Layer.Invalid;
            case "LOWER_LEGS_TOP": return Layer.Invalid;
            case "LOWER_LEGS_BOTTOM": return Layer.Invalid;
            case "FEET": return Layer.Shoes;
        }
        return null;
    }

    enum Layer : byte {
        Invalid = 0x00,
        OneHanded = 0x01,
        TwoHanded = 0x02,
        Shoes = 0x03,
        Pants = 0x04,
        Shirt = 0x05,
        Helmet = 0x06,
        Gloves = 0x07,
        Ring = 0x08,
        Talisman = 0x09,
        Necklace = 0x0A,
        Hair = 0x0B,
        Waist = 0x0C,
        Torso = 0x0D,
        Bracelet = 0x0E,
        Face = 0x0F,
        Beard = 0x10,
        Tunic = 0x11,
        Earrings = 0x12,
        Arms = 0x13,
        Cloak = 0x14,
        Backpack = 0x15,
        Robe = 0x16,
        Skirt = 0x17,
        Legs = 0x18,
        Mount = 0x19,
        ShopBuyRestock = 0x1A,
        ShopBuy = 0x1B,
        ShopSell = 0x1C,
        Bank = 0x1D
    }
}
