// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;

namespace GameX.Origin.UO;

/// <summary>
/// Login credits. Drawn over the login gump that is already up. No socket and no login-step change.
/// Back disposes this gump only.
/// </summary>
public class CreditsGump : Gump {
    Control _prevFocus;

    public CreditsGump(World world) : base(world, 0, 0) {
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        // Full screen so the login arrow and text boxes under this gump cannot submit.
        WantUpdateSize = false;
        Width = 640;
        Height = 480;
        AcceptMouseInput = true;
        AcceptKeyboardInput = true;

        var version = UltimaOnline.Current?.Version ?? 0;
        var versionText = UltimaOnline.Current?.VersionText;
        if (string.IsNullOrEmpty(versionText)) versionText = version.ToString();
        var hue = version < ClientVersion.CV_706400 ? (ushort)0x034E : (ushort)0x0481;

        Add(new ResizePic(0x0A28) { X = 142, Y = 134, Width = 366, Height = 212 });
        Add(new Label("Credits", false, 0x0386, 2) { X = 162, Y = 160 });
        Add(new Label("GameX", false, hue, 9) { X = 162, Y = 186 });
        Add(new Label($"UO Version {versionText}.", false, hue, 9) { X = 162, Y = 210 });
        Add(new Button((int)Buttons.Back, 0x15A1, 0x15A3, 0x15A2) {
            X = 430, Y = 280, ButtonAction = ButtonAction.Activate,
        });

        _prevFocus = UIManager.KeyboardFocusControl;
        UIManager.KeyboardFocusControl = this;
        LayerOrder = UILayer.Over;
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == (int)Buttons.Back) Dispose();
    }

    public override void Dispose() {
        if (IsDisposed) return;
        var prev = _prevFocus;
        _prevFocus = null;
        base.Dispose();
        if (prev != null && !prev.IsDisposed)
            UIManager.KeyboardFocusControl = prev;
    }

    enum Buttons { Back }
}
