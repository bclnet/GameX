// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Clients.UO;
using GameX.Origin.Netcode.UO;
using OpenX.Client;
using System;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// Overhead view after 0x1B. The player is a body frame plus the center label. Arrows are handled by LoginScene.
/// Felucca land and static art are drawn through GumpArt.Sink when the map is open.
/// A body uses ClassicUO's group: people stand on 4, walk on 0 or 1, run on 2 or 3. 0x6E replaces that group.
/// Enter sends 0xAD, or answers a prompt. Double-click sends 0x06, or 0x05 on another mobile while at war.
/// A drag lifts with 0x07. Tab sends 0x72. Hits sit on the coordinate line.
/// Stamps are multiplied by IsometricLight when personal is below overall. Weather.Draw paints the stored packet.
/// </summary>
public class WorldGump : Gump {
    const int Slots = 48;
    const int Radius = 10;
    const int Journal = 6;
    const int ViewW = 640;
    const int ViewH = 480;
    const int MaxWeather = 70;
    const float WeatherSim = 37f;
    const uint WeatherMs = 6 * 60 * 1000;
    readonly Label _coords;
    readonly UseLabel _pack;
    readonly UseLabel[] _rows;
    readonly Label[] _journal;
    readonly TextBox _say;
    readonly List<Stamp> _stamps = [];
    readonly List<(ushort graphic, sbyte z)> _statics = [];
    readonly List<UOEquipState> _worn = [];
    readonly List<UOSpeechState> _lines = [];
    readonly Dictionary<long, byte[]> _shadePx = [];
    readonly Dictionary<long, int> _shadeId = [];
    readonly Dictionary<int, byte[]> _dots = [];
    readonly Drop[] _drops = new Drop[MaxWeather];
    readonly Random _rng = new();
    int _rev = -1;
    int _shadeSeq;
    byte _wType, _wCount, _wNow, _wSeenType, _wSeenCount, _wSeenTemp, _wCur;
    sbyte _wWind;
    uint _wTimer, _wWindAt, _wLast;
    bool _wLit, _wSeen;

    struct Stamp {
        public int Key, X, Y, W, H, Ord;
        public byte[] Px;
    }

    struct Drop {
        public float SpeedX, SpeedY, X, Y, ScaleRatio, SpeedAngle, SpeedMagnitude;
        public uint ID;
    }

    public WorldGump(World world, LoginScene scene) : base(world, 0, 0) {
        CanCloseWithRightClick = false;
        CanCloseWithEsc = false;
        AcceptKeyboardInput = false;
        _coords = new Label("", false, 0x0386, 2) { X = 16, Y = 16 };
        Add(_coords);
        _pack = new UseLabel("backpack", 0x034F) { X = 16, Y = 36, IsVisible = false };
        Add(_pack);
        _rows = new UseLabel[Slots];
        for (var i = 0; i < Slots; i++) {
            _rows[i] = new UseLabel("", 0x0481) { X = 0, Y = 0, IsVisible = false };
            Add(_rows[i]);
        }
        _journal = new Label[Journal];
        for (var i = 0; i < Journal; i++) {
            _journal[i] = new Label("", false, 0x03B2, 3) { X = 16, Y = 360 + i * 14, IsVisible = false };
            Add(_journal[i]);
        }
        Add(new ResizePic(0x0BB8) { X = 16, Y = 448, Width = 420, Height = 24 });
        Add(_say = new TextBox { X = 22, Y = 450, Width = 400, Height = 20, Hue = 0x02B2, MaxLength = 100 });
        _say.SetKeyboardFocus();
        if (UIManager.GetGump<TopBarGump>() == null) UIManager.Add(new TopBarGump(World));
    }

