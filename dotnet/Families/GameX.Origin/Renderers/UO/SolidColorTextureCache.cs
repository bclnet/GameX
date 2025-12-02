using GameX.Eng;
using System.Collections.Generic;
using System.Drawing;

namespace GameX.Origin.Renderers.UO;

public interface ITexture2D { }

public static class SolidColorTextureCache {
    static readonly Dictionary<Color, ITexture2D> Textures = [];
    static GraphicsDevice Device;
    public static void Load(GraphicsDevice device) => Device = device;

    public static ITexture2D GetTexture(Color color) {
        if (Textures.TryGetValue(color, out var texture)) return texture;
        //texture = new Texture2D(_device, 1, 1, false, SurfaceFormat.Color);
        //texture.SetData(new[] { color });
        //Textures[color] = texture;
        return texture;
    }
}