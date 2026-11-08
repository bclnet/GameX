// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO InfoBarGump default rows: name, hits, mana, stamina, then weight.
/// Weight, gold, luck, resists, damage, and followers are skipped. UOLoginNet has no getter for them.
/// Str, dex, and int are shown because UOStatusState exposes them. Mana, stamina, and those three
/// stay hidden unless HasStats, so a missing stat block is not drawn as zero. Hits need TryStatus.
/// The name hue is Notoriety.GetHue at the profile defaults. Meter hues are InfoBarHighlightType 0.
/// BGRA1555 has no partial alpha, so the 0.7 black fill is an opaque dark pixel. Right-click closes.
/// </summary>
public sealed class InfoBarGump : Gump {
    const int FillKey = 0x57200001;
    const ushort DataHue = 0x0481;

    // r = g = b = 4, opaque. Stand-in for AlphaBlendControl(0.7).
    static readonly byte[] Dark = { 0x84, 0x90 };

    readonly StatRow[] _rows;

    public InfoBarGump(World world) : base(world, 0, 0) {
        CanMove = true;
        CanCloseWithRightClick = true;
        AcceptMouseInput = true;
        WantUpdateSize = false;
        X = 300;
        Y = 300;
        Width = 40;
        Height = 20;
        _rows = new[] {
            new StatRow(Stat.Name, "", 0x3D2),
            new StatRow(Stat.Hits, "Hits", 0x1B6),
            new StatRow(Stat.Mana, "Mana", 0x1ED),
            new StatRow(Stat.Stam, "Stam", 0x22E),
            new StatRow(Stat.Str, "Str", 0x3D2),
            new StatRow(Stat.Dex, "Dex", 0x3D2),
            new StatRow(Stat.Int, "Int", 0x3D2),
        };
        for (var i = 0; i < _rows.Length; i++) Add(_rows[i]);
    }

    /// <summary>One gump. A second call disposes it and does not add another. World may be null.</summary>
    public static void Toggle(World world) {
        var open = UIManager.GetGump<InfoBarGump>();
        if (open != null) {
            open.Dispose();
            return;
        }
        UIManager.Add(new InfoBarGump(world));
    }

    public override void Update() {
        Apply();
        base.Update();
        Place();
    }

    public override bool Draw(int x, int y) {
        if (IsDisposed || !IsVisible) return false;
        if (Width > 0 && Height > 0)
            GumpArt.Sink?.DrawRaw(FillKey, x, y, Width, Height, Dark, 1, 1);
        return base.Draw(x, y);
    }

    void Apply() {
        var serial = UOLoginNet.PlayerSerial;
        var found = UOLoginNet.TryStatus(serial, out var st);
        var extra = found && st.HasStats;
        var name = found && !string.IsNullOrEmpty(st.Name) ? st.Name : UOLoginNet.NameOf(serial);
        for (var i = 0; i < _rows.Length; i++) {
            var row = _rows[i];
            switch (row.Kind) {
                case Stat.Name:
                    row.Show(name, NotorietyHue(UOLoginNet.Notoriety));
                    break;
                case Stat.Hits:
                    if (found) row.Show($"{st.Hits}/{st.HitsMax}", MeterHue(st.Hits, st.HitsMax));
                    else row.IsVisible = false;
                    break;
                case Stat.Mana:
                    if (extra) row.Show($"{st.Mana}/{st.ManaMax}", MeterHue(st.Mana, st.ManaMax));
                    else row.IsVisible = false;
                    break;
                case Stat.Stam:
                    if (extra) row.Show($"{st.Stam}/{st.StamMax}", MeterHue(st.Stam, st.StamMax));
                    else row.IsVisible = false;
                    break;
                case Stat.Str:
                    if (extra) row.Show(st.Str.ToString(), DataHue);
                    else row.IsVisible = false;
                    break;
                case Stat.Dex:
                    if (extra) row.Show(st.Dex.ToString(), DataHue);
                    else row.IsVisible = false;
                    break;
                case Stat.Int:
                    if (extra) row.Show(st.Int.ToString(), DataHue);
                    else row.IsVisible = false;
                    break;
            }
        }
    }

    void Place() {
        var x = 5;
        var n = 0;
        for (var i = 0; i < _rows.Length; i++) {
            var row = _rows[i];
            if (!row.IsVisible) continue;
            if (n > 0) x += 5;
            row.X = x;
            row.Y = 2;
            x += row.Width;
            n++;
        }
        Width = n == 0 ? 40 : x + 5;
        if (Width < 40) Width = 40;
        Height = 20;
    }

    /// <summary>ClassicUO Notoriety.GetHue. Values are the profile defaults. Invulnerable is 0x0034.</summary>
    static ushort NotorietyHue(byte flag) {
        switch (flag) {
            case 1: return 0x005A;
            case 2: return 0x0044;
            case 3: return 0x03B2;
            case 4: return 0x03B2;
            case 5: return 0x0031;
            case 6: return 0x0023;
            case 7: return 0x0034;
            default: return 0;
        }
    }

    static ushort MeterHue(int cur, int max) {
        if (max <= 0) return DataHue;
        var percent = cur / (float)max;
        if (percent <= 0.25f) return 0x0021;
        if (percent <= 0.5f) return 0x0030;
        if (percent <= 0.75f) return 0x0035;
        return DataHue;
    }

    enum Stat { Name, Hits, Mana, Stam, Str, Dex, Int }

    sealed class StatRow : Control {
        public readonly Stat Kind;
        public readonly Label Caption;
        public readonly Label Value;

        public StatRow(Stat kind, string caption, ushort hue) {
            Kind = kind;
            AcceptMouseInput = false;
            CanMove = false;
            WantUpdateSize = false;
            Height = 16;
            Caption = new Label(caption, true, hue, 1);
            Value = new Label("", true, DataHue, 1);
            Add(Caption);
            Add(Value);
        }

        public void Show(string text, ushort hue) {
            IsVisible = true;
            Value.X = Caption.Width;
            Value.Hue = hue;
            Value.Text = text ?? "";
            Width = Value.X + Value.Width;
        }
    }
}
