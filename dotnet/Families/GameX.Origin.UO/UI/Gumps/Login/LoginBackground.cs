// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO LoginBackground. The quit button does not close the window: ISceneHost has no Exit.
/// </summary>
public class LoginBackground : Gump {
    public LoginBackground(World world) : base(world, 0, 0) {
        var version = UltimaOnline.Current?.Version ?? 0;
        if (version >= ClientVersion.CV_706400) {
            Add(new GumpPicTiled(0, 0, 640, 480, 0x0150) { AcceptKeyboardInput = false });
            Add(new GumpPic(0, 4, 0x0151, 0) { AcceptKeyboardInput = false });
        }
        else {
            Add(new GumpPicTiled(0, 0, 640, 480, 0x0E14) { AcceptKeyboardInput = false });
            Add(new GumpPic(0, 0, 0x157C, 0) { AcceptKeyboardInput = false });
            Add(new GumpPic(0, 4, 0x15A0, 0) { AcceptKeyboardInput = false });
            Add(new Button(0, 0x1589, 0x158B, 0x158A) {
                X = 555,
                Y = 4,
                ButtonAction = ButtonAction.Activate,
                AcceptKeyboardInput = false,
            });
        }
        CanCloseWithEsc = false;
        CanCloseWithRightClick = false;
        AcceptKeyboardInput = false;
        LayerOrder = UILayer.Under;
    }
}
