// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO popup on the login path. OK returns to Main. No socket.
/// </summary>
public class PopupMessageGump : Gump {
    readonly LoginScene _scene;

    public PopupMessageGump(World world, LoginScene scene) : base(world, 0, 0) {
        _scene = scene;
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        AcceptKeyboardInput = false;
        Add(new ResizePic(0x0A28) { X = 142, Y = 134, Width = 366, Height = 212 });
        Add(new Label(scene.PopupMessage ?? "", false, 0x0386, 2) { X = 162, Y = 178 });
        Add(new Button((int)Buttons.Ok, 0x15A4, 0x15A6, 0x15A5) {
            X = 430, Y = 280, ButtonAction = ButtonAction.Activate,
        });
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == (int)Buttons.Ok)
            _scene.CurrentLoginStep = LoginSteps.Main;
    }

    enum Buttons { Ok }
}
