using GameX;
using GameX.Origin.Formats;
using GameX.Origin.Formats.UO;
using GameX.Xbox;
using OpenX.Client;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace GameX.Origin.Clients.UO;

public class Assets(UOGame game) {
    bool Uop = game.Uop;
    UoAnimations Animations;
    Binary_Animdata AnimData;
    Archive Arts;
    //Maps Maps;
    Binary_StringTable Clilocs;
    Archive Gumps;
    Binary_GumpDef GumpsDef;
    Binary_AsciiFont Fonts;
    Binary_Hues Hues;
    Binary_RadarColor HuesRadar;
    //TileData TileData;
    //Multi Multis;
    //Skills Skills;
    //Texmaps Texmaps;
    //Speeches Speeches;
    //Lights Lights;
    //Sounds Sounds;
    //MultiMap MultiMaps;
    //Verdata Verdata;
    //Professions Professions;
    //TileArt TileArt;
    //StringDictionary StringDictionary;
    //// statics
    //StaticFilters StaticFilters;
    //BuffTable BuffTable;
    //ChairTable ChairTable;
    readonly Dictionary<int, (byte[] pixels, int width, int height)> _gumpPixels = [];
    readonly HashSet<int> _gumpPixelsMiss = [];
    Stream _map;
    int _mapWidth;
    Stream _staidx;
    Stream _statics;
    readonly Dictionary<long, byte[]> _landBlocks = [];
    readonly Dictionary<long, byte[]> _staticBlocks = [];
    readonly Dictionary<int, (byte[] pixels, int width, int height)> _artPixels = [];
    readonly HashSet<int> _artMiss = [];
    readonly Dictionary<long, (byte[] pixels, int width, int height, int cx, int cy)> _bodyFrames = [];
    readonly HashSet<long> _bodyMiss = [];
    readonly object _mapGate = new();
    Archive _game;
    Binary_TileData _tile;
    int _blockHeight = 512;
    int _facet;


    public class UoAnimations {
        public Archive[] Anims = new Archive[10];

        public async Task<UoAnimations> Load(bool uop, Archive game, ClientVersion version) {
            for (var i = 0; i < 10; i++) {
                var name = uop ? $"AnimationFrame{i + 1}.uop" : $"anim{(i == 0 ? "" : (i + 1).ToString())}.idx";
                Anims[i] = Assets.OpenPack(game, name);
            }
            // A missing file comes back as the path string. GetAsset then throws InvalidCastException.
            if (uop & version > ClientVersion.CV_60144)
                await Assets.TryAsset<Binary_Animdata>(game, "AnimationSequence.uop");
            if (version >= ClientVersion.CV_500A)
                await Assets.TryAsset<Binary_MobType>(game, "mobtypes.txt");
            await Assets.TryAsset<Binary_Animdef>(game, "Anim1.def");
            await Assets.TryAsset<Binary_Animdef>(game, "Anim2.def");
            if (version >= ClientVersion.CV_300) {
                await Assets.TryAsset<Binary_Animdata>(game, "Equipconv.def");
                await Assets.TryAsset<Binary_Animdata>(game, "Bodyconv.def");
                await Assets.TryAsset<Binary_Animdata>(game, "Body.def");
                await Assets.TryAsset<Binary_Animdata>(game, "Corpse.def");
            }
            return this;
        }
    }

    /// <summary>
    /// Missing archive entries are returned as the path string, and GetAsset casts that string to T.
    /// A file can also be present and still unreadable. Login only needs gumps, so that one stays fatal.
    /// </summary>
    static async Task<T> TryAsset<T>(Archive game, string name, bool required = false) where T : class {
        try { return await game.GetAsset<T>(name, throwOnError: false); }
        catch (Exception) {
            if (required) throw;
            return null;
        }
    }

    /// <summary>
    /// A pack file is a child archive. GetAsset&lt;Archive&gt; opens it and then casts the path string.
    /// GetSource already returns that opened archive when the name has no ':' entry.
    /// </summary>
    static Archive OpenPack(Archive game, string name) {
        var (arc, file) = game.GetSource(name, throwOnError: false);
        if (file?.Arc != null) return file.Arc.Open();
        if (file == null && arc != null && !ReferenceEquals(arc, game)) return arc;
        return null;
    }

