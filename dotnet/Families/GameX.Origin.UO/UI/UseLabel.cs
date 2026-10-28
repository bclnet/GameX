// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>A label whose click answers 0x6C and whose double-click sends 0x06, or 0x05 in war mode.</summary>
public sealed class UseLabel : Label {
    public UseLabel(string text, ushort hue, byte font = 5) : base(text, false, hue, font) {
        AcceptMouseInput = true;
    }

    public uint Serial { get; set; }

    public override void OnMouseClick(int x, int y) {
        if (Serial == 0) return;
        if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(Serial);
        else UOLoginNet.RequestPopup(Serial);
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        if (Serial == 0) return true;
        if (UOLoginNet.HasTarget || UOLoginNet.TargetedJustNow) {
            if (UOLoginNet.HasTarget) UOLoginNet.TargetSerial(Serial);
            return true;
        }
        if (Serial != UOLoginNet.PlayerSerial && UOLoginNet.IsMobile(Serial) && UOLoginNet.InWarMode)
            UOLoginNet.Attack(Serial);
        else UOLoginNet.Use(Serial);
        return true;
    }
}
