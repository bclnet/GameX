// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;
using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CharacterSelectionGump. Shows scene.Characters, or no rows when that array is null.
/// A row selects that slot. Next sends 0x5D for the selected name. New opens the creation shell.
/// </summary>
public class CharacterSelectionGump : Gump {
    readonly LoginScene _scene;
    readonly Dictionary<int, Label> _names = new();
    int _selected = -1;

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

        var pos = 0;
        for (var i = 0; i < characters.Length; i++) {
            var name = characters[i];
            if (string.IsNullOrEmpty(name)) continue;
            if (_selected < 0) _selected = i;
            var y = yOffset + pos * 40;
            Add(new ResizePic(0x0BB8) { X = 224, Y = y, Width = 280, Height = 30 });
            var hue = (ushort)(i == _selected ? 0x0021 : 0x034F);
            var textW = GumpArt.Sink?.MeasureText(name, 5) ?? name.Length * 6;
            var nameX = textW < 280 ? 224 + (280 - textW) / 2 : 224;
            var label = new Label(name, false, hue, 5) { X = nameX, Y = y };
            _names[i] = label;
            Add(label);
            Add(new CharHit(this, i) { X = 224, Y = y });
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
                if (characters == null || _selected < 0 || _selected >= characters.Length) return;
                if (string.IsNullOrEmpty(characters[_selected])) return;
                UOLoginSettings.NoteCharacter(LoginScene.Account, _scene.ServerName, characters[_selected]);
                UOLoginNet.SelectCharacter(_selected, characters[_selected]);
                _scene.CurrentLoginStep = LoginSteps.EnteringBritania;
                break;
        }
    }

    void Select(int slot) {
        if (slot == _selected) return;
        if (_names.TryGetValue(_selected, out var prev)) prev.Hue = 0x034F;
        _selected = slot;
        if (_names.TryGetValue(slot, out var next)) next.Hue = 0x0021;
    }

    sealed class CharHit : Control {
        readonly CharacterSelectionGump _gump;
        readonly int _slot;

        public CharHit(CharacterSelectionGump gump, int slot) {
            _gump = gump;
            _slot = slot;
            Width = 280;
            Height = 30;
            AcceptMouseInput = true;
        }

        public override void OnMouseClick(int x, int y) => _gump.Select(_slot);
    }

    enum Buttons { New, Delete, Next, Prev }
}
