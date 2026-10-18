// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Gump. Save and server reply are not here. Dispose does not touch items:
/// World.Items is not a typed item list yet.
/// </summary>
public class Gump : Control {
    public Gump(World world, uint local, uint server) {
        World = world;
        LocalSerial = local;
        ServerSerial = server;
        AcceptMouseInput = false;
        AcceptKeyboardInput = false;
    }

    public World World { get; }
    public bool InvalidateContents { get; set; }

    public override void Update() {
        if (InvalidateContents) {
            UpdateContents();
            InvalidateContents = false;
        }
        if (ActivePage == 0) ActivePage = 1;
        base.Update();
    }

    public override bool Draw(int x, int y) => IsVisible && base.Draw(x, y);

    protected virtual void UpdateContents() { }
}
