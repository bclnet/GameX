using System;
using System.Collections.Generic;
using static GameX.FamilyManager;

namespace GameX;

public static class TestHelper {
    static readonly Family familyAC = GetFamily("AC");
    static readonly Family familyArkane = GetFamily("Arkane");
    static readonly Family familyBioware = GetFamily("Bioware");
    static readonly Family familyBlizzard = GetFamily("Blizzard");
    static readonly Family familyCapcom = GetFamily("Capcom");
    static readonly Family familyCry = GetFamily("Cry");
    static readonly Family familyCryptic = GetFamily("Cryptic");
    static readonly Family familyCyanide = GetFamily("Cyanide");
    static readonly Family familyFrontier = GetFamily("Frontier");
    static readonly Family familyHpl = GetFamily("Hpl");
    static readonly Family familyId = GetFamily("Id");
    static readonly Family familyIW = GetFamily("IW");
    static readonly Family familyLith = GetFamily("Lith");
    static readonly Family familyOrigin = GetFamily("Origin");
    static readonly Family familyRed = GetFamily("Red");
    static readonly Family familyRsi = GetFamily("Rsi");
    static readonly Family familyTes = GetFamily("Tes");
    static readonly Family familyUnity = GetFamily("Unity");
    static readonly Family familyUnreal = GetFamily("Unreal");
    static readonly Family familyValve = GetFamily("Valve");

    public static readonly Dictionary<string, Lazy<Archive>> Paks = new()
    {
        { "AC:AC", new Lazy<Archive>(() => familyAC.GetArchive(new Uri("game:/*.dat#AC"))) },
        { "Arkane:AF", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*.arc#AF"))) },
        { "Arkane:DOM", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*_dir.vpk#DOM"))) },
        { "Arkane:Radius", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*TOC.txt#Radius"))) },
        { "Arkane:D2", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*.Index#D2"))) },
        { "Arkane:P", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*.arc#P"))) },
        { "Arkane:Radius:DOTO", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*.Index#Radius:DOTO"))) },
        { "Arkane:Height:YB", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*#Height:YB"))) },
        { "Arkane:Height:CP", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*#Height:CP"))) },
        { "Arkane:DL", new Lazy<Archive>(() => familyArkane.GetArchive(new Uri("game:/*.Index#DL"))) },
        //{ "Arkane:RF", new Lazy<Archive>(() => familyArkane.OpenArchive(new Uri("game:/*#RF"))) }, //: future

        { "Cry:MWO", new Lazy<Archive>(() => familyCry.GetArchive(new Uri("game:/*.arc#MWO"))) },
        { "Cyanide:TC", new Lazy<Archive>(() => familyCyanide.GetArchive(new Uri("game:/*.cpk#TC"))) },
        { "Origin:UO", new Lazy<Archive>(() => familyOrigin.GetArchive(new Uri("game:/*.idx#UO"))) },
        { "Origin:U9", new Lazy<Archive>(() => familyOrigin.GetArchive(new Uri("game:/*.flx#U9"))) },
        { "Red:Witcher", new Lazy<Archive>(() => familyRed.GetArchive(new Uri("game:/*.key#Witcher"))) },
        { "Red:Witcher2", new Lazy<Archive>(() => familyRed.GetArchive(new Uri("game:/*#Witcher2"))) },
        { "Red:Witcher3", new Lazy<Archive>(() => familyRed.GetArchive(new Uri("game:/content0/*#Witcher3"))) },
        { "Red:CP77", new Lazy<Archive>(() => familyRed.GetArchive(new Uri("game:/*.archive#CP77"))) },
        { "Rsi:StarCitizen", new Lazy<Archive>(() => familyRsi.GetArchive(new Uri("game:/Sbi.p4k#StarCitizen"))) },
        { "Tes:Morrowind", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Morrowind.bsa#Morrowind"))) },
        { "Tes:Oblivion", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Oblivion*.bsa#Oblivion"))) },
        { "Tes:Skyrim", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Skyrim*.bsa#Skyrim"))) },
        { "Tes:SkyrimSE", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Skyrim*.bsa#SkyrimSE"))) },
        { "Tes:Fallout2", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/*.dat#Fallout2"))) },
        { "Tes:Fallout3", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Fallout*.bsa#Fallout3"))) },
        { "Tes:FalloutNV", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Fallout*.bsa#FalloutNV"))) },
        { "Tes:Fallout4", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Fallout4*.ba2#Fallout4"))) },
        { "Tes:Fallout4VR", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/Fallout4*.ba2#FalloutVR"))) },
        { "Tes:Fallout76", new Lazy<Archive>(() => familyTes.GetArchive(new Uri("game:/SeventySix*.ba2#Fallout76"))) },
        { "Valve:Dota2", new Lazy<Archive>(() => familyValve.GetArchive(new Uri("game:/(core:dota)/*_dir.vpk#Dota2"))) },
    };
}
