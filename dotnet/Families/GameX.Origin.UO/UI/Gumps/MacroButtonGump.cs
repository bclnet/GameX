// SPDX-License-Identifier: BSD-2-Clause

using System;
using System.Xml;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO MacroButtonGump. The face is 88 by 44. The name is hue 0x03B2, and 53 while the pointer is over it.
/// Gump has no GumpType property. This gump stores GumpType.MacroButton.
/// World has no Macros. The button keeps one MacroManager and ticks it while the button is open.
/// A left double-click runs the macro. CastSpellsByOneClick defaults to false, so one click does not.
/// Profile path is not ported. Save and Restore do not open a profile file.
/// </summary>
public class MacroButtonGump : AnchorableGump {
    const int IdleKey = 0x57400002;
    const int HotKey = 0x57400003;
    const int FrameKey = 0x57400004;

    // Profile path is not ported. CastSpellsByOneClick defaults to false.
    static readonly bool CastSpellsByOneClick = false;

    // r = g = b = 30, and DimGray 105. The frame is Gray 128.
    static readonly byte[] IdlePx = { 0x63, 0x8C };
    static readonly byte[] HotPx = { 0xAD, 0xB5 };
    static readonly byte[] GrayPx = { 0x10, 0xC2 };

    static MacroManager _manager;

    Label _label = null;
    public Macro _macro = null;

    public MacroButtonGump(World world, Macro macro, int x, int y) : this(world) {
        X = x;
        Y = y;
        _macro = macro;
        BuildGump();
    }

    public MacroButtonGump(World world) : base(world, 0, 0) {
        GumpType = GameX.Origin.UO.GumpType.MacroButton;
        CanMove = true;
        AcceptMouseInput = true;
        CanCloseWithRightClick = true;
        WantUpdateSize = false;
        WidthMultiplier = 2;
        HeightMultiplier = 1;
        GroupMatrixWidth = 44;
        GroupMatrixHeight = 44;
        AnchorType = ANCHOR_TYPE.SPELL;
    }

    public GumpType GumpType { get; }

    public override void Update() {
        if (_label != null && !_label.IsDisposed)
            _label.Hue = Over(this) ? (ushort)53 : (ushort)0x03B2;
        if (_macro != null) Manager(World).Update();
        base.Update();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        var sink = GumpArt.Sink;
        if (sink != null && Width > 0 && Height > 0) {
            var hot = Over(this);
            sink.DrawRaw(hot ? HotKey : IdleKey, x, y, Width, Height, hot ? HotPx : IdlePx, 1, 1);
            Frame(sink, x, y, Width, Height);
        }
        return base.Draw(x, y);
    }

    public override void OnMouseClick(int x, int y) {
        base.OnMouseClick(x, y);
        if (!CastSpellsByOneClick || AltDown()) return;
        var off = Mouse.Position - Mouse.LClickPosition;
        if (Math.Abs(off.X) < 5 && Math.Abs(off.Y) < 5) RunMacro();
    }

    public override bool OnMouseDoubleClick(int x, int y) {
        if (CastSpellsByOneClick) return false;
        RunMacro();
        return true;
    }

    public void Save(XmlTextWriter writer) {
        // Profile path is not ported.
        if (writer == null || _macro == null) return;
        var macroid = Manager(World).GetAllMacros().IndexOf(_macro);
        LocalSerial = (uint)macroid + 1000;
        writer.WriteAttributeString("x", X.ToString());
        writer.WriteAttributeString("y", Y.ToString());
        writer.WriteAttributeString("serial", LocalSerial.ToString());
        writer.WriteAttributeString("name", _macro.Name);
    }

    public void Restore(XmlElement xml) {
        // Profile path is not ported.
        if (xml == null) return;
        if (int.TryParse(xml.GetAttribute("x"), out var x)) X = x;
        if (int.TryParse(xml.GetAttribute("y"), out var y)) Y = y;
        var macro = Manager(World).FindMacro(xml.GetAttribute("name"));
        if (macro == null) return;
        _macro = macro;
        BuildGump();
    }

    void BuildGump() {
        if (_macro == null) return;
        Width = 88;
        Height = 44;
        if (_label != null) _label.Dispose();
        // ClassicUO font 255, black border, and center align. Label has none of those. The line is centered by hand.
        _label = new Label(_macro.Name, true, 0x03B2, 1);
        _label.Y = (Height >> 1) - (_label.Height >> 1);
        var lx = (Width - _label.Width) >> 1;
        _label.X = lx < 0 ? 0 : lx;
        Add(_label);
        Sync();
    }

    void Sync() {
        Anchors.SetAnchorId(this, (int)AnchorType, WidthMultiplier, HeightMultiplier, GroupMatrixWidth, GroupMatrixHeight);
    }

    void RunMacro() {
        if (_macro == null) return;
        var macros = Manager(World);
        macros.SetMacroToExecute(_macro.Items as MacroObject);
        macros.WaitForTargetTimer = 0;
        macros.Update();
    }

    static MacroManager Manager(World world) {
        if (_manager == null) _manager = new MacroManager(world);
        return _manager;
    }

    // ClassicUO Keyboard.Alt. Mouse has no key state.
    static bool AltDown() => false;

    static void Frame(IGumpArt sink, int x, int y, int w, int h) {
        sink.DrawRaw(FrameKey, x, y, w, 1, GrayPx, 1, 1);
        if (h > 1) sink.DrawRaw(FrameKey, x, y + h - 1, w, 1, GrayPx, 1, 1);
        sink.DrawRaw(FrameKey, x, y, 1, h, GrayPx, 1, 1);
        if (w > 1) sink.DrawRaw(FrameKey, x + w - 1, y, 1, h, GrayPx, 1, 1);
    }
}
