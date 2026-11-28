using GameX;
using GameX.Engines.Mirage;
using Mirage;
using OpenX;
using OpenX.Gfx;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Mirage.Mcp;

/// <summary>
/// Opens a family archive and renders one asset through Mirage. The server process owns the scene, so a later texture call reads the same tree.
/// </summary>
static class Review {
    static int _shot;

    public static int Command(string[] args) {
        if (args.Length < 4) {
            Console.Error.WriteLine("review <family> <uri> <path> [type]");
            return 2;
        }
        var result = Open(args[1], args[2], null, args[3], args.Length > 4 ? args[4] : null);
        Console.Error.WriteLine(result["content"]?[0]?["text"]?.GetValue<string>());
        return result["isError"]?.GetValue<bool>() == true ? 1 : 0;
    }

    public static JsonObject Families() {
        FamilyManager.LoadFamilies(throwOnError: false);
        var sb = new StringBuilder();
        foreach (var f in FamilyManager.Families.Values.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase)) {
            var games = f.Games?.Values.ToList() ?? [];
            var found = games.Count(g => g.Found != null);
            sb.Append(f.Id).Append('\t').Append(f.Name).Append("\tgames=").Append(games.Count).Append("\tfound=").Append(found).Append('\n');
        }
        return Text(false, sb.Length == 0 ? "no families" : sb.ToString().TrimEnd());
    }

    public static JsonObject Games(string familyId) {
        var family = FamilyOf(familyId);
        var sb = new StringBuilder();
        foreach (var g in family.Games.Values.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
            sb.Append(g.Id).Append('\t').Append(g.Name).Append('\t').Append(g.Found?.Root ?? "").Append('\n');
        return Text(false, sb.Length == 0 ? "no games" : sb.ToString().TrimEnd());
    }

    public static JsonObject Samples(string familyId, string gameId) {
        var family = FamilyOf(familyId);
        var game = family.GetGame(gameId ?? "", out _);
        if (game == null) return Text(true, "game is required");
        if (!family.Samples.TryGetValue(game.Id, out var files) || files.Count == 0) return Text(false, "no samples");
        var sb = new StringBuilder();
        for (var i = 0; i < files.Count; i++) {
            var f = files[i];
            var type = f.Data != null && f.Data.TryGetValue("type", out var t) ? t as string : null;
            sb.Append(i).Append('\t').Append(type ?? "").Append('\t').Append(f.Paths == null ? "" : string.Join(" | ", f.Paths)).Append('\n');
        }
        return Text(false, sb.ToString().TrimEnd());
    }

    public static JsonObject Files(string familyId, string uri, string gameId, string filter, int limit, int offset) {
        var family = FamilyOf(familyId);
        uri = UriOf(uri, gameId);
        var archive = family.GetArchive(uri);
        if (archive is not BinaryArchive bin || bin.Files == null || bin.Files.Count == 0)
            return Text(false, $"uri={uri}\nfiles=0\narchive={archive?.GetType().Name}");
        var files = bin.Files;
        limit = Math.Clamp(limit, 1, 200);
        offset = Math.Max(0, offset);
        var match = string.IsNullOrEmpty(filter) ? files.Where(f => f?.Path != null).ToList() : files.Where(f => f?.Path != null && f.Path.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        var page = match.Skip(offset).Take(limit).ToList();
        var sb = new StringBuilder();
        sb.Append("uri=").Append(uri).Append("\nmatched=").Append(match.Count).Append("\noffset=").Append(offset).Append('\n');
        foreach (var f in page) sb.Append(f.Path).Append('\n');
        return Text(false, sb.ToString().TrimEnd());
    }

    public static JsonObject Open(string familyId, string uri, string gameId, string path, string type) {
        try {
            if (string.IsNullOrEmpty(path)) return Text(true, "path is required");
            MirageEngine.KeepPixels = true;
            MirageRenderer.Register();
            var family = FamilyOf(familyId);
            var game = GameOf(family, gameId, uri);
            string sampleNote = null;
            if (path.StartsWith("sample:", StringComparison.OrdinalIgnoreCase)) {
                if (game == null) return Text(true, "sample:N needs a game id");
                var sample = game.GetSample(path[7..]);
                if (sample?.Paths == null || sample.Paths.Length == 0) return Text(true, $"no sample {path}");
                sampleNote = "sample " + string.Join(" | ", sample.Paths);
                if (string.IsNullOrEmpty(type) && sample.Data != null && sample.Data.TryGetValue("type", out var st)) type = st as string;
                path = sample.Paths[^1];
            }
            uri = UriOf(uri, game?.Id ?? gameId);
            MirageEngine.Reset();
            Log.Func = m => { MirageEngine.Tree.Log(m); Console.Error.WriteLine(m ?? ""); };
            var archive = family.GetArchive(uri);
            var asset = archive.GetAsset<object>(path).GetAwaiter().GetResult();
            var view = ForRenderer(asset);
            var used = RendererType(type, view);
            string error = null;
            if (asset == null) error = "asset was null";
            else if (used == null) { if (!Describe(asset)) error = "no renderer for " + (type ?? asset.GetType().Name); }
            else {
                try {
                    var renderer = MirageRenderer.CreateRenderer(null, EngineX.Gfx, archive, view, used);
                    if (renderer == null) { if (!Describe(asset)) error = "no renderer for " + used; }
                    else renderer.Start();
                }
                catch (Exception e) {
                    var msg = e.GetBaseException().Message;
                    // Missing-builder is not a failure when the asset can describe itself. Any other throw stays an error.
                    if (!MissingBuilder(msg) || !Describe(asset)) error = msg;
                }
            }
            return Shot(family.Id, uri, path, used, sampleNote, error, asset, view);
        }
        catch (Exception e) { return Text(true, e.GetBaseException().Message); }
    }

    public static JsonObject Texture(string name) {
        if (string.IsNullOrEmpty(name)) return Text(true, "name is required");
        if (MirageEngine.Tree == null || !MirageEngine.Tree.Resources.TryGet<Texture>(name, out var tex))
            return Text(true, "no texture " + name + " in the last scene");
        var png = SceneView.TexturePng(tex);
        var file = Save(png, "texture.png");
        return Content(false, file + "\n" + tex, png);
    }

    public static JsonObject Text(bool error, string text) => Content(error, text, null);

    // A loaded tab, FRM, NWX, or BAM stays that object. The view is its sheet when ToSheet returned cells.
    static object ForRenderer(object asset) {
        if (asset is GfxSpriteSheet) return asset;
        GfxSpriteSheet sheet;
        try {
            sheet = asset switch {
                GameX.Bullfrog.Formats.Binary_SyndicateX x => x.ToSheet(),
                GameX.Black.Formats.Binary_Frm frm => frm.ToSheet(),
                GameX.Lucas.Formats.Binary_Nwx nwx => nwx.ToSheet(),
                GameX.Beamdog.Formats.Binary_Bam bam => bam.ToSheet(),
                _ => null,
            };
        }
        catch { return asset; }
        if (sheet?.Frames == null || !sheet.Frames.Any(f => f.Width > 0 && f.Height > 0)) return asset;
        return sheet;
    }

    static JsonObject Shot(string family, string uri, string path, string type, string sampleNote, string error, object asset, object view) {
        var tree = MirageEngine.Tree;
        var scene = tree.ToText() ?? "";
        var resources = string.Join("\n", tree.Resources.All.OrderBy(r => r.Kind, StringComparer.Ordinal).ThenBy(r => r.Name, StringComparer.Ordinal).Select(r => r.ToString()));
        var png = SceneView.Png(tree);
        var n = ++_shot;
        var frame = Save(png, $"frame-{n}.png");
        var sceneFile = Path.Combine(Dir(), $"scene-{n}.txt");
        File.WriteAllText(sceneFile, scene);
        var sb = new StringBuilder();
        sb.Append("frame: ").Append(frame).Append('\n');
        sb.Append("scene: ").Append(sceneFile).Append('\n');
        sb.Append("family: ").Append(family).Append('\n');
        sb.Append("uri: ").Append(uri).Append('\n');
        sb.Append("path: ").Append(path).Append('\n');
        sb.Append("type: ").Append(type ?? "").Append('\n');
        if (asset != null) sb.Append("asset: ").Append(asset.GetType().Name).Append('\n');
        if (view is GfxSpriteSheet sheet) {
            sb.Append("sheet: ").Append(sheet.Frames.Length).Append(" frames");
            if (sheet.Anims is { Length: > 0 }) sb.Append(", ").Append(string.Join(", ", sheet.Anims.Select(a => a.Name)));
            sb.Append('\n');
        }
        if (!string.IsNullOrEmpty(sampleNote)) sb.Append(sampleNote).Append('\n');
        if (!string.IsNullOrEmpty(error)) sb.Append("error: ").Append(error).Append('\n');
        sb.Append('\n').Append(Trim(resources, 4000)).Append("\n\n").Append(Trim(scene, 8000));
        return Content(!string.IsNullOrEmpty(error) && scene.Length < 8, sb.ToString().TrimEnd(), png);
    }

    static string Trim(string s, int max) => string.IsNullOrEmpty(s) ? "" : s.Length <= max ? s : s[..max] + "\n…";

    static JsonObject Content(bool error, string text, byte[] png) {
        var content = new JsonArray { new JsonObject { ["type"] = "text", ["text"] = text ?? "" } };
        if (png != null && png.Length > 0 && png.Length < 1_500_000)
            content.Add(new JsonObject { ["type"] = "image", ["data"] = Convert.ToBase64String(png), ["mimeType"] = "image/png" });
        var result = new JsonObject { ["content"] = content };
        if (error) result["isError"] = true;
        return result;
    }

    static string Save(byte[] png, string name) {
        var path = Path.Combine(Dir(), name);
        File.WriteAllBytes(path, png);
        return path;
    }

    static string Dir() {
        var d = Path.Combine(Path.GetTempPath(), "gamex-mirage");
        Directory.CreateDirectory(d);
        return d;
    }

    static Family FamilyOf(string id) {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("family is required");
        return FamilyManager.GetFamily(id);
    }

    static FamilyGame GameOf(Family family, string gameId, string uri) {
        var id = gameId;
        if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(uri)) {
            var hash = uri.LastIndexOf('#');
            if (hash >= 0 && hash + 1 < uri.Length) id = uri[(hash + 1)..];
        }
        return string.IsNullOrEmpty(id) ? null : family.GetGame(id, out _);
    }

    static string UriOf(string uri, string gameId) {
        if (!string.IsNullOrEmpty(uri)) return uri;
        if (string.IsNullOrEmpty(gameId)) throw new ArgumentException("uri or game is required");
        return $"game:/#{gameId}";
    }

    static string RendererType(string type, object asset) {
        var mapped = Fits(Map(type), asset);
        if (mapped != null) return mapped;
        if (asset is IHaveMetaInfo info) {
            try {
                var nodes = info.GetInfoNodes();
                var content = nodes?.Select(n => n.Tag as MetaContent).FirstOrDefault(c => c != null);
                mapped = Fits(Map(content?.Type), asset);
                if (mapped != null) return mapped;
            }
            catch { /* the asset can still be a texture without a meta tree */ }
        }
        if (asset is GfxSpriteSheet) return "SpriteSheet";
        if (asset is ITexture) return "Texture";
        if (asset is GameX.Arkane.Formats.Danae.Binary_Ftl or GameX.Gamebryo.Formats.Binary_Nif) return "Model";
        if (asset is GameX.Arkane.Formats.Danae.Binary_Fts or GameX.ID.Formats.Binary_BspX) return "Level";
        return null;
    }

    // SpriteSheet is a GfxSpriteSheet. Level is an FTS or an idTech BSP, Model is an FTL or a NIF, Texture is an ITexture, Object is a registered builder. Anything else falls through.
    static string Fits(string renderer, object asset) => renderer switch {
        null => null,
        "SpriteSheet" => asset is GfxSpriteSheet ? renderer : null,
        "Texture" => asset is ITexture ? renderer : null,
        "Model" => asset is GameX.Arkane.Formats.Danae.Binary_Ftl or GameX.Gamebryo.Formats.Binary_Nif ? renderer : null,
        "Level" => asset is GameX.Arkane.Formats.Danae.Binary_Fts or GameX.ID.Formats.Binary_BspX ? renderer : null,
        "Object" => asset != null && OpenX.Gfx.Mirage.MirageX.BuildersByType.ContainsKey(asset.GetType()) ? renderer : null,
        _ => renderer,
    };

    static bool MissingBuilder(string message) => message != null && message.Contains("No Mirage builder registered", StringComparison.Ordinal);

    static bool Describe(object asset) {
        if (asset is not IHaveMetaInfo info) return false;
        var node = Line(MirageEngine.Tree.Root, asset.ToString());
        try {
            var nodes = info.GetInfoNodes();
            if (nodes != null) foreach (var n in nodes) AddInfo(node, n);
        }
        catch { /* the asset line is already in the tree */ }
        return true;
    }

    static void AddInfo(Node parent, MetaInfo info) {
        if (info == null) return;
        var node = parent;
        if (!string.IsNullOrEmpty(info.Name)) node = Line(parent, info.Name);
        else if (info.Tag is MetaContent content && content.Value is string text && text.Length > 0) node = Line(parent, text);
        foreach (var child in info.Items) AddInfo(node, child);
    }

    static Node Line(Node parent, string text) {
        var line = string.IsNullOrEmpty(text) ? "meta" : text.Replace('\r', ' ').Replace('\n', ' ');
        MirageEngine.Tree.Log(line);
        return parent.AddChild(line);
    }

    static string Map(string type) => type switch {
        "Texture" or "VideoTexture" or "Image" => "Texture",
        "Sprite" => "Sprite",
        "SpriteSheet" => "SpriteSheet",
        "Model" or "Mesh" => "Model",
        "Level" => "Level",
        "Object" => "Object",
        "Material" => "Material",
        "Audio" => "Audio",
        "TestTri" => "TestTri",
        _ => null,
    };
}
