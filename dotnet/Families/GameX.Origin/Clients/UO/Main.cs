using GameX.Origin.Formats.UO;
using GameX.Origin.Structs.UO;
using GameX.Xbox.Formats;
using GameX.Xbox.Formats.Xna;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Origin.Clients.UO;

public class Main(bool uop) {
    bool Uop = uop;
    //Animations Animations;
    Binary_Animdata AnimData;
    Archive Arts;
    //Maps Maps;
    Binary_StringTable Clilocs;
    //Gumps Gumps;
    //Fonts Fonts;
    //Hues Hues;
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

    public async Task Load(Archive game, GameController ctx) {
        var lang = "enu";
        //Animations = new Animations(this);
        AnimData = await game.GetAsset<Binary_Animdata>("animdata.mul");
        Arts = game.GetArchive(Uop ? "artLegacyMUL.uop" : "artidx.mul");
        //Maps = new Maps(this);
        Clilocs = await game.GetAsset<Binary_StringTable>($"cliloc.{lang}");
        //Gumps = new Gumps(this);
        //Fonts = new Fonts(this);
        //Hues = new Hues(this);
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
