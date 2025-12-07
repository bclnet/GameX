using GameX.Cig.Apps.DataForge;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameX.Cig.Apps;

/// <summary>
/// DataForgeApp
/// </summary>
/// <seealso cref="FamilyApp" />
public class DataForgeApp(Family family, string id, JsonElement elem) : FamilyApp(family, id, elem) {
    public readonly Database Db = new();

    public override async Task OpenAsync(Type explorerType, MetaManager manager) {
        await Db.OpenAsync(manager);
        await base.OpenAsync(explorerType, manager);
    }
}