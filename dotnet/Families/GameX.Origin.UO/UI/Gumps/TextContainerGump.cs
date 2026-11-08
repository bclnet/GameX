// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO TextContainerGump. ClassicUO passes a TextObject and sets its Time to Ticks + 4000.
/// TextObject is not a public type here. AddText stores the line on TextRenderer.
/// Draw asks the renderer to walk the lines. The renderer does not upload pixels.
/// </summary>
public abstract class TextContainerGump : Gump {
    protected TextContainerGump(World world, uint local, uint server) : base(world, local, server) {
        TextRenderer = new GameX.Origin.UO.TextRenderer(World);
    }

    public TextRenderer TextRenderer { get; }

    public void AddText(string text, ushort hue, int x, int y, bool owned = false, byte font = 1) {
        if (text == null) return;
        TextRenderer.AddMessage(text, hue, x, y, Time.Ticks + 4000, owned, font);
    }

    public override void Update() {
        base.Update();
        TextRenderer.Update();
    }

    public override void Dispose() {
        // ClassicUO unlinks the renderer node. TextRenderer has no UnlinkD. Clear drops the lines.
        TextRenderer.Clear();
        base.Dispose();
    }

    public override bool Draw(int x, int y) {
        if (!base.Draw(x, y)) return false;
        TextRenderer.ProcessWorldText(true);
        TextRenderer.Draw(x, y, 0f, true);
        return true;
    }
}
