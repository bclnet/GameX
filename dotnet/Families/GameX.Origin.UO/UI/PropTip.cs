// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// Shows 0xD6 text while the pointer is on a world label. The tip does not take mouse hits.
/// </summary>
public static class PropTip {
    static Gump _tip;
    static uint _serial;

    public static void Poll(World world) {
        var label = UIManager.MouseOverControl as UseLabel;
        if (world == null || label == null || label.Serial == 0) {
            Hide();
            return;
        }
        if (!UOLoginNet.TryProperties(label.Serial, out var name, out var data)) {
            UOLoginNet.RequestProperties(label.Serial);
            Hide();
            return;
        }
        var text = string.IsNullOrEmpty(name) ? data : string.IsNullOrEmpty(data) ? name : name + "  " + data.Replace('\n', ' ');
        if (string.IsNullOrEmpty(text)) {
            Hide();
            return;
        }
        if (text.Length > 180) text = text.Substring(0, 180);
        if (_tip == null || _tip.IsDisposed || _serial != label.Serial) {
            Hide();
            _serial = label.Serial;
            _tip = new Gump(world, 0, 0) {
                CanMove = false,
                CanCloseWithRightClick = false,
                AcceptMouseInput = false,
                LayerOrder = UILayer.Over,
            };
            _tip.Add(new Label(text, false, 0x0035, 1) { X = 4, Y = 2, AcceptMouseInput = false });
            UIManager.Add(_tip);
        }
        _tip.X = UIManager.MouseX + 14;
        _tip.Y = UIManager.MouseY + 18;
    }

    static void Hide() {
        if (_tip != null && !_tip.IsDisposed) _tip.Dispose();
        _tip = null;
        _serial = 0;
    }
}