    public async Task Load(Archive game, UOGameClient ctx) {
        var version = ((UOGame)game.Game).Version;
        var lang = "enu";
        Animations = await new UoAnimations().Load(Uop, game, version);
        AnimData = await TryAsset<Binary_Animdata>(game, "animdata.mul");
        Arts = OpenPack(game, Uop ? "artLegacyMUL.uop" : "artidx.mul");
        //Maps = new Maps(this);
        Clilocs = await TryAsset<Binary_StringTable>(game, $"cliloc.{lang}");
        Gumps = OpenPack(game, Uop ? "gumpartLegacyMUL.uop" : "gumpidx.mul")
            ?? throw new FileNotFoundException("UO gump archive was not opened.", Uop ? "gumpartLegacyMUL.uop" : "gumpidx.mul");
        GumpsDef = await TryAsset<Binary_GumpDef>(game, "gump.def");
        Fonts = await TryAsset<Binary_AsciiFont>(game, "fonts.mul");
        //Fonts = await game.GetAsset<Binary_UnicodeFont>("fonts.mul");
        Hues = await TryAsset<Binary_Hues>(game, "hues.mul");
        HuesRadar = await TryAsset<Binary_RadarColor>(game, "radarcol.mul");
        _game = game;
        _tile = await TryAsset<Binary_TileData>(game, "tiledata.mul");
        await OpenFelucca(game);
        //TileData = new TileDataoader(this);
        //Multis = new Multi(this);
        //Skills = new Skills(this);
        //Texmaps = new Texmaps(this);
        //Speeches = new Speeches(this);
        //Lights = new Lights(this);
        //Sounds = new Sounds(this);
        //MultiMaps = new MultiMap(this);
        //Verdata = new Verdata(this);
        //Professions = new Profession(this);
        //TileArt = new TileArt(this);
        //StringDictionary = new StringDictionary(this);
        //// statics
        //StaticFilters.Load(TileData);
        //BuffTable.Load();
        //ChairTable.Load();
        ////UltimaLive.Enable();
    }

    public bool TryAsciiGlyph(int fontIndex, int ch, out int width, out int height, out byte[] pixels1555) {
        if (Fonts == null) { width = height = 0; pixels1555 = null; return false; }
        return Fonts.TryGlyph(fontIndex, ch, out width, out height, out pixels1555);
    }

    public bool TryHueRamp(int hueIndex, out ushort[] colors) {
        if (Hues == null) { colors = null; return false; }
        return Hues.TryRamp(hueIndex, out colors);
    }

    /// <summary>
    /// Width and height of a gump. Flag-3 entries keep that size in the payload, so this reads pixels once and caches them.
    /// </summary>
    public bool TryGumpSize(int graphic, out int width, out int height) {
        // Flag-3 gumps store width and height inside the payload, so the index extra is empty until the first read.
        if (TryGumpPixels(graphic, out _, out width, out height)) return true;
        width = height = 0;
        if (Gumps is not BinaryArchive bin || bin.Files == null || graphic < 0) return false;
        IList<FileSource> files = bin.Files;
        FileSource file = null;
        if (graphic < files.Count) {
            var at = files[graphic];
            if (at != null && at.Id == graphic) file = at;
        }
        if (file == null)
            for (var i = 0; i < files.Count; i++) {
                var at = files[i];
                if (at != null && at.Id == graphic) { file = at; break; }
            }
        if (file == null || file.Offset < 0) return false;
        width = (file.Compressed >> 16) & 0xFFFF;
        height = file.Compressed & 0xFFFF;
        return width > 0 && height > 0;
    }

