// SPDX-License-Identifier: BSD-2-Clause

using System;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO Checkbox. The box size is the inactive gump.
/// </summary>
public class Checkbox : Control {
    readonly ushort _inactive, _active;

    public Checkbox(ushort inactive, ushort active, string text = "", byte font = 0, ushort color = 0, bool isunicode = true) {
        _inactive = inactive;
        _active = active;
        Text = text ?? "";
        Font = font;
        Hue = color;
        IsUnicode = isunicode;
        if (!GumpMetrics.TrySize(inactive, out var w, out var h) || !GumpMetrics.TrySize(active, out _, out _)) {
            Dispose();
            return;
        }
        Width = w + (GumpArt.Sink?.MeasureText(Text, Font) ?? Text.Length * 6);
        Height = h > 16 ? h : 16;
        CanMove = false;
        AcceptMouseInput = true;
    }

    public string Text { get; }
    public byte Font { get; }
    public ushort Hue { get; }
    public bool IsUnicode { get; }
    public ushort InactiveGraphic => _inactive;
    public ushort ActiveGraphic => _active;
    public override ClickPriority Priority => ClickPriority.High;

    public bool IsChecked {
        get => _isChecked;
        set {
            if (_isChecked == value) return;
            _isChecked = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    bool _isChecked;

    public event EventHandler ValueChanged;

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        GumpArtDraw.Pic(IsChecked ? _active : _inactive, x, y, 0, 0, false);
        if (!string.IsNullOrEmpty(Text)) {
            var textW = GumpArt.Sink?.MeasureText(Text, Font) ?? Text.Length * 6;
            GumpArt.Sink?.DrawText(x + Math.Max(0, Width - textW), y, Text, Hue, Font);
        }
        return base.Draw(x, y);
    }
}
