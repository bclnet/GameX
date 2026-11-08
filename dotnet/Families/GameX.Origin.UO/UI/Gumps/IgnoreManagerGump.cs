// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Session ignore list. The names stay on IgnoreManager until the process exits.
/// Nothing is written to a profile or a settings file.
/// An empty or whitespace name is not added. A name already stored is not added again.
/// A different case is a different entry. The stored key is the trimmed name.
/// Each row is that name and a remove button. Toggle closes the open gump, or adds one.
/// </summary>
public sealed class IgnoreManagerGump : Gump {
    const int AddId = 1;
    const int RemoveBase = 100;
    const int RowStep = 25;

    TextBox _box;
    string _draft = "";
    string[] _shown = new string[0];

    public IgnoreManagerGump(World world) : base(world, 0, 0) {
        X = 100;
        Y = 80;
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        Build();
    }

    /// <summary>False for a null or empty name. Otherwise Contains with the default comparer, which is ordinal and case-sensitive.</summary>
    public static bool Ignored(string name) {
        if (string.IsNullOrEmpty(name)) return false;
        return IgnoreManager.Instance.IgnoredCharsList.Contains(name);
    }

    /// <summary>One gump. A second call disposes it and does not add another.</summary>
    public static void Toggle(World world) {
        var open = UIManager.GetGump<IgnoreManagerGump>();
        if (open != null) {
            open.Dispose();
            return;
        }
        UIManager.Add(new IgnoreManagerGump(world));
    }

    public override void OnButtonClick(int buttonID) {
        if (buttonID == AddId) {
            AddName();
            return;
        }
        var index = buttonID - RemoveBase;
        if (index < 0 || index >= _shown.Length) return;
        IgnoreManager.Instance.RemoveIgnoredTarget(_shown[index]);
        Build();
    }

    public override void OnKeyboardReturn(int textID, string text) => AddName();

    void AddName() {
        var text = _box != null && !_box.IsDisposed ? _box.Text : _draft;
        if (!IgnoreManager.Instance.AddIgnoredTarget(text)) return;
        _draft = "";
        if (_box != null && !_box.IsDisposed) _box.Text = "";
        Build();
    }

    void Build() {
        _draft = _box != null && !_box.IsDisposed ? _box.Text : _draft;
        ClearChildren();
        _box = null;
        var names = IgnoreManager.Instance.IgnoredCharsList;
        _shown = new string[names.Count];
        names.CopyTo(_shown);
        var height = 96 + _shown.Length * RowStep + 12;
        if (height < 400) height = 400;
        Width = 300;
        Height = height;
        WantUpdateSize = false;
        Keep(new ResizePic(0x0A28) { X = 0, Y = 0, Width = 300, Height = height });
        Keep(new Label("Ignore List", false, 0x0386, 1) { X = 16, Y = 12 });
        Keep(new ResizePic(0x0BB8) { X = 16, Y = 34, Width = 268, Height = 22 });
        _box = new TextBox { X = 20, Y = 36, Width = 260, Height = 18, Hue = 0x0481, MaxLength = 100 };
        _box.Text = _draft ?? "";
        Keep(_box);
        Keep(new Button(AddId, 0x0FA8, 0x0FAA, 0x0FA9) { X = 16, Y = 62, ButtonAction = ButtonAction.Activate });
        Keep(new Label("Add", false, 0x0386, 1) { X = 44, Y = 64 });
        var y = 96;
        for (var i = 0; i < _shown.Length; i++) {
            Keep(new Button(RemoveBase + i, 0x0FB1, 0x0FB3, 0x0FB2) { X = 16, Y = y, ButtonAction = ButtonAction.Activate });
            Keep(new Label(_shown[i], false, 0x0481, 1) { X = 44, Y = y + 2 });
            y += RowStep;
        }
        if (_box != null && !_box.IsDisposed) _box.SetKeyboardFocus();
    }

    void Keep(Control c) {
        if (c != null && !c.IsDisposed) Add(c);
    }

    void ClearChildren() {
        while (Children.Count > 0) {
            var c = Children[Children.Count - 1];
            c.Parent = null;
            c.Dispose();
        }
    }
}
