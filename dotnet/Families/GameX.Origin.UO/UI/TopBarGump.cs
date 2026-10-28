// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO TopBarGump. Page 1 is the wide bar. Page 2 is the 30 by 27 bar.
/// Map opens MiniMapGump. Journal opens JournalGump. Button has no caption, so the names are labels.
/// Paperdoll and Backpack call Use when that serial is nonzero. War calls ToggleWar.
/// </summary>
public sealed class TopBarGump : Gump {
    const ushort SmallButton = 0x098B;
    const ushort LargeButton = 0x098D;
    int _page = 1;

    public TopBarGump(World world) : base(world, 0, 0) {
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = false;
        ActivePage = 1;
        LayerOrder = UILayer.Over;

        Add(new ResizePic(0x13BE) { Width = 30, Height = 27, AcceptMouseInput = true }, 2);
        Add(new Button(0, 0x15A1, 0x15A1, 0x15A1) {
            X = 5, Y = 3, ToPage = 1, IsPageButton = true,
        }, 2);

        var smallWidth = WidthOf(SmallButton, 50);
        var largeWidth = WidthOf(LargeButton, 100);
        var background = new ResizePic(0x13BE) { Height = 27, AcceptMouseInput = true };
        Add(background, 1);
        Add(new Button(0, 0x15A4, 0x15A4, 0x15A4) {
            X = 5, Y = 3, ToPage = 2, IsPageButton = true,
        }, 1);

        var x = 30;
        x = AddButton((int)Buttons.Map, SmallButton, smallWidth, "Map", x);
        x = AddButton((int)Buttons.Paperdoll, LargeButton, largeWidth, "Paperdoll", x);
        x = AddButton((int)Buttons.Backpack, LargeButton, largeWidth, "Backpack", x);
        x = AddButton((int)Buttons.Journal, LargeButton, largeWidth, "Journal", x);
        x = AddButton((int)Buttons.War, SmallButton, smallWidth, "War", x);
        background.Width = x + 1;
    }

    public override void Update() {
        if (ActivePage != 0 && ActivePage != _page) WantUpdateSize = true;
        base.Update();
        _page = ActivePage;
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.Map:
                if (UIManager.GetGump<MiniMapGump>() == null) UIManager.Add(new MiniMapGump(World));
                break;
            case Buttons.Paperdoll:
                var player = UOLoginNet.PlayerSerial;
                if (player != 0) UOLoginNet.Use(player);
                break;
            case Buttons.Backpack:
                var pack = UOLoginNet.PlayerBackpack;
                if (pack != 0) UOLoginNet.Use(pack);
                break;
            case Buttons.Journal:
                if (UIManager.GetGump<JournalGump>() == null) UIManager.Add(new JournalGump(World));
                break;
            case Buttons.War:
                UOLoginNet.ToggleWar();
                break;
        }
    }

    int AddButton(int id, ushort graphic, int fallback, string text, int x) {
        var button = new Button(id, graphic, graphic, graphic) {
            X = x, Y = 1, ButtonAction = ButtonAction.Activate,
        };
        if (button.IsDisposed) return x;
        var span = button.Width > 0 ? button.Width : fallback;
        var label = new Label(text, true, 0, 1);
        label.X = (span - label.Width) >> 1;
        var h = button.Height;
        label.Y = h > label.Height ? (h - label.Height) >> 1 : 0;
        button.Add(label);
        Add(button, 1);
        return x + span + 1;
    }

    static int WidthOf(ushort graphic, int fallback) {
        if (GumpMetrics.TrySize(graphic, out var w, out _) && w > 0) return w;
        return fallback;
    }

    enum Buttons {
        Map = 0,
        Paperdoll = 1,
        Backpack = 2,
        Journal = 3,
        War = 4,
    }
}
