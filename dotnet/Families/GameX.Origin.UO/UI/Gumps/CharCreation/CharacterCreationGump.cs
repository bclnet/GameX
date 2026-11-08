// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;

namespace GameX.Origin.UO;

/// <summary>
/// Login character-creation shell. No paperdoll, race, profession, or city pages.
/// Next sends 0x00 or 0xF8. Stats and skills are the fixed starter in UOGamePackets.CreateCharacter.
/// </summary>
public class CharacterCreationGump : Gump {
    readonly LoginScene _scene;
    readonly TextBox _name;
    readonly Checkbox _male, _female;

    public CharacterCreationGump(World world, LoginScene scene) : base(world, 0, 0) {
        _scene = scene;
        CanCloseWithRightClick = false;
        AcceptKeyboardInput = false;

        Add(new ResizePic(0x0A28) { X = 160, Y = 70, Width = 408, Height = 343 });
        Add(new Label("Character Creation", false, 0x0386, 2) { X = 267, Y = 106 });
        Add(new Label("Name", false, 0x0386, 2) { X = 224, Y = 150 });
        Add(new ResizePic(0x0BB8) { X = 224, Y = 170, Width = 280, Height = 30 });
        Add(_name = new TextBox {
            X = 231, Y = 173, Width = 266, Height = 25, Hue = 0x034F, MaxLength = 16,
        });
        Add(_male = new Checkbox(0x00D2, 0x00D3, "Male", 2, 0x0386, false) { X = 224, Y = 220 });
        Add(_female = new Checkbox(0x00D2, 0x00D3, "Female", 2, 0x0386, false) { X = 320, Y = 220 });
        _male.IsChecked = true;
        _male.ValueChanged += OnSexChanged;
        _female.ValueChanged += OnSexChanged;
        Add(new Button((int)Buttons.Prev, 0x15A1, 0x15A3, 0x15A2) {
            X = 586, Y = 445, ButtonAction = ButtonAction.Activate,
        });
        Add(new Button((int)Buttons.Next, 0x15A4, 0x15A6, 0x15A5) {
            X = 610, Y = 445, ButtonAction = ButtonAction.Activate,
        });
        _name.SetKeyboardFocus();
    }

    public override void OnButtonClick(int buttonID) {
        switch ((Buttons)buttonID) {
            case Buttons.Prev:
                _scene.CurrentLoginStep = LoginSteps.CharacterSelection;
                break;
            case Buttons.Next:
                var name = _name.Text;
                if (string.IsNullOrWhiteSpace(name)) break;
                UOLoginNet.CreateCharacter(name, _female.IsChecked, _scene.Characters?.Length ?? 0);
                _scene.CurrentLoginStep = LoginSteps.CharacterCreationDone;
                break;
        }
    }

    void OnSexChanged(object sender, EventArgs e) {
        if (sender == _male && _male.IsChecked) _female.IsChecked = false;
        else if (sender == _female && _female.IsChecked) _male.IsChecked = false;
    }

    enum Buttons { Prev, Next }
}
