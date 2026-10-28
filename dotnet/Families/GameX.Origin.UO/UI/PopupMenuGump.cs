// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// 0xBF subcommand 0x0014. A row sends 0x0015. Right-click closes and does not reply.
/// WorldGump calls Show from Update. Text is ClilocText, or the cliloc number when that is empty.
/// </summary>
public sealed class PopupMenuGump : Gump {
    readonly uint _serial;
    bool _sent;

    public static void Show(World world) {
        while (UOLoginNet.TryTakePopup(out var menu)) {
            var open = UIManager.GetGump<PopupMenuGump>();
            if (open != null && !open.IsDisposed) open.Dispose();
            if (menu == null || menu.Entries == null || menu.Entries.Length == 0) continue;
            UIManager.Add(new PopupMenuGump(world, menu));
        }
    }

    public PopupMenuGump(World world, UOPopupMenu menu) : base(world, menu.Serial, 0) {
        _serial = menu.Serial;
        X = Mouse.Position.X;
        Y = Mouse.Position.Y;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        var entries = menu.Entries;
        var width = 40;
        var texts = new string[entries.Length];
        for (var i = 0; i < entries.Length; i++) {
            var text = UOGamePackets.ClilocText(entries[i].Cliloc);
            if (string.IsNullOrEmpty(text)) text = entries[i].Cliloc.ToString();
            texts[i] = text;
            var w = text.Length * 6;
            if (w > width) width = w;
        }
        width += 24;
        Add(new ResizePic(0x0A3C) { X = 0, Y = 0, Width = width, Height = 16 + entries.Length * 18 });
        for (var i = 0; i < entries.Length; i++)
            Add(new PopupRow(this, texts[i], entries[i].Index, entries[i].Hue) { X = 12, Y = 8 + i * 18 });
    }

    internal void Reply(ushort index) {
        if (_sent || IsDisposed) return;
        _sent = true;
        UOLoginNet.ReplyPopup(_serial, index);
        Dispose();
    }

    sealed class PopupRow : Label {
        readonly PopupMenuGump _owner;
        readonly ushort _index;

        public PopupRow(PopupMenuGump owner, string text, ushort index, ushort hue) : base(text, true, hue, 1) {
            _owner = owner;
            _index = index;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Reply(_index);
    }
}
