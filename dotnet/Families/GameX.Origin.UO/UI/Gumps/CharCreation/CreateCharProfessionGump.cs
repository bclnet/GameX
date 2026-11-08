// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO profession page. Rows come from Prof.txt, then Advanced.
/// No profile and no plugin host. A category opens its children.
/// </summary>
public class CreateCharProfessionGump : Gump {
    static ProfessionBook _book;
    readonly ProfessionInfo _parent;

    public CreateCharProfessionGump(World world, ProfessionInfo parent = null) : base(world, 0, 0) {
        _parent = parent;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        var book = Book();
        List<ProfessionInfo> rows = null;
        if (parent != null) book.Children.TryGetValue(parent, out rows);
        if (rows == null) rows = book.Roots;

        Add(new ResizePic(2600) { X = 100, Y = 80, Width = 470, Height = 372 });
        AddPic(291, 42, 0x0589);
        AddPic(214, 58, 0x058B);
        AddPic(300, 51, 0x15A9);
        Add(new Label(CharCreationGump.Cliloc(3000326, "Choose a Trade for Your Character"), false, 0x0386, 2) { X = 158, Y = 132 });

        for (var i = 0; i < rows.Count; i++) {
            var cx = i % 2;
            var cy = i >> 1;
            Add(new ProfessionInfoGump(rows[i], SelectProfession) { X = 145 + cx * 195, Y = 168 + cy * 70 });
        }

        var prev = new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2) {
            X = 586, Y = 445, ButtonAction = ButtonAction.Activate,
        };
        if (!prev.IsDisposed) Add(prev);
    }

    public void SelectProfession(ProfessionInfo info) {
        if (info == null) return;
        var shell = CharCreationGump.Shell(this);
        if (info.Type == ProfessionKind.Category && Book().Children.TryGetValue(info, out var list) && list != null) {
            Parent?.Add(new CreateCharProfessionGump(World, info), Page);
            Parent = null;
            Dispose();
            return;
        }
        shell?.SetProfession(info);
    }

    public override void OnButtonClick(int buttonID) {
        if ((Buttons)buttonID != Buttons.Prev) {
            base.OnButtonClick(buttonID);
            return;
        }
        var shell = CharCreationGump.Shell(this);
        if (_parent != null && _parent.TopLevel) {
            Parent?.Add(new CreateCharProfessionGump(World), Page);
            Parent = null;
            Dispose();
        }
        else {
            Parent = null;
            Dispose();
            shell?.StepBack();
        }
    }

    void AddPic(int x, int y, ushort graphic) {
        var pic = new GumpPic(x, y, graphic, 0) { AcceptMouseInput = false, CanMove = false };
        if (!pic.IsDisposed) Add(pic);
    }

    static ProfessionBook Book() => _book ??= ProfessionBook.Load(CharCreationGump.Version);

    enum Buttons { Prev }

    sealed class ProfessionInfoGump : Control {
        readonly ProfessionInfo _info;
        readonly Action<ProfessionInfo> _selected;

        public ProfessionInfoGump(ProfessionInfo info, Action<ProfessionInfo> selected) {
            _info = info;
            _selected = selected;
            Width = 175;
            Height = 34;
            AcceptMouseInput = true;
            CanMove = false;
            WantUpdateSize = false;
            Add(new ResizePic(3000) { Width = 175, Height = 34, AcceptMouseInput = false, CanMove = false });
            Add(new Label(CharCreationGump.Cliloc(info.Localization, info.Name), true, 0, 1) { X = 7, Y = 8 });
            var icon = new GumpPic(121, -12, info.Graphic, 0) { AcceptMouseInput = false, CanMove = false };
            if (!icon.IsDisposed) Add(icon);
        }

        public override void OnMouseClick(int x, int y) => _selected?.Invoke(_info);
    }
}

/// <summary>ClassicUO ProfessionLoader. Prof.txt when it is present, then Advanced.</summary>
public sealed class ProfessionBook {
    public List<ProfessionInfo> Roots { get; } = new();
    public Dictionary<ProfessionInfo, List<ProfessionInfo>> Children { get; } = new();

    public static ProfessionBook Load(ClientVersion version) {
        var book = new ProfessionBook();
        var path = UltimaOnline.Current?.ClientPath;
        if (!string.IsNullOrEmpty(path)) {
            var file = Path.Combine(path, "Prof.txt");
            if (File.Exists(file)) {
                var info = new FileInfo(file);
                if (info.Length > 0x100000)
                    throw new InternalBufferOverflowException(file + " exceeds the maximum 1Megabyte allowed size");
                Parse(book, version, File.ReadAllText(file));
            }
        }
        book.Roots.Add(new ProfessionInfo(version) {
            Name = "Advanced",
            TrueName = "advanced",
            Localization = 1061176,
            Description = 1061226,
            Graphic = 5545,
            TopLevel = true,
            Type = ProfessionKind.Profession,
            DescriptionIndex = -1,
        });
        return book;
    }

