// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// 0x7C. A row sends 0x7D with a 1-based code. Right-click sends code 0 on a graphic menu.
/// A gray menu has no right-click close. Its cancel row sends code 0.
/// </summary>
public sealed class MenuGump : Gump {
    readonly ushort _menu;
    bool _sent;

    public MenuGump(World world, UOMenu menu) : base(world, menu.Serial, menu.Id) {
        _menu = menu.Id;
        X = 100;
        Y = 100;
        CanMove = true;
        CanCloseWithRightClick = !menu.Gray;
        AcceptMouseInput = true;
        var rows = menu.Entries.Length + (menu.Gray ? 1 : 0);
        if (rows < 1) rows = 1;
        Add(new ResizePic(0x0BB8) { X = 0, Y = 0, Width = 280, Height = 28 + rows * 18 });
        Add(new Label(string.IsNullOrEmpty(menu.Name) ? "menu" : menu.Name, false, 0x0386) { X = 12, Y = 8 });
        for (var i = 0; i < menu.Entries.Length; i++) {
            var e = menu.Entries[i];
            var label = e.Name;
            if (string.IsNullOrEmpty(label)) label = e.Graphic == 0 ? "choice" : e.Graphic.ToString("X4");
            Add(new MenuRow(this, label, (ushort)(i + 1), e.Graphic, e.Hue) { X = 12, Y = 28 + i * 18 });
        }
        if (menu.Gray)
            Add(new MenuRow(this, "cancel", 0, 0, 0) { X = 12, Y = 28 + menu.Entries.Length * 18 });
    }

    public override void CloseWithRightClick() {
        if (!CanCloseWithRightClick || IsDisposed) return;
        Reply(0, 0, 0);
    }

    internal void Reply(ushort code, ushort graphic, ushort hue) {
        if (_sent || IsDisposed) return;
        _sent = true;
        UOLoginNet.ReplyMenu(LocalSerial, _menu, code, graphic, hue);
        Dispose();
    }

    sealed class MenuRow : Label {
        readonly MenuGump _owner;
        readonly ushort _code, _graphic, _hue;

        public MenuRow(MenuGump owner, string text, ushort code, ushort graphic, ushort hue) : base(text, false, 0x0481) {
            _owner = owner;
            _code = code;
            _graphic = graphic;
            _hue = hue;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _owner.Reply(_code, _graphic, _hue);
    }
}
