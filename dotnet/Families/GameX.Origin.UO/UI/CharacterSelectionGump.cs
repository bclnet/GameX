// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CharacterSelectionGump. Shows scene.Characters, or no rows when that array is null.
/// Next enters Britannia when a named character exists. New opens the creation shell. No socket.
/// </summary>
public class CharacterSelectionGump : Gump {
    readonly LoginScene _scene;

    public CharacterSelectionGump(World world, LoginScene scene) : base(world, 0, 0) {
        _scene = scene;
        CanCloseWithRightClick = false;
        AcceptKeyboardInput = false;

        var characters = scene.Characters ?? [];
        var yOffset = 150;
        var yBonus = 0;
        var listTitleY = 106;
        var version = UltimaOnline.Current?.Version ?? 0;
        if (version >= ClientVersion.CV_6040 || version >= ClientVersion.CV_5020 && characters.Length > 5) {
            listTitleY = 96;
            yOffset = 125;
            yBonus = 45;
        }

        Add(new ResizePic(0x0A28) { X = 160, Y = 70, Width = 408, Height = 343 + yBonus });
        Add(new Label("Character Selection", false, 0x0386, 2) { X = 267, Y = listTitleY });

        var selected = characters.Length > 0 ? 0 : -1;
        var pos = 0;
        for (var i = 0; i < characters.Length; i++) {
            var name = characters[i];
            if (string.IsNullOrEmpty(name)) continue;
            var y = yOffset + pos * 40;
            Add(new ResizePic(0x0BB8) { X = 224, Y = y, Width = 280, Height = 30 });
            var hue = (ushort)(i == selected ? 0x0021 : 0x034F);
            var textW = name.Length * 6;
            var nameX = textW < 280 ? 224 + (280 - textW) / 2 : 224;
            Add(new Label(name, false, hue, 5) { X = nameX, Y = y });
            pos++;
        }

        Add(new Button((int)Buttons.New, 0x159D, 0x159F, 0x159E) {
            X = 224, Y = 350 + yBonus, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Delete, 0x159A, 0x159C, 0x159B) {
            X = 442, Y = 350 + yBonus, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2) {
            X = 586, Y = 445, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5) {
            X = 610, Y = 445, ButtonAction = ButtonAction.Activate,
        });
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.New:
                _scene.CurrentLoginStep = LoginSteps.CharacterCreation;
                break;
            case Buttons.Prev:
                _scene.CurrentLoginStep = LoginSteps.ServerSelection;
                break;
            case Buttons.Next:
                var characters = _scene.Characters;
                if (characters == null) return;
                for (var i = 0; i < characters.Length; i++) {
                    if (string.IsNullOrEmpty(characters[i])) continue;
                    _scene.CurrentLoginStep = LoginSteps.EnteringBritania;
                    return;
                }
                break;
        }
    }

    enum Buttons { New, Delete, Next, Prev }
}
