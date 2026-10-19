using GameX;
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
    //Fonts Fonts;
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


    public class UoAnimations {
        public Archive[] Anims = new Archive[10];

        public async Task<UoAnimations> Load(bool uop, Archive game, ClientVersion version) {
            for (var i = 0; i < 10; i++) Anims[i] = await game.GetAsset<Archive>(uop ? $"AnimationFrame{i + 1}.uop" : $"anim{(i == 0 ? string.Empty : (i + 1).ToString())}.idx");
            if (uop & version > ClientVersion.CV_60144) await game.GetAsset<Binary_Animdata>("AnimationSequence.uop");
            if (version >= ClientVersion.CV_500A)
                await game.GetAsset<Binary_MobType>("mobtypes.txt");
            await game.GetAsset<Binary_Animdef>("Anim1.def");
            await game.GetAsset<Binary_Animdef>("Anim2.def");
            if (version >= ClientVersion.CV_300) {
                await game.GetAsset<Binary_Animdata>("Equipconv.def");
                await game.GetAsset<Binary_Animdata>("Bodyconv.def");
                await game.GetAsset<Binary_Animdata>("Body.def");
                await game.GetAsset<Binary_Animdata>("Corpse.def");
            }
            return this;
        }
    }

    public async Task Load(Archive game, UOGameClient ctx) {
        var version = ((UOGame)game.Game).Version;
        var lang = "enu";
        Animations = await new UoAnimations().Load(Uop, game, version);
        AnimData = await game.GetAsset<Binary_Animdata>("animdata.mul");
        Arts = await game.GetAsset<Archive>(Uop ? "artLegacyMUL.uop" : "artidx.mul");
        //Maps = new Maps(this);
        Clilocs = await game.GetAsset<Binary_StringTable>($"cliloc.{lang}");
        Gumps = await game.GetAsset<Archive>(Uop ? "gumpartLegacyMUL.uop" : "gumpidx.mul");
        GumpsDef = await game.GetAsset<Binary_GumpDef>("gump.def");
        //Fonts = await game.GetAsset<Binary_AsciiFont>("fonts.mul");
        //Fonts = await game.GetAsset<Binary_UnicodeFont>("fonts.mul");
        Hues = await game.GetAsset<Binary_Hues>("hues.mul");
        HuesRadar = await game.GetAsset<Binary_RadarColor>("radarcol.mul");
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

    /// <summary>
    /// Width and height packed in the gump index extra (<see cref="FileSource.Compressed"/>). Does not decode pixels.
    /// </summary>
    public bool TryGumpSize(int graphic, out int width, out int height) {
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
        host.SetScene(CreateLoginScene(this));
        AttachGumpArt(Host);
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

    // Origin cannot reference GameX.Engine.Mg (net10.0). The painter lives there because this project has no Texture2D.
    static void AttachGumpArt(IClientHost host) {
        Assembly asm = null;
        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            if (loaded.GetName().Name == "GameX.Engine.Mg") { asm = loaded; break; }
        if (asm == null) {
            var file = Path.Combine(AppContext.BaseDirectory, "GameX.Engine.Mg.dll");
            if (!File.Exists(file)) return;
            asm = Assembly.LoadFrom(file);
        }
        var type = asm.GetType("GameX.Engines.Mg.UoGumpArt");
        type?.GetMethod("Attach")?.Invoke(null, [host]);
    }

    public override Task UnloadContent() => Task.CompletedTask;
}