    /// <summary>
    /// Decoded gump, 16-bit little-endian BGRA1555, bit 15 set when the pixel is opaque.
    /// Cached after the first read. Does not upload a texture.
    /// </summary>
    public bool TryGumpPixels(int graphic, out byte[] pixels, out int width, out int height) {
        pixels = null;
        width = height = 0;
        if (_gumpPixels.TryGetValue(graphic, out var hit)) {
            pixels = hit.pixels;
            width = hit.width;
            height = hit.height;
            return pixels != null && width > 0 && height > 0;
        }
        if (_gumpPixelsMiss.Contains(graphic)) return false;
        if (Gumps is not BinaryArchive bin || bin.Files == null || graphic < 0) {
            _gumpPixelsMiss.Add(graphic);
            return false;
        }
        var file = FindGump(bin, graphic);
        if (file == null && GumpsDef != null && GumpsDef.ItemHasGumpTranslation(graphic, out var translated, out _) && translated != graphic)
            file = FindGump(bin, translated);
        if (file == null) {
            _gumpPixelsMiss.Add(graphic);
            return false;
        }
        var gump = bin.GetAsset<Binary_Gump>(file, throwOnError: false).GetAwaiter().GetResult();
        if (gump == null) {
            _gumpPixelsMiss.Add(graphic);
            return false;
        }
        var packed = gump.Create("", o => (TextureAsBytes)o);
        pixels = packed.Bytes;
        width = gump.Width;
        height = gump.Height;
        if (pixels == null || width <= 0 || height <= 0 || pixels.Length < width * height * 2) {
            pixels = null;
            width = height = 0;
            _gumpPixelsMiss.Add(graphic);
            return false;
        }
        _gumpPixels[graphic] = (pixels, width, height);
        return true;
    }

    /// <summary>Block rows for Felucca, Trammel, Ilshenar, Malas, Tokuno, and Ter Mur.</summary>
    static readonly int[] FacetBlocks = [512, 512, 200, 256, 181, 512];

    /// <summary>
    /// Felucca. map0LegacyMUL.uop is flattened by OpenMap. A missing map leaves the overhead labels.
    /// </summary>
    async Task OpenFelucca(Archive game) {
        try {
            var map = await OpenMapStream(game, 0);
            var sta = await TryData(game, "staidx0.mul");
            var stat = await TryData(game, "statics0.mul");
            lock (_mapGate) ApplyMap(map, sta, stat, 0);
        }
        catch (Exception) {
            lock (_mapGate) {
                _map = null;
                _mapWidth = 0;
            }
        }
    }

    /// <summary>
    /// 0xBF subcommand 0x0008. A missing Trammel pack keeps Felucca's files.
    /// Any other missing pack leaves the map that is already open.
    /// </summary>
    public void UseFacet(int index) {
        if (_game == null || (uint)index >= (uint)FacetBlocks.Length || index == _facet) return;
        try {
            var map = OpenMapStream(_game, index).GetAwaiter().GetResult();
            if (map == null && index == 1) {
                lock (_mapGate) { if (_facet == 0) _facet = 1; }
                return;
            }
            if (map == null) return;
            var sta = TryData(_game, $"staidx{index}.mul").GetAwaiter().GetResult();
            var stat = TryData(_game, $"statics{index}.mul").GetAwaiter().GetResult();
            lock (_mapGate) ApplyMap(map, sta, stat, index);
        }
        catch (Exception) { }
    }

    static async Task<Stream> OpenMapStream(Archive game, int index) {
        var pack = OpenPack(game, $"map{index}LegacyMUL.uop");
        if (pack is BinaryArchive bin && bin.Files != null && bin.Files.Count > 0) {
            var flat = Binary_UO.OpenMap(bin);
            if (flat != null) return flat;
        }
        return await TryData(game, $"map{index}.mul");
    }

    void ApplyMap(Stream map, Stream sta, Stream stat, int index) {
        _map = map;
        _staidx = sta;
        _statics = stat;
        _blockHeight = FacetBlocks[index];
        _facet = index;
        _mapWidth = map != null && _blockHeight > 0 && map.CanSeek && map.Length > 0 ? (int)(map.Length / (_blockHeight * 196L)) : 0;
        _landBlocks.Clear();
        _staticBlocks.Clear();
    }

    static async Task<Stream> TryData(Archive game, string name) {
        try {
            var s = await game.GetData(name, default, false);
            if (s == null) return null;
            if (s.CanSeek) return s;
            var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            ms.Position = 0;
            return ms;
        }
        catch (Exception) { return null; }
    }

