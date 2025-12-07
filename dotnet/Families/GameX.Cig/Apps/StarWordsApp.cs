using GameX.Cig.Apps.StarWords;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Cig.Apps;

/// <summary>
/// StarWordsApp
/// </summary>
/// <seealso cref="FamilyApp" />
public class StarWordsApp(Family family, string id, JsonElement elem) : FamilyApp(family, id, elem) {
    public readonly Database Db = new();

    public override async Task OpenAsync(Type explorerType, MetaManager manager) {
        await Db.OpenAsync(manager);
        await base.OpenAsync(explorerType, manager);
    }
}