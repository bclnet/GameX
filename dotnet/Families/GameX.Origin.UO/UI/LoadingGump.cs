// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO LoadingGump for the connecting steps. The label is the step name.
/// The frame is resize pic 0x0A28. No buttons, no music, no socket.
/// </summary>
public class LoadingGump : Gump {
    public LoadingGump(World world, LoginScene scene) : base(world, 0, 0) {
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        AcceptKeyboardInput = false;
        Add(new ResizePic(0x0A28) { X = 142, Y = 134, Width = 366, Height = 212 });
        Add(new Label(scene.CurrentLoginStep.ToString(), false, 0x0386, 2) { X = 162, Y = 178 });
    }
}