    /// <summary>
    /// One Felucca land tile. x is the outer index inside the 8x8 block, matching UltimaCell.ReadLand.
    /// </summary>
    public bool TryLandTile(int wx, int wy, out ushort tile, out sbyte z) {
        tile = 0;
        z = 0;
        if (_map == null || _mapWidth <= 0 || wx < 0 || wy < 0) return false;
        var cx = wx >> 3;
        var cy = wy >> 3;
        if (cx >= _mapWidth || cy >= _blockHeight) return false;
        var block = LandBlock(cx, cy);
        if (block == null) return false;
        var o = ((wx & 7) * 8 + (wy & 7)) * 3;
        tile = (ushort)((block[o] | (block[o + 1] << 8)) & 0x3FFF);
        z = (sbyte)block[o + 2];
        return true;
    }

    byte[] LandBlock(int cx, int cy) {
        var key = ((long)cx << 32) | (uint)cy;
        lock (_mapGate) {
            if (_map == null || cy < 0 || cy >= _blockHeight || cx < 0 || cx >= _mapWidth) return null;
            if (_landBlocks.TryGetValue(key, out var hit)) return hit;
            var ptr = ((long)cx * _blockHeight + cy) * 196 + 4;
            var data = new byte[192];
            if (ptr < 0 || ptr + 192 > _map.Length) return null;
            _map.Position = ptr;
            if (_map.Read(data, 0, 192) < 192) return null;
            _landBlocks[key] = data;
            return data;
        }
    }

    /// <summary>
    /// Statics whose local x,y is this tile. Skips 0x0C45..0x0DAF. graphic is the item id, not the art file id.
    /// </summary>
    public void CopyStatics(int wx, int wy, List<(ushort graphic, sbyte z)> dest) {
        if (dest == null || _staidx == null || _statics == null || _mapWidth <= 0 || wx < 0 || wy < 0) return;
        var cx = wx >> 3;
        var cy = wy >> 3;
        if (cx >= _mapWidth || cy >= _blockHeight) return;
        var recs = StaticBlock(cx, cy);
        if (recs == null || recs.Length < 7) return;
        var lx = wx & 7;
        var ly = wy & 7;
        for (var i = 0; i + 7 <= recs.Length; i += 7) {
            var tile = (ushort)(recs[i] | (recs[i + 1] << 8));
            if (recs[i + 2] != lx || recs[i + 3] != ly) continue;
            if (tile >= 0x0C45 && tile <= 0x0DAF) continue;
            dest.Add((tile, (sbyte)recs[i + 4]));
        }
    }

    byte[] StaticBlock(int cx, int cy) {
        var key = ((long)cx << 32) | (uint)cy;
        lock (_mapGate) {
            if (_staidx == null || _statics == null || cy < 0 || cy >= _blockHeight || cx < 0 || cx >= _mapWidth) return null;
            if (_staticBlocks.TryGetValue(key, out var hit)) return hit;
            var pos = ((long)cx * _blockHeight + cy) * 12;
            var idx = new byte[8];
            if (pos < 0 || pos + 8 > _staidx.Length) return EmptyStatic(key);
            _staidx.Position = pos;
            if (_staidx.Read(idx, 0, 8) < 8) return EmptyStatic(key);
            var lookup = BitConverter.ToInt32(idx, 0);
            var length = BitConverter.ToInt32(idx, 4);
            if (lookup < 0 || length <= 0) return EmptyStatic(key);
            var buf = new byte[length];
            if ((long)lookup + length > _statics.Length) return EmptyStatic(key);
            _statics.Position = lookup;
            length = _statics.Read(buf, 0, length);
            if (length < buf.Length) buf = buf[..length];
            _staticBlocks[key] = buf;
            return buf;
        }
    }

    byte[] EmptyStatic(long key) {
        _staticBlocks[key] = [];
        return _staticBlocks[key];
    }