    public override void Update() {
        while (UOLoginNet.TakeNewAnim(out var serial, out var body, out var type, out var action, out var mode)) {
            var assets = UltimaOnline.Current?.Assets;
            if (assets == null) continue;
            var group = assets.ObjectNewAnimation(body, type, action, mode);
            UOLoginNet.SetServerAnim(serial, group, (type == 1 || type == 2) && body == 0x0015);
        }
        while (UOLoginNet.TakeSpellbook(out var book)) {
            if (UIManager.GetGump<SpellbookGump>(book) == null) UIManager.Add(new SpellbookGump(book));
        }
        while (UOLoginNet.TakeVendor(out var vendor)) {
            if (UIManager.GetGump<VendorGump>(vendor) == null) UIManager.Add(new VendorGump(vendor));
        }
        while (UOLoginNet.TakePaperdoll(out var serial, out var title, out var flags)) {
            var open = UIManager.GetGump<PaperdollGump>(serial);
            if (open == null) UIManager.Add(new PaperdollGump(World, serial, title, flags));
            else open.Title = title;
        }
        while (UOLoginNet.TakeContainer(out var serial, out var gump)) {
            if (UIManager.GetGump<ContainerGump>(serial) == null)
                UIManager.Add(new ContainerGump(World, serial, gump));
        }
        while (UOLoginNet.TakeGump(out var ev)) {
            if (ev.Open) {
                if (ev.Gump == null) continue;
                foreach (var old in Match(ev.Gump.Sender, ev.Gump.Id, bySender: true)) old.Dispose();
                UIManager.Add(new ServerGump(World, ev.Gump));
            }
            else foreach (var old in Match(0, ev.Id, bySender: false)) old.ServerClose(ev.Button);
        }
        while (UOLoginNet.TakeMenu(out var menu))
            UIManager.Add(new MenuGump(World, menu));
        while (UOLoginNet.TakeText(out var entry))
            UIManager.Add(new TextEntryGump(World, entry));
        while (UOLoginNet.TakeShop(out var shop)) {
            foreach (var old in Shops(shop.Serial)) old.Dispose();
            if (shop.Open) UIManager.Add(new ShopGump(World, shop.Serial, shop.Buy));
        }
        while (UOLoginNet.TakeTrade(out var trade)) {
            foreach (var old in Trades(trade.Serial)) old.Dispose();
            if (trade.Open) UIManager.Add(new TradeGump(World, trade.Serial, trade.Name));
        }
        while (UOLoginNet.TakeDye(out var dye)) {
            var keep = false;
            foreach (var old in Dyes(dye.Serial))
                if (old.Graphic == dye.Graphic) keep = true;
                else old.Dispose();
            if (!keep) UIManager.Add(new DyeGump(World, dye.Serial, dye.Graphic));
        }
        var rev = UOLoginNet.WorldRevision;
        if (rev != _rev) {
            _rev = rev;
            Paint();
        }
        base.Update();
    }

    public override void OnKeyboardReturn(int textID, string text) {
        if (UOLoginNet.HasPrompt) {
            UOLoginNet.ReplyPrompt(text ?? "", false);
            _say.Text = "";
            return;
        }
        if (string.IsNullOrWhiteSpace(text)) return;
        UOLoginNet.Say(text.Trim());
        _say.Text = "";
    }

    public override void Dispose() {
        var extra = new List<Gump>();
        foreach (var g in UIManager.Gumps)
            if (g is PaperdollGump or ContainerGump or ServerGump or MenuGump or TextEntryGump or ShopGump or TradeGump or DyeGump) extra.Add(g);
        foreach (var g in extra) g.Dispose();
        base.Dispose();
    }

    /// <summary>Inverse of the diamond used to place labels. Z comes from StepHeight when the asset slot set it.</summary>
    public static void GroundTile(int sx, int sy, out int x, out int y, out int z) {
        var u = RoundDiv(sx - 320, 22);
        var v = RoundDiv(sy - 220, 22);
        var dx = RoundDiv(u + v, 2);
        var dy = RoundDiv(v - u, 2);
        x = UOLoginNet.PlayerX + dx;
        y = UOLoginNet.PlayerY + dy;
        var step = UOLoginNet.StepHeight;
        z = step != null ? step(x, y, UOLoginNet.PlayerZ) : UOLoginNet.PlayerZ;
    }

