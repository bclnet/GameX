// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO RadioButton. Checking one clears the others in the same parent and group.
/// Checkbox has no max width, so that argument is kept and not cropped.
/// </summary>
public class RadioButton : Checkbox {
    public RadioButton(int group, List<string> parts, string[] lines) : this(group, ushort.Parse(parts[3]), ushort.Parse(parts[4])) {
        X = int.Parse(parts[1]);
        Y = int.Parse(parts[2]);
        IsChecked = parts[5] == "1";
        LocalSerial = SerialHelper.Parse(parts[6]);
        IsFromServer = true;
    }

    public RadioButton(int group, ushort inactive, ushort active, string text = "", byte font = 0, ushort color = 0, bool isunicode = true, int maxWidth = 0)
        : base(inactive, active, text, font, color, isunicode) {
        GroupIndex = group;
        MaxWidth = maxWidth;
        ValueChanged += (_, _) => OnCheckedChanged();
    }

    public int GroupIndex { get; set; }
    public int MaxWidth { get; }
    public bool IsFromServer { get; set; }

    protected void OnCheckedChanged() {
        if (IsChecked) HandleClick();
    }

    bool HandleClick() {
        var parent = Parent;
        if (parent == null) return false;
        var children = parent.Children;
        for (var i = 0; i < children.Count; i++) {
            if (children[i] is not RadioButton button || button == this || button.IsDisposed) continue;
            if (button.GroupIndex == GroupIndex) button.IsChecked = false;
        }
        return true;
    }
}