    /// <summary>
    /// Stand height for one predicted step. Land is the average of the four corners.
    /// A static is included when tiledata marks it Surface or Bridge. Step up is 14, step down is 25.
    /// </summary>
    public int StepZ(int x, int y, int fromZ) {
        if (!TryLandTile(x, y, out _, out var z0)) return fromZ;
        var best = int.MinValue;
        Note(LandAverage(x, y, z0), fromZ, ref best);
        var list = new List<(ushort graphic, sbyte z)>(8);
        CopyStatics(x, y, list);
        var items = _tile?.ItemDatas;
        for (var i = 0; i < list.Count; i++) {
            var (graphic, z) = list[i];
            if (items == null || graphic >= items.Length) continue;
            var item = items[graphic];
            if (!item.IsSurface && !item.IsBridge) continue;
            Note(z + item.CalcHeight, fromZ, ref best);
        }
        return best == int.MinValue ? fromZ : best;
    }

    int LandAverage(int x, int y, sbyte z00) {
        var z10 = z00;
        var z01 = z00;
        var z11 = z00;
        if (TryLandTile(x + 1, y, out _, out var a)) z10 = a;
        if (TryLandTile(x, y + 1, out _, out var b)) z01 = b;
        if (TryLandTile(x + 1, y + 1, out _, out var c)) z11 = c;
        var v = z00 + z10 + z01 + z11;
        return v < 0 ? (v - 2) / 4 : v / 4;
    }

    static void Note(int top, int fromZ, ref int best) {
        var d = top - fromZ;
        if (d > 14 || d < -25) return;
        if (top > best) best = top;
    }

    /// <summary>
    /// Land file ids are 0..0x3FFF. Static art is the item graphic plus 0x4000.
    /// Pixels are little-endian BGRA1555. A missing land tile falls back to radarcol.mul.
    /// </summary>
    public bool TryArtPixels(int fileId, out byte[] pixels, out int width, out int height) {
        pixels = null;
        width = height = 0;
        if (_artPixels.TryGetValue(fileId, out var hit)) {
            pixels = hit.pixels;
            width = hit.width;
            height = hit.height;
            return pixels != null && width > 0 && height > 0;
        }
        if (fileId >= 0 && fileId <= 0xFFFF && !_artMiss.Contains(fileId) && Arts is BinaryArchive bin) {
            var path = fileId < 0x4000 ? $"land/file{fileId:x5}.land" : $"art/file{fileId:x5}.art";
            try {
                var asset = bin.GetAsset<object>(path, throwOnError: false).GetAwaiter().GetResult();
                if (asset is ITexture tex) {
                    var packed = tex.Create("", o => (TextureAsBytes)o);
                    pixels = packed.Bytes;
                    width = tex.Width;
                    height = tex.Height;
                    if (pixels != null && width > 0 && height > 0 && pixels.Length >= width * height * 2) {
                        _artPixels[fileId] = (pixels, width, height);
                        return true;
                    }
                }
            }
            catch (Exception) { }
            _artMiss.Add(fileId);
        }
        if (fileId >= 0 && fileId < 0x4000 && TryRadarDiamond(fileId, out pixels)) {
            width = height = 44;
            _artPixels[fileId] = (pixels, 44, 44);
            return true;
        }
        pixels = null;
        width = height = 0;
        return false;
    }

