using GameX.Origin.Formats.UO;
using GameX.Xbox;
using System;
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
}

public class UOGameClient : GameClient {
    //AudioManager Audio;
    readonly Assets Assets;

    public UOGameClient(ClientState state) : base(state) {
        TypeX.ScanTypes([typeof(XboxArchive)]);
        Assets = new(state.Archive.Game as UOGame);
    }

    public override async Task LoadContent() {
        await base.LoadContent();
        //await Fonts<Texture2D>.Load(Game, Device);
        //SolidColorTextureCache.Load(Device);
        //Audio = new AudioManager();
#if false
        //SetScene(new MainScene(this));
#else
        await Assets.Load(Archive, this);
        //Audio.Initialize();
        // SetScene(new LoginScene(UO.World));
#endif
    }

    public override Task UnloadContent() => Task.CompletedTask;
}
