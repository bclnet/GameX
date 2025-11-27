using GameX.Xbox.Formats.Xna;
using System;
using System.IO;

namespace GameX.Xbox.Formats.StardewValley.xTile;

public class Map {
}

/// <summary>Tile Reader.</summary>
[RType("xTile.Pipeline.TideReader"), RAssembly("xTile")]
public class TideReader : TypeReader<Map> {
    public override Map Read(ContentReader r, Map o) {
        var data = r.ReadL32Bytes();
        return default;
        //Map map = FormatManager.Instance.BinaryFormat.Load((Stream)new MemoryStream(r.ReadBytes(count)));
        //if (map != null) {
        //    map.assetPath = r.AssetName;
        //    map.FlattenTileSheetPaths();
        //}
        //return map;
    }
}

