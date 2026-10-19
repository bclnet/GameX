using OpenX;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using static OpenX.CellManager;

namespace GameX.Bethesda;

/// <summary>
/// Morrowind LAND rules. 65 samples, cumulative heights times 8, 16 by 16 textures, index minus 1.
/// </summary>
public static class TesCells {
    public const int Side = 65;
    public const int HeightScale = 8;
    public const int TextureRows = 16;
    public const int TextureColumns = 16;
    public const int SampleDivisor = Side - 1;
    public const string DefaultTexturePath = "textures/_land_default.dds";

    public static uint[] TextureIndexs(ILand land) => land.Vtex ?? new uint[256];

    public static void DecodeHeights(ILand land, float[,] dest) {
        var heights = land.Heights;
        var rowOffset = land.HeightOffset;
        for (var y = 0; y < Side; y++) {
            rowOffset += heights[y * Side];
            dest[y, 0] = rowOffset * HeightScale;
            var colOffset = rowOffset;
            for (var x = 1; x < Side; x++) {
                colOffset += heights[(y * Side) + x];
                dest[y, x] = colOffset * HeightScale;
            }
        }
    }

    public static int LayerKey(uint index) => (int)index - 1;

    public static int AlphaKey(uint[] indexs, int x, int y) {
        var yMajor = y / 4; var yMinor = y - (yMajor * 4);
        var xMajor = x / 4; var xMinor = x - (xMajor * 4);
        return (int)indexs[(yMajor * 64) + (xMajor * 16) + (yMinor * 4) + xMinor] - 1;
    }

    public static string TexturePath(IQuery query, int key) =>
        key >= 0 ? query.FindLtex(key).Path : DefaultTexturePath;
}

/// <summary>
/// Draws a Morrowind LAND through an engine's terrain slot.
/// </summary>
public class BethesdaCellBuilder<Object, Material, Texture, Shader>(ISource source, IQuery query, IOpenGfx[] gfx) : CellBuilder<Object, Material, Texture, Shader>(source, query, gfx) {
    static readonly Dictionary<Texture, GfxTerrainLayer<Texture>> TerrainLayers = [];

    /// <summary>
    /// Creates terrain representing a LAND record.
    /// </summary>
    protected override async Task CreateLand(ILand land, Object parent) {
        var side = TesCells.Side;
        var rows = TesCells.TextureRows;
        var cols = TesCells.TextureColumns;
        var heights = land.Heights;
        if (heights == null) return;

        // Read in the heights in Morrowind units.
        var newHeights = new float[side, side];
        TesCells.DecodeHeights(land, newHeights);

        // Change the heights to percentages.
        newHeights.GetExtrema(out var minHeight, out var maxHeight);
        for (var y = 0; y < side; y++)
            for (var x = 0; x < side; x++)
                newHeights[y, x] = System.Polyfill.ChangeRange(newHeights[y, x], minHeight, maxHeight, 0f, 1f);

        // Texture the terrain.
        var textureManager = GfxModel.TextureManager;
        var indexs = TesCells.TextureIndexs(land); var layers = new List<GfxTerrainLayer<Texture>>(); var layerIndexs = new Dictionary<int, int>();
        for (var i = 0; i < indexs.Length; i++) {
            var index = TesCells.LayerKey(indexs[i]);
            if (layerIndexs.ContainsKey(index)) continue;
            var path = TesCells.TexturePath(Query, index);
            var (tex, _) = await GfxModel.CreateTexture(Source, path);
            if (!TerrainLayers.TryGetValue(tex, out var layer)) {
                layer = new GfxTerrainLayer<Texture> {
                    Texture = tex,
                    Smoothness = .3f,
                    Metallic = .2f,
                    Specular = Color.Black,
                    TileSize = new Vector2(6, 6)
                };
                layer.MaskMapTexture = textureManager.CreateSolid(1, 1, [layer.Metallic, 0f, 0f, layer.Smoothness]);
                layer.NormalMapTexture = textureManager.CreateNormalMap(tex);
                TerrainLayers[tex] = layer;
            }
            var layerIndex = layers.Count; layers.Add(layer); layerIndexs.Add(index, layerIndex);
        }
        var newLayers = layers.ToArray();

        // Create the alpha map.
        var alphaMap = new float[rows, cols, newLayers.Length];
        for (var y = 0; y < rows; y++)
            for (var x = 0; x < cols; x++) {
                var texIndex = TesCells.AlphaKey(indexs, x, y);
                alphaMap[y, x, texIndex >= 0 ? layerIndexs[texIndex] : 0] = 1;
            }

        // Create the terrain.
        var heightRange = (maxHeight - minHeight) / MeterInUnits;
        var position = new Vector3(land.GridId.X * CellLengthInMeters, land.GridId.Y * CellLengthInMeters, minHeight / MeterInUnits);
        var sampleDistance = CellLengthInMeters / TesCells.SampleDivisor;
        var terrainData = GfxTerrain.CreateData(0, newHeights, heightRange, sampleDistance, newLayers, alphaMap);
        GfxTerrain.Create("terrain", position, terrainData, parent);
    }
}
