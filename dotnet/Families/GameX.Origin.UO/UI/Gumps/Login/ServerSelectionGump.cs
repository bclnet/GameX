// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ServerSelectionGump. Rows come from LoginScene.Servers.
/// Next and Earth select a server when the login socket has a list; otherwise they jump to character selection.
/// Prev goes to Main. No ping.
/// </summary>
public class ServerSelectionGump : Gump {
    readonly LoginScene _scene;
    readonly string[] _servers;

    public ServerSelectionGump(World world, LoginScene scene) : base(world, 0, 0) {
        _scene = scene;
        _servers = scene.Servers ?? [];
        CanCloseWithRightClick = false;
        AcceptKeyboardInput = false;

        Add(new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2) {
            X = 586, Y = 445, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5) {
            X = 610, Y = 445, ButtonAction = ButtonAction.Activate,
        });

        bool unicode;
        byte font;
        ushort hue;
        var version = UltimaOnline.Current?.Version ?? 0;
        if (version >= ClientVersion.CV_500A) {
            unicode = true;
            font = 1;
            hue = 0xFFFF;
        }
        else {
            unicode = false;
            font = 9;
            hue = 0x0481;
        }

        Add(new Label("Select which shard to play on:", unicode, hue, font) { X = 155, Y = 70 });
        Add(new Label("Latency:", unicode, hue, font) { X = 400, Y = 70 });
        Add(new Label("Packet Loss:", unicode, hue, font) { X = 470, Y = 70 });
        Add(new Label("Sort by:", unicode, hue, font) { X = 153, Y = 368 });
        Add(new Button((int)Buttons.SortTimeZone, 0x093B, 0x093C, 0x093D) { X = 230, Y = 366 });
        Add(new Button((int)Buttons.SortFull, 0x093E, 0x093F, 0x0940) { X = 338, Y = 366 });
        Add(new Button((int)Buttons.SortConnection, 0x0941, 0x0942, 0x0943) { X = 446, Y = 366 });
        Add(new GumpPic(150, 390, 0x0589, 0));
        Add(new Button((int)Buttons.Earth, 0x15E8, 0x15EA, 0x15E9) {
            X = 160, Y = 400, ButtonAction = ButtonAction.Activate,
        });
        Add(new ResizePic(0x0DAC) { X = 150, Y = 90, Width = 393 - 14, Height = 271 });

        var row = 0;
        for (var i = 0; i < _servers.Length; i++) {
            var name = _servers[i];
            if (string.IsNullOrEmpty(name)) continue;
            Add(new Label(name, false, 0x034F, 5) { X = 224, Y = 90 + row * 25 });
            row++;
        }
        if (_servers.Length != 0)
            Add(new Label(_servers[0], false, 0x0481, 9) { X = 243, Y = 420 });
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.Prev:
                _scene.CurrentLoginStep = LoginSteps.Main;
                break;
            case Buttons.Next:
            case Buttons.Earth:
                if (!UOLoginNet.Started || _servers.Length == 0)
                    _scene.CurrentLoginStep = LoginSteps.CharacterSelection;
                else {
                    var name = _servers.Length > 0 ? _servers[0] : "";
                    _scene.ServerName = name;
                    UOLoginSettings.NoteServer(name, 0);
                    UOLoginNet.SelectServer(0);
                    _scene.CurrentLoginStep = LoginSteps.LoginInToServer;
                }
                break;
        }
    }

    enum Buttons { Prev, Next, SortTimeZone, SortFull, SortConnection, Earth }
}
