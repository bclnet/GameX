using GameX.Black.Formats;
using OpenX;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.GameObject;

namespace GameX.Engines.Unity;

/// <summary>
/// A Fallout MAP becomes an isometric tilemap. One floor and one roof per elevation.
/// </summary>
public static class UnityMapObjectBuilder {
    public static async Task<Object> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        var map = (Binary_Map)path;
        var name = string.IsNullOrEmpty(map.Name) ? "map" : map.Name;
        var gobj = new Object(name) { isStatic = isStatic };
        var grid = gobj.AddComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Isometric;
        grid.cellSize = new Vector3(1f, 0.45f, 1f);
        var textureManager = materialManager.TextureManager;
        var lines = await TileLines(source);
        var tiles = new Dictionary<ushort, Tile>();
        var active = 0;
        for (var i = 0; i < map.Levels.Length; i++)
            if (map.Levels[i].Elevation == map.EnteringElevation) { active = i; break; }
        for (var i = 0; i < map.Levels.Length; i++) {
            var level = map.Levels[i];
            var on = i == active;
            await AddLayer(gobj, $"Floor{level.Elevation}", level.Floor, lines, tiles, source, textureManager, 0, on);
            await AddLayer(gobj, $"Roof{level.Elevation}", level.Roof, lines, tiles, source, textureManager, 1, on);
        }
        return gobj;
    }

    static async Task AddLayer(Object root, string name, ushort[] cells, string[] lines, Dictionary<ushort, Tile> tiles, ISource source, TextureManager<Texture2D> textureManager, int order, bool active) {
        var child = new Object(name);
        child.transform.SetParent(root.transform, false);
        var tilemap = child.AddComponent<Tilemap>();
        var renderer = child.AddComponent<TilemapRenderer>();
        renderer.mode = TilemapRenderer.Mode.Individual;
        renderer.sortingOrder = order;
        for (var i = 0; i < cells.Length; i++) {
            var id = cells[i];
            if (id <= 1) continue;
            if (!tiles.TryGetValue(id, out var tile)) tiles[id] = tile = await LoadTile(source, textureManager, lines, id);
            if (tile == null) continue;
            tilemap.SetTile(new Vector3Int(i % 100, 99 - (i / 100), 0), tile);
        }
        child.SetActive(active);
    }

    static async Task<string[]> TileLines(ISource source) {
        const string path = "art/tiles/tiles.lst";
        if (source is not Archive arc || !arc.Contains(path)) return null;
        var data = await source.GetAsset<Stream>(path);
        if (data == null) return null;
        using (data) {
            using var reader = new StreamReader(data, Encoding.ASCII);
            var lines = new List<string>();
            while (reader.ReadLine() is { } line) lines.Add(line);
            return [.. lines];
        }
    }

    static async Task<Tile> LoadTile(ISource source, TextureManager<Texture2D> textureManager, string[] lines, ushort id) {
        if (lines == null || id >= lines.Length) return null;
        var frm = lines[id].Trim().Replace('\\', '/');
        if (frm.Length == 0) return null;
        if (string.IsNullOrEmpty(Path.GetExtension(frm))) frm += ".frm";
        var full = "art/tiles/" + frm;
        try {
            if (source is Archive arc && !arc.Contains(full)) return null;
            textureManager.Preload(source, full);
            var (_, tag) = await textureManager.Create(source, full);
            if (tag is not ITexture it || it.Width <= 0 || it.Height <= 0) return null;
            var bytes = it.Create<byte[]>("UN", o => o is TextureAsBytes b ? b.Bytes : null);
            if (bytes == null) return null;
            var tex = new Texture2D(it.Width, it.Height, UnityEngine.TextureFormat.RGBA32, false) { name = frm };
            tex.LoadRawTextureData(bytes);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, it.Width, it.Height), new Vector2(0.5f, 0f), 80f);
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            return tile;
        }
        catch (Exception) { return null; }
    }
}
