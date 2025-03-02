﻿using System;
using System.Collections.Generic;
using static GameX.FamilyManager;

namespace GameX;

public static class TestHelper
{
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

    public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
    {
        { "AC:AC", new Lazy<PakFile>(() => familyAC.OpenPakFile(new Uri("game:/*.dat#AC"))) },
        { "Arkane:AF", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*.pak#AF"))) },
        { "Arkane:DOM", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*_dir.vpk#DOM"))) },
        { "Arkane:D", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*TOC.txt#D"))) },
        { "Arkane:D2", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*.index#D2"))) },
        { "Arkane:P", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*.pak#P"))) },
        { "Arkane:D:DOTO", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*.index#D:DOTO"))) },
        { "Arkane:W:YB", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*#W:YB"))) },
        { "Arkane:W:CP", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*#W:CP"))) },
        { "Arkane:DL", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*.index#DL"))) },
        //{ "Arkane:RF", new Lazy<PakFile>(() => familyArkane.OpenPakFile(new Uri("game:/*#RF"))) }, //: future

        { "Cry:MWO", new Lazy<PakFile>(() => familyCry.OpenPakFile(new Uri("game:/*.pak#MWO"))) },
        { "Cyanide:TC", new Lazy<PakFile>(() => familyCyanide.OpenPakFile(new Uri("game:/*.cpk#TC"))) },
        { "Origin:UO", new Lazy<PakFile>(() => familyOrigin.OpenPakFile(new Uri("game:/*.idx#UO"))) },
        { "Origin:U9", new Lazy<PakFile>(() => familyOrigin.OpenPakFile(new Uri("game:/*.flx#U9"))) },
        { "Red:Witcher", new Lazy<PakFile>(() => familyRed.OpenPakFile(new Uri("game:/*.key#Witcher"))) },
        { "Red:Witcher2", new Lazy<PakFile>(() => familyRed.OpenPakFile(new Uri("game:/*#Witcher2"))) },
        { "Red:Witcher3", new Lazy<PakFile>(() => familyRed.OpenPakFile(new Uri("game:/content0/*#Witcher3"))) },
        { "Red:CP77", new Lazy<PakFile>(() => familyRed.OpenPakFile(new Uri("game:/*.archive#CP77"))) },
        { "Rsi:StarCitizen", new Lazy<PakFile>(() => familyRsi.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
        { "Tes:Morrowind", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Morrowind.bsa#Morrowind"))) },
        { "Tes:Oblivion", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Oblivion*.bsa#Oblivion"))) },
        { "Tes:Skyrim", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Skyrim*.bsa#Skyrim"))) },
        { "Tes:SkyrimSE", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Skyrim*.bsa#SkyrimSE"))) },
        { "Tes:Fallout2", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/*.dat#Fallout2"))) },
        { "Tes:Fallout3", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Fallout*.bsa#Fallout3"))) },
        { "Tes:FalloutNV", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Fallout*.bsa#FalloutNV"))) },
        { "Tes:Fallout4", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Fallout4*.ba2#Fallout4"))) },
        { "Tes:Fallout4VR", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/Fallout4*.ba2#FalloutVR"))) },
        { "Tes:Fallout76", new Lazy<PakFile>(() => familyTes.OpenPakFile(new Uri("game:/SeventySix*.ba2#Fallout76"))) },
        { "Valve:Dota2", new Lazy<PakFile>(() => familyValve.OpenPakFile(new Uri("game:/(core:dota)/*_dir.vpk#Dota2"))) },
    };
}
