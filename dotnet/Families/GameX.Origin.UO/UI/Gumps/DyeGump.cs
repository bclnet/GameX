// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>0x95. OK sends the selected cell hue. Close and right-click dispose the gump and do not dye.</summary>
public sealed class DyeGump : ColorPickerGump {
    public DyeGump(World world, uint serial, ushort graphic) : base(world, serial, graphic, 200, 250) {
        CanCloseWithRightClick = true;
    }

    protected override void OnOk(ushort hue) => UOLoginNet.ReplyDye(LocalSerial, hue);
}