    static int RoundDiv(int n, int d) {
        if (n < 0) return -(((-n) + d / 2) / d);
        return (n + d / 2) / d;
    }

    public override bool Draw(int x, int y) {
        PaintGround(x, y);
        return base.Draw(x, y);
    }

    void PaintGround(int ox, int oy) {
        var sink = GumpArt.Sink;
        var assets = UltimaOnline.Current?.Assets;
        if (sink == null || assets == null) {
            MovingEffect.Draw(ox, oy);
            HealthBarGump.Draw(ox, oy);
            return;
        }
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        _stamps.Clear();
        for (var dy = -Radius; dy <= Radius; dy++)
            for (var dx = -Radius; dx <= Radius; dx++) {
                var wx = px + dx;
                var wy = py + dy;
                if (!assets.TryLandTile(wx, wy, out var tile, out var tz)) continue;
                if (!assets.TryArtPixels(tile, out var pix, out var w, out var h)) continue;
                var sx = 320 + (dx - dy) * 22;
                var sy = 220 + (dx + dy) * 22 - (tz - pz) * 4;
                _stamps.Add(new Stamp { Key = 0x10000 | tile, X = ox + sx - 22, Y = oy + sy - 22, W = w, H = h, Px = pix, Ord = (dx + dy + 32) * 1000 + tz });
                _statics.Clear();
                assets.CopyStatics(wx, wy, _statics);
                foreach (var (graphic, z) in _statics) {
                    var id = graphic + 0x4000;
                    if (id > 0xFFFF || !assets.TryArtPixels(id, out var ap, out var aw, out var ah)) continue;
                    var ssy = 220 + (dx + dy) * 22 - (z - pz) * 4;
                    _stamps.Add(new Stamp { Key = 0x20000 | id, X = ox + sx - aw / 2, Y = oy + ssy - ah + 22, W = aw, H = ah, Px = ap, Ord = (dx + dy + 32) * 1000 + z + 1 });
                }
            }
        AddBody(assets, ox, oy, UOLoginNet.PlayerSerial, UOLoginNet.PlayerBody, UOLoginNet.PlayerDirection, 0, 0, pz);
        foreach (var entry in World.Mobiles) {
            if (entry.Value is not Mobile m || m.Serial == UOLoginNet.PlayerSerial) continue;
            var dx = m.X - px;
            var dy = m.Y - py;
            if (dx > Radius || dx < -Radius || dy > Radius || dy < -Radius) continue;
            AddBody(assets, ox, oy, m.Serial, m.Body, m.Direction, dx, dy, m.Z);
        }
        foreach (var entry in World.Items) {
            if (entry.Value is not Item it) continue;
            var dx = it.X - px;
            var dy = it.Y - py;
            if (dx > Radius || dx < -Radius || dy > Radius || dy < -Radius) continue;
            var id = (it.Graphic & 0x7FFF) + 0x4000;
            if (id > 0xFFFF || !assets.TryArtPixels(id, out var ap, out var aw, out var ah)) continue;
            var sx = 320 + (dx - dy) * 22;
            var sy = 220 + (dx + dy) * 22 - (it.Z - pz) * 4;
            _stamps.Add(new Stamp { Key = 0x20000 | id, X = ox + sx - aw / 2, Y = oy + sy - ah + 22, W = aw, H = ah, Px = ap, Ord = (dx + dy + 32) * 1000 + it.Z + 1 });
        }
        _stamps.Sort((a, b) => a.Ord - b.Ord);
        var scale = LightScale();
        foreach (var s in _stamps) {
            var lit = Shaded(s.Key, s.Px, scale);
            sink.DrawRaw(ShadeKey(s.Key, scale), s.X, s.Y, s.W, s.H, lit, s.W, s.H);
        }
        MovingEffect.Draw(ox, oy);
        HealthBarGump.Draw(ox, oy);
        DrawWeather(sink, ox, oy, scale);
    }

