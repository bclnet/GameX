using OpenX;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using static OpenX.CellManager;

namespace GameX.Origin;

/// <summary>
/// Ultima Online land rules. 64 absolute height samples, texture id used raw, path <c>tex{id}</c>.
/// </summary>
public static class UltimaCells {
    public const int Side = 64;
    public const int TextureRows = Side;
    public const int TextureColumns = Side;
    public const int SampleDivisor = Side;

    public static uint[] TextureIndexs(ILand land) => land.Vtex ?? new uint[Side * Side];

    public static void DecodeHeights(ILand land, float[,] dest) {
        var heights = land.Heights;
        for (var y = 0; y < Side; y++)
            for (var x = 0; x < Side; x++)
                dest[y, x] = heights[(y * Side) + x];
    }

    public static int LayerKey(uint index) => (int)index;
    public static int AlphaKey(uint[] indexs, int x, int y) => (int)indexs[(y * Side) + x];
    public static string TexturePath(int key) => $"tex{key}";
}

/// <summary>
/// Draws an Ultima Online land cell through an engine's terrain slot.
/// </summary>
public class UltimaCellBuilder<Object, Material, Texture, Shader>(ISource source, IQuery query, IOpenGfx[] gfx) : CellBuilder<Object, Material, Texture, Shader>(source, query, gfx) {
    static readonly Dictionary<Texture, GfxTerrainLayer<Texture>> TerrainLayers = [];

    protected override async Task CreateLand(ILand land, Object parent) {
        var side = UltimaCells.Side;
        var rows = UltimaCells.TextureRows;
        var cols = UltimaCells.TextureColumns;
        var heights = land.Heights;
        if (heights == null) return;

        var newHeights = new float[side, side];
        UltimaCells.DecodeHeights(land, newHeights);

        newHeights.GetExtrema(out var minHeight, out var maxHeight);
        for (var y = 0; y < side; y++)
            for (var x = 0; x < side; x++)
                newHeights[y, x] = System.Polyfill.ChangeRange(newHeights[y, x], minHeight, maxHeight, 0f, 1f);

        var textureManager = GfxModel.TextureManager;
        var indexs = UltimaCells.TextureIndexs(land); var layers = new List<GfxTerrainLayer<Texture>>(); var layerIndexs = new Dictionary<int, int>();
        for (var i = 0; i < indexs.Length; i++) {
            var index = UltimaCells.LayerKey(indexs[i]);
            if (layerIndexs.ContainsKey(index)) continue;
            var path = UltimaCells.TexturePath(index);
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

        var alphaMap = new float[rows, cols, newLayers.Length];
        for (var y = 0; y < rows; y++)
            for (var x = 0; x < cols; x++) {
                var texIndex = UltimaCells.AlphaKey(indexs, x, y);
                alphaMap[y, x, layerIndexs[texIndex]] = 1;
            }

        var heightRange = (maxHeight - minHeight) / MeterInUnits;
        var position = new Vector3(land.GridId.X * CellLengthInMeters, land.GridId.Y * CellLengthInMeters, minHeight / MeterInUnits);
        var sampleDistance = CellLengthInMeters / UltimaCells.SampleDivisor;
        var terrainData = GfxTerrain.CreateData(0, newHeights, heightRange, sampleDistance, newLayers, alphaMap);
        GfxTerrain.Create("terrain", position, terrainData, parent);
    }
}