    bool TryRadarDiamond(int id, out byte[] pixels) {
        pixels = null;
        if (HuesRadar == null || id < 0 || id >= HuesRadar.Colors.Length) return false;
        var c = HuesRadar.Colors[id];
        var r = (int)(c & 0xFF);
        var g = (int)((c >> 8) & 0xFF);
        var b = (int)((c >> 16) & 0xFF);
        var p = (ushort)(((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3) | 0x8000);
        var lo = (byte)p;
        var hi = (byte)(p >> 8);
        var bd = new byte[44 * 44 * 2];
        var xOffset = 21;
        var xRun = 2;
        for (var y = 0; y < 22; y++, xOffset--, xRun += 2) FillRun(bd, y, xOffset, xRun, lo, hi);
        xOffset = 0;
        xRun = 44;
        for (var y = 22; y < 44; y++, xOffset++, xRun -= 2) FillRun(bd, y, xOffset, xRun, lo, hi);
        pixels = bd;
        return true;
    }

    static void FillRun(byte[] bd, int y, int xOffset, int xRun, byte lo, byte hi) {
        var row = y * 44;
        var end = xOffset + xRun;
        for (var x = xOffset; x < end; x++) {
            var i = (row + x) * 2;
            bd[i] = lo;
            bd[i + 1] = hi;
        }
    }

    static FileSource FindGump(BinaryArchive bin, int graphic) {
        IList<FileSource> files = bin.Files;
        FileSource file = null;
        if (graphic < files.Count) {
            var at = files[graphic];
            if (at != null && at.Id == graphic) file = at;
        }
        if (file == null)
            for (var i = 0; i < files.Count; i++) {
                var at = files[i];
                if (at != null && at.Id == graphic) { file = at; break; }
            }
        return file == null || file.Offset < 0 ? null : file;
    }

    /// <summary>
    /// One frame of a body. People stand on action 4. Directions 5..7 mirror 3..1.
    /// Pixels are little-endian BGRA1555. The anchor is the frame center.
    /// </summary>
    public bool TryBodyFrame(int body, int action, int direction, out byte[] pixels, out int width, out int height, out int centerX, out int centerY) {
        pixels = null;
        width = height = centerX = centerY = 0;
        if (body <= 0 || direction < 0 || direction > 7) return false;
        var key = ((long)body << 16) | ((long)(action & 0xFF) << 8) | (uint)(direction & 7);
        if (_bodyFrames.TryGetValue(key, out var hit)) {
            pixels = hit.pixels;
            width = hit.width;
            height = hit.height;
            centerX = hit.cx;
            centerY = hit.cy;
            return pixels != null;
        }
        if (_bodyMiss.Contains(key)) return false;
        for (var file = 0; file < 5; file++) {
            var index = AnimIndex(body, action, direction, file);
            if (!TryAnimGroup(file, index, out var group)) continue;
            if (!DecodeFrame(group, direction > 4, out pixels, out width, out height, out centerX, out centerY)) continue;
            _bodyFrames[key] = (pixels, width, height, centerX, centerY);
            return true;
        }
        _bodyMiss.Add(key);
        return false;
    }

    static int AnimIndex(int body, int action, int direction, int file) {
        int index;
        if (file == 1) index = body < 200 ? body * 110 : 22000 + (body - 200) * 65;
        else if (file == 2) {
            if (body < 300) index = body * 65;
            else if (body < 400) index = 33000 + (body - 300) * 110;
            else index = 35000 + (body - 400) * 175;
        }
        else if (file == 4 && body == 34) index = 22000 + (body - 200) * 65;
        else if (body < 200) index = body * 110;
        else if (body < 400) index = 22000 + (body - 200) * 65;
        else index = 35000 + (body - 400) * 175;
        index += action * 5;
        if (direction <= 4) index += direction;
        else index += direction - (direction - 4) * 2;
        return index;
    }

    bool TryAnimGroup(int file, int index, out byte[] group) {
        group = null;
        if (Animations == null || file < 0 || file >= Animations.Anims.Length || index < 0) return false;
        if (Animations.Anims[file] is not BinaryArchive bin) return false;
        try {
            var s = bin.GetData($"file{index:x5}.anim", default, false).GetAwaiter().GetResult();
            if (s == null) return false;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            group = ms.ToArray();
            return group.Length >= 516;
        }
        catch (Exception) { return false; }
    }

    static bool DecodeFrame(byte[] group, bool flip, out byte[] pixels, out int width, out int height, out int centerX, out int centerY) {
        pixels = null;
        width = height = centerX = centerY = 0;
        var count = BitConverter.ToInt32(group, 512);
        if (count <= 0 || count > 64) return false;
        var table = 516;
        if (table + count * 4 > group.Length) return false;
        var at = 512 + BitConverter.ToInt32(group, table);
        if (at < 0 || at + 8 > group.Length) return false;
        centerX = BitConverter.ToInt16(group, at);
        centerY = BitConverter.ToInt16(group, at + 2);
        width = BitConverter.ToUInt16(group, at + 4);
        height = BitConverter.ToUInt16(group, at + 6);
        if (width <= 0 || height <= 0 || width > 512 || height > 512) return false;
        pixels = new byte[width * height * 2];
        var xBase = centerX - 0x200;
        var yBase = centerY + height - 0x200;
        const uint mask = (0x200u << 22) | (0x200u << 12);
        var p = at + 8;
        if (p + 4 > group.Length) return false;
        var header = BitConverter.ToUInt32(group, p);
        p += 4;
        while (header != 0x7FFF7FFF) {
            header ^= mask;
            var run = (int)(header & 0xFFF);
            var x = (int)((header >> 22) & 0x3FF) + xBase;
            var y = (int)((header >> 12) & 0x3FF) + yBase;
            if (p + run > group.Length) return false;
            for (var i = 0; i < run; i++) {
                var c = (ushort)(BitConverter.ToUInt16(group, group[p + i] * 2) ^ 0x8000);
                var px = x + i;
                if (c != 0 && px >= 0 && px < width && y >= 0 && y < height) {
                    var o = (y * width + px) * 2;
                    pixels[o] = (byte)c;
                    pixels[o + 1] = (byte)(c >> 8);
                }
            }
            p += run;
            if (p + 4 > group.Length) break;
            header = BitConverter.ToUInt32(group, p);
            p += 4;
        }
        if (!flip) return true;
        var flipped = new byte[pixels.Length];
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) {
                var s = (y * width + x) * 2;
                var d = (y * width + (width - 1 - x)) * 2;
                flipped[d] = pixels[s];
                flipped[d + 1] = pixels[s + 1];
            }
        pixels = flipped;
        centerX = width - centerX;
        return true;
    }
}