    void AddBody(Assets assets, int ox, int oy, uint serial, int body, int direction, int dx, int dy, int z) {
        if (body <= 0 || assets == null) return;
        var dir = direction & 7;
        var moving = UOLoginNet.MobileMoving(serial, out var run);
        var war = serial == UOLoginNet.PlayerSerial && UOLoginNet.InWarMode;
        UOLoginNet.TryServerAnim(serial, out var fromServer, out var raw, out var frame);
        var action = assets.GroupFor((ushort)body, moving, run, war, TwoHanded(serial), fromServer, raw);
        if (!assets.TryFrameCount(body, action, dir, out var count)) {
            if (action == 0 || !assets.TryFrameCount(body, 0, dir, out count)) return;
            action = 0;
        }
        if (count <= 0) return;
        frame = (byte)(frame % count);
        if (!assets.TryBodyFrame(body, action, dir, frame, out var pix, out var w, out var h, out var cx, out var cy)) return;
        UOLoginNet.StepAnim(serial, count);
        var sx = 320 + (dx - dy) * 22;
        var sy = 220 + (dx + dy) * 22 - (z - UOLoginNet.PlayerZ) * 4;
        var key = 0x30000 + (body << 16) + ((action & 0x7F) << 9) + ((dir & 7) << 6) + (frame & 0x3F);
        _stamps.Add(new Stamp { Key = key, X = ox + sx - cx, Y = oy + sy - cy - h, W = w, H = h, Px = pix, Ord = (dx + dy + 32) * 1000 + z + 2 });
    }

    bool TwoHanded(uint serial) {
        UOLoginNet.CopyEquip(serial, _worn);
        foreach (var e in _worn)
            if (e.Layer == 0x02) return true;
        return false;
    }

    void Paint() {
        var mobiles = new List<UOMobileState>();
        var items = new List<UOItemState>();
        UOLoginNet.CopyActors(mobiles, items);
        World.Mobiles.Clear();
        World.Items.Clear();
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var dir = UOLoginNet.PlayerDirection & 7;
        var hp = "";
        if (UOLoginNet.TryStatus(UOLoginNet.PlayerSerial, out var st))
            hp = st.HasStats
                ? $"   HP {st.Hits}/{st.HitsMax}  Stam {st.Stam}/{st.StamMax}  Mana {st.Mana}/{st.ManaMax}"
                : $"   HP {st.Hits}/{st.HitsMax}";
        var mark = "";
        if (UOLoginNet.InWarMode) mark += "  war";
        if (UOLoginNet.HasTarget) mark += "  target";
        if (UOLoginNet.Holding) mark += "  hold";
        if (UOLoginNet.HasPrompt) mark += "  prompt";
        _coords.Text = $"X {px}  Y {py}  Z {pz}  body {UOLoginNet.PlayerBody}  {DirName((byte)dir)}{hp}  light {UOLoginNet.OverallLight}  season {UOLoginNet.Season}{mark}   arrows move, shift runs, enter says";
        var pack = UOLoginNet.PlayerBackpack;
        _pack.Serial = pack;
        _pack.IsVisible = pack != 0;
        var n = 0;
        var you = UOLoginNet.NameOf(UOLoginNet.PlayerSerial);
        Place(ref n, string.IsNullOrEmpty(you) ? "You" : you, 0x0035, 0, 0, 0, UOLoginNet.PlayerSerial);
        foreach (var m in mobiles) {
            World.Mobiles[m.Serial] = new Mobile(m.Serial, m.Body, m.X, m.Y, m.Z, m.Direction);
            if (m.Serial == UOLoginNet.PlayerSerial) continue;
            var name = UOLoginNet.NameOf(m.Serial);
            Place(ref n, string.IsNullOrEmpty(name) ? $"M {m.Body:X4}" : name, 0x0481, m.X - px, m.Y - py, m.Z - pz, m.Serial);
        }
        foreach (var it in items) {
            World.Items[it.Serial] = new Item(it.Serial, it.Graphic, it.X, it.Y, it.Z);
            Place(ref n, $"I {it.Graphic:X4}", 0x034F, it.X - px, it.Y - py, it.Z - pz, it.Serial);
        }
        for (; n < Slots; n++) _rows[n].IsVisible = false;
        UOLoginNet.CopySpeech(_lines);
        var start = _lines.Count > Journal ? _lines.Count - Journal : 0;
        var j = 0;
        for (var i = start; i < _lines.Count && j < Journal; i++, j++) {
            var line = _lines[i];
            var who = string.IsNullOrEmpty(line.Name) ? " " : line.Name;
            _journal[j].Text = $"{who}: {line.Text}";
            _journal[j].Hue = line.Hue == 0 ? (ushort)0x03B2 : line.Hue;
            _journal[j].IsVisible = true;
        }
        for (; j < Journal; j++) _journal[j].IsVisible = false;
    }

