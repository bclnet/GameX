using System;
using System.Collections.Generic;

namespace GameX.Cig;

public static class Helper {
    static readonly Family familyCig = FamilyManager.GetFamily("Cig");

    public static readonly Dictionary<string, Lazy<Archive>> Paks = new()
    {
        { "Cig:StarCitizen", new Lazy<Archive>(() => familyCig.OpenArchive(new Uri("game:/Sbi.p4k#StarCitizen"))) },
    };
}