public class UOGameClient : GameClient {
    //AudioManager Audio;
    public Assets Assets { get; }

    public UOGameClient(ClientState state) : base(state) {
        TypeX.ScanTypes([typeof(XboxArchive)]);
        Assets = new(state.Archive.Game as UOGame);
    }

    public override async Task LoadContent() {
        await base.LoadContent();
        // Fonts, SolidColorTextureCache, and AudioManager need the MonoGame device.
        // They are not in this assembly. Plugins stay on the host, after this returns.
        var game = Archive.Game as UOGame;
        if (string.IsNullOrEmpty(game?.Found?.Root)) throw new InvalidOperationException("UO install path is not set.");
        await Assets.Load(Archive, this);
        if (Host is not ISceneHost host) throw new InvalidOperationException("UO client host cannot install a scene.");
        // Sink has to exist before LoginGump measures fonts.mul widths.
        AttachGumpArt(Host);
        host.SetScene(CreateLoginScene(this));
    }

    // Origin cannot reference GameX.Origin.UO: that project already references this one, and it is netstandard2.1.
    static SceneBase CreateLoginScene(UOGameClient client) {
        Assembly asm = null;
        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            if (loaded.GetName().Name == "GameX.Origin.UO") { asm = loaded; break; }
        if (asm == null) {
            var file = Path.Combine(AppContext.BaseDirectory, "GameX.Origin.UO.dll");
            if (!File.Exists(file)) throw new FileNotFoundException("GameX.Origin.UO.dll was not next to the client.", file);
            asm = Assembly.LoadFrom(file);
        }
        var login = asm.GetType("GameX.Origin.UO.UOLogin") ?? throw new InvalidOperationException("GameX.Origin.UO.UOLogin is missing.");
        var create = login.GetMethod("Create") ?? throw new InvalidOperationException("UOLogin.Create is missing.");
        return (SceneBase)create.Invoke(null, [client]);
    }

    // Origin cannot reference GameX.Origin.Mg because that project is net10.0. The painter lives there because this project has no Texture2D.
    static void AttachGumpArt(IClientHost host) {
        Assembly asm = null;
        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            if (loaded.GetName().Name == "GameX.Origin.Mg") { asm = loaded; break; }
        if (asm == null) {
            var file = Path.Combine(AppContext.BaseDirectory, "GameX.Origin.Mg.dll");
            if (!File.Exists(file)) return;
            asm = Assembly.LoadFrom(file);
        }
        var type = asm.GetType("GameX.Origin.Mg.UO.UoGumpArt");
        type?.GetMethod("Attach")?.Invoke(null, [host]);
    }

    public override Task UnloadContent() => Task.CompletedTask;
}