    void Place(ref int n, string text, ushort hue, int dx, int dy, int dz, uint serial) {
        if (n >= Slots) return;
        if (dx > 12 || dx < -12 || dy > 12 || dy < -12) return;
        var row = _rows[n++];
        row.Hue = hue;
        row.Text = text;
        row.Serial = serial;
        row.X = 320 + (dx - dy) * 22 - 20;
        row.Y = 220 + (dx + dy) * 22 - dz * 4;
        row.IsVisible = true;
    }

    static List<ShopGump> Shops(uint serial) {
        var hit = new List<ShopGump>();
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (n.Value is ShopGump g && !g.IsDisposed && g.LocalSerial == serial) hit.Add(g);
        return hit;
    }

    static List<TradeGump> Trades(uint serial) {
        var hit = new List<TradeGump>();
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (n.Value is TradeGump g && !g.IsDisposed && g.LocalSerial == serial) hit.Add(g);
        return hit;
    }

    static List<DyeGump> Dyes(uint serial) {
        var hit = new List<DyeGump>();
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (n.Value is DyeGump g && !g.IsDisposed && g.LocalSerial == serial) hit.Add(g);
        return hit;
    }

    static List<ServerGump> Match(uint sender, uint id, bool bySender) {
        var hit = new List<ServerGump>();
        for (var n = UIManager.Gumps.First; n != null; n = n.Next)
            if (n.Value is ServerGump g && !g.IsDisposed && g.IsFromServer && g.ServerSerial == id && (!bySender || g.LocalSerial == sender))
                hit.Add(g);
        return hit;
    }

    /// <summary>
    /// ClassicUO GameScene.UseLights, then IsometricLight.Recalculate.
    /// Personal at or above overall skips the darkness pass. Otherwise the multiply is max(personal, 32 - overall) / 32.
    /// </summary>
    static int LightScale() {
        var overall = UOLoginNet.OverallLight;
        var personal = UOLoginNet.PersonalLight;
        if (personal >= overall) return 32;
        var reverted = 32 - overall;
        return personal > reverted ? personal : reverted;
    }

    static long ShadeId(int key, int scale) => ((long)(uint)key << 6) | (uint)scale;

    byte[] Shaded(int key, byte[] src, int scale) {
        if (src == null || scale >= 32) return src;
        var id = ShadeId(key, scale);
        if (_shadePx.TryGetValue(id, out var buf)) return buf;
        buf = new byte[src.Length];
        for (var i = 0; i + 1 < src.Length; i += 2) {
            var c = src[i] | (src[i + 1] << 8);
            if ((c & 0x8000) != 0) {
                var r = ((c >> 10) & 31) * scale / 32;
                var g = ((c >> 5) & 31) * scale / 32;
                var b = (c & 31) * scale / 32;
                c = 0x8000 | (r << 10) | (g << 5) | b;
            }
            buf[i] = (byte)c;
            buf[i + 1] = (byte)(c >> 8);
        }
        _shadePx[id] = buf;
        return buf;
    }

