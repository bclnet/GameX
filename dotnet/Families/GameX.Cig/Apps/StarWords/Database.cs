using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace GameX.Cig.Apps.StarWords;

public class Node {
    static readonly Dictionary<string, Node> Paths = [];
    public string Name { get; private set; }
    public object Icon { get; private set; }
    public object Tag { get; }
    public List<Node> Items { get; } = [];
    public List<Entity> Entities { get; } = [];

    public static void CreateNode(MetaManager manager, List<Node> nodes, string k, string v) {
        var p = k.Split("_");
        var pathTake = p.Length > 3 ? 3 : 1;
        var path = string.Join('/', p.Take(pathTake));
        var icon = manager.FolderIcon;
        var node = Paths.TryGetValue(path, out var z) ? z : CreatePath(nodes, path, icon);
        node.Entities.Add(new Entity { Name = k, English = v });
    }

    static Node CreatePath(List<Node> nodes, string path, object icon) {
        var lastIndex = path.LastIndexOf('/');
        if (lastIndex == -1) {
            var newNode2 = new Node { Name = path, Icon = icon };
            nodes.Add(newNode2);
            Paths.Add(path, newNode2);
            return newNode2;
        }
        string prevPath = path[..lastIndex], nextPath = path[(lastIndex + 1)..];
        var node = Paths.TryGetValue(prevPath, out var z) ? z : CreatePath(nodes, prevPath, icon);
        var newNode = new Node { Name = nextPath, Icon = icon };
        node.Items.Add(newNode);
        Paths.Add(path, newNode);
        return newNode;
    }
}

public class Entity {
    public string Name { get; set; }
    public string English { get; set; }
}

/// <summary>
/// Database
/// </summary>
public class Database {
    readonly static string[] locales = [
        "chinese_(simplified)",
        "chinese_(traditional)",
        "english",
        "french_(france)",
        "german_(germany)",
        "italian_(italy)",
        "japanese_(japan)",
        "korean_(south_korea)",
        "polish_(poland)",
        "portuguese_(brazil)",
        "spanish_(latin_america)",
        "spanish_(spain)"
    ];
    Family family;
    Archive archive;

    public List<Node> Nodes = [];
    public Dictionary<string, Dictionary<string, string>> Others = [];

    public async Task OpenAsync(MetaManager manager) {
        family = FamilyManager.GetFamily("Cig");
        archive = family.GetArchive(new Uri("game:/Sbi.p4k#StarCitizen"));
        foreach (var local in locales) {
            var stream = await archive.GetData($"Sbi/Localization/{local}/global.ini");
            using var r = new StreamReader(stream, Encoding.UTF8);
            var body = r.ReadToEnd();
            var values = body.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=', 2)).ToDictionary(x => x[0].Trim(), x => x.Length > 1 ? x[1] : null);
            if (local != "english") Others.Add(local, values);
            else foreach (var (key, value) in values) Node.CreateNode(manager, Nodes, key, value);
        }
    }
}