    static void Parse(ProfessionBook book, ClientVersion version, string text) {
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (var i = 0; i < lines.Length; i++) {
            var tokens = Tokens(lines[i]);
            if (tokens.Count > 0 && tokens[0].Equals("begin", StringComparison.OrdinalIgnoreCase))
                if (!Read(book, version, lines, ref i)) return;
        }
    }

    static bool Read(ProfessionBook book, ClientVersion version, string[] lines, ref int i) {
        var children = new List<string>();
        var type = ProfessionKind.None;
        var name = "";
        var trueName = "";
        var nameId = 0;
        var descId = 0;
        var descIndex = 0;
        ushort gump = 0;
        var top = false;
        var skills = new[,] { { 0xFF, 0 }, { 0xFF, 0 }, { 0xFF, 0 }, { 0xFF, 0 } };
        var stats = new int[3];
        while (++i < lines.Length) {
            var tokens = Tokens(lines[i]);
            if (tokens.Count < 1) continue;
            var key = tokens[0].ToLowerInvariant();
            if (key == "begin" || key == "end") break;
            switch (key) {
                case "name":
                    if (tokens.Count > 1) name = tokens[1];
                    break;
                case "truename":
                    if (tokens.Count > 1) trueName = tokens[1];
                    break;
                case "desc":
                    if (tokens.Count > 1) int.TryParse(tokens[1], out descIndex);
                    break;
                case "toplevel":
                    top = tokens.Count > 1 && tokens[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "gump":
                    if (tokens.Count > 1) ushort.TryParse(tokens[1], out gump);
                    break;
                case "type":
                    if (tokens.Count > 1 && tokens[1].Equals("category", StringComparison.OrdinalIgnoreCase)) type = ProfessionKind.Category;
                    else type = ProfessionKind.Profession;
                    break;
                case "children":
                    for (var c = 1; c < tokens.Count; c++) children.Add(tokens[c]);
                    break;
                case "skill":
                    if (tokens.Count > 2) {
                        var slot = 0;
                        for (var s = 0; s < 4; s++)
                            if (skills[s, 0] == 0xFF) { slot = s; break; }
                        var index = CharSkillNames.IndexOf(tokens[1]);
                        if (index >= 0) {
                            skills[slot, 0] = index;
                            int.TryParse(tokens[2], out skills[slot, 1]);
                        }
                    }
                    break;
                case "stat":
                    if (tokens.Count > 2) {
                        int.TryParse(tokens[2], out var val);
                        var which = tokens[1].ToLowerInvariant();
                        if (which == "str") stats[0] = val;
                        else if (which == "int") stats[1] = val;
                        else if (which == "dex") stats[2] = val;
                    }
                    break;
                case "nameid":
                    if (tokens.Count > 1) int.TryParse(tokens[1], out nameId);
                    break;
                case "descid":
                    if (tokens.Count > 1) int.TryParse(tokens[1], out descId);
                    break;
            }
        }
        if (type == ProfessionKind.None) return true;
        var row = new ProfessionInfo(version) {
            Name = nameId != 0 ? CharCreationGump.Cliloc(nameId, name) : name,
            TrueName = trueName,
            Localization = nameId,
            Description = descId,
            DescriptionIndex = descIndex,
            TopLevel = top,
            Graphic = gump,
            Type = type,
            Children = type == ProfessionKind.Category ? children : null,
        };
        if (type == ProfessionKind.Profession) {
            row.StatsVal = stats;
            row.SkillDefVal = skills;
        }
        if (top) {
            book.Roots.Add(row);
            if (type == ProfessionKind.Category) book.Children[row] = new List<ProfessionInfo>();
            return true;
        }
        foreach (var root in book.Roots) {
            if (root.Children == null || !root.Children.Contains(trueName)) continue;
            if (!book.Children.TryGetValue(root, out var list)) {
                list = new List<ProfessionInfo>();
                book.Children[root] = list;
            }
            list.Add(row);
            break;
        }
        return true;
    }

    static List<string> Tokens(string line) {
        var list = new List<string>();
        if (string.IsNullOrEmpty(line)) return list;
        var cur = new StringBuilder();
        var quote = false;
        for (var i = 0; i < line.Length; i++) {
            var c = line[i];
            if (quote) {
                if (c == '"') quote = false;
                else cur.Append(c);
                continue;
            }
            if (c == '"') { quote = true; continue; }
            if (c == '#' || c == ';') break;
            if (c == ' ' || c == '\t' || c == ',') {
                if (cur.Length > 0) { list.Add(cur.ToString()); cur.Clear(); }
                continue;
            }
            cur.Append(c);
        }
        if (cur.Length > 0) list.Add(cur.ToString());
        return list;
    }
}