    int ShadeKey(int key, int scale) {
        if (scale >= 32) return key;
        var id = ShadeId(key, scale);
        if (_shadeId.TryGetValue(id, out var k)) return k;
        k = 0x68000000 + _shadeSeq++;
        _shadeId[id] = k;
        return k;
    }

    byte[] Dot(int r, int g, int b, int scale) {
        if (scale < 32) {
            r = r * scale / 32;
            g = g * scale / 32;
            b = b * scale / 32;
        }
        var k = (r << 10) | (g << 5) | b;
        if (_dots.TryGetValue(k, out var buf)) return buf;
        var c = (ushort)(0x8000 | k);
        var lo = (byte)c;
        var hi = (byte)(c >> 8);
        buf = [lo, hi, lo, hi, lo, hi, lo, hi];
        _dots[k] = buf;
        return buf;
    }

    void PlotLine(IGumpArt sink, float x1, float y1, float x2, float y2, int scale) {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var steps = (int)Math.Sqrt(dx * dx + dy * dy);
        if (steps < 1) return;
        var px = Dot(0, 0, 31, scale);
        var key = 0x67000000 | (scale << 16) | 31;
        for (var i = 0; i <= steps; i++) {
            var t = i / (float)steps;
            sink.DrawRaw(key, (int)(x1 + dx * t), (int)(y1 + dy * t), 2, 2, px, 2, 2);
        }
    }

    /// <summary>ClassicUO Weather.SinOscillate. Sign of the radian, not sine.</summary>
    static float SinOscillate(float freq, int range, uint tick) {
        var anglef = (int)(tick / 2.7777f * freq) % 360;
        return Math.Sign(anglef * (Math.PI / 180.0)) * range;
    }

    void DrawWeather(IGumpArt sink, int ox, int oy, int scale) {
        var type = UOLoginNet.WeatherType;
        var count = UOLoginNet.WeatherCount;
        var temp = UOLoginNet.WeatherTemp;
        if (!(_wLit && _wCur == type) && !(_wSeen && _wSeenType == type && _wSeenCount == count && _wSeenTemp == temp)) {
            _wSeen = true;
            _wSeenType = type;
            _wSeenCount = count;
            _wSeenTemp = temp;
            SpawnWeather(type, count, temp);
        }
        var tick = GlobalTime.Ticks;
        var remove = false;
        if (_wTimer < tick) {
            if (_wNow == 0) return;
            remove = true;
        }
        else if (_wType == 0xFE || _wType == 0xFF) return;
        var passed = tick - _wLast;
        if (passed > 7000) {
            _wLast = tick;
            passed = 25;
        }
        var windChanged = false;
        if (_wWindAt < tick) {
            if (_wWindAt == 0) windChanged = true;
            _wWindAt = tick + (uint)(_rng.Next(13, 20) * 1000);
            var last = _wWind;
            _wWind = (sbyte)_rng.Next(0, 5);
            if (_rng.Next(0, 3) != 0) _wWind *= -1;
            if (_wWind < 0 && last > 0) _wWind = 0;
            else if (_wWind > 0 && last < 0) _wWind = 0;
            if (last != _wWind) windChanged = true;
        }
        var speedOffset = passed / WeatherSim;
        for (var i = 0; i < _wNow; i++) {
            ref var e = ref _drops[i];
            if (e.X < 0 || e.X > ViewW || e.Y < 0 || e.Y > ViewH) {
                if (remove) {
                    if (_wNow > 0) _wNow--;
                    else _wNow = 0;
                    continue;
                }
                e.X = _rng.Next(0, ViewW + 1);
                e.Y = _rng.Next(0, ViewH + 1);
            }
            switch (_wType) {
                case 0:
                    e.SpeedX = -4.5f - e.ScaleRatio;
                    e.SpeedY = 5f + e.ScaleRatio;
                    break;
                case 3:
                    e.SpeedX = _wWind * 1.5f;
                    e.SpeedY = 1.5f;
                    break;
                case 2:
                case 1: {
                    if (_wType == 2) { e.SpeedX = _wWind; e.SpeedY = 1f; }
                    else { e.SpeedX = _wWind; e.SpeedY = 6f; }
                    if (windChanged) {
                        e.SpeedAngle = (float)(Math.Atan2(e.SpeedX, e.SpeedY) * 180.0 / Math.PI);
                        e.SpeedMagnitude = (float)Math.Sqrt(e.SpeedX * e.SpeedX + e.SpeedY * e.SpeedY);
                    }
                    var speedAngle = e.SpeedAngle;
                    var speedMagnitude = e.SpeedMagnitude + e.ScaleRatio;
                    speedAngle += SinOscillate(0.4f, 20, tick + e.ID);
                    var rad = speedAngle * ((float)Math.PI / 180f);
                    e.SpeedX = speedMagnitude * (float)Math.Sin(rad);
                    e.SpeedY = speedMagnitude * (float)Math.Cos(rad);
                    break;
                }
            }
            switch (_wType) {
                case 0:
                case 1: {
                    var oldX = (int)e.X;
                    var oldY = (int)e.Y;
                    var ofsx = e.SpeedX * speedOffset;
                    var ofsy = e.SpeedY * speedOffset;
                    e.X += ofsx;
                    e.Y += ofsy;
                    const float maxOff = 5f;
                    if (ofsx >= maxOff) oldX = (int)(e.X - maxOff);
                    else if (ofsx <= -maxOff) oldX = (int)(e.X + maxOff);
                    if (ofsy >= maxOff) oldY = (int)(e.Y - maxOff);
                    else if (oldY <= -maxOff) oldY = (int)(e.Y + maxOff);
                    PlotLine(sink, ox + oldX, oy + oldY, ox + e.X, oy + e.Y, scale);
                    break;
                }
                case 2:
                    e.X += e.SpeedX * speedOffset;
                    e.Y += e.SpeedY * speedOffset;
                    sink.DrawRaw(0x67000000 | (scale << 16) | 0x7FFF, ox + (int)e.X, oy + (int)e.Y, 2, 2, Dot(31, 31, 31, scale), 2, 2);
                    break;
            }
        }
        _wLast = tick;
    }

    /// <summary>ClassicUO Weather.Generate. Temperature is stored there; the draw color switch is commented out.</summary>
    void SpawnWeather(byte type, byte count, byte temp) {
        _ = temp;
        if (_wLit && _wCur == type) return;
        _wType = 0;
        _wCount = _wNow = 0;
        _wWind = 0;
        _wWindAt = _wTimer = 0;
        _wLit = false;
        _wType = type;
        _wCount = (byte)Math.Min(MaxWeather, (int)count);
        _wTimer = GlobalTime.Ticks + WeatherMs;
        _wLast = GlobalTime.Ticks;
        if (type == 0xFE || type == 0xFF) {
            _wTimer = 0;
            _wLit = false;
            return;
        }
        var show = _wCount > 0;
        switch (type) {
            case 0:
            case 1:
            case 2:
            case 3:
                if (show) { _wLit = true; _wCur = type; }
                break;
        }
        _wWindAt = 0;
        while (_wNow < _wCount) {
            ref var e = ref _drops[_wNow++];
            e = default;
            e.X = _rng.Next(0, ViewW + 1);
            e.Y = _rng.Next(0, ViewH + 1);
        }
    }

    static string DirName(byte d) => d switch {
        0 => "N",
        1 => "NE",
        2 => "E",
        3 => "SE",
        4 => "S",
        5 => "SW",
        6 => "W",
        _ => "NW",
    };
}
