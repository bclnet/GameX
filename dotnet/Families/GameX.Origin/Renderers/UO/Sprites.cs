using GameX.Eng;
using GameX.Xbox.Formats;
using GameX.Xbox.Renderers;
using System.Threading.Tasks;

namespace GameX.Origin.Renderers.UO;

public static class Fonts<Texture2D> {
    public static SpriteFont<Texture2D> Regular;
    public static SpriteFont<Texture2D> Bold;
    public static SpriteFont<Texture2D> Map1;
    public static SpriteFont<Texture2D> Map2;
    public static SpriteFont<Texture2D> Map3;
    public static SpriteFont<Texture2D> Map4;
    public static SpriteFont<Texture2D> Map5;
    public static SpriteFont<Texture2D> Map6;

    public static async Task Load(PakFile game, GraphicsDevice device) {
        Regular = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/regular_font.xnb"));
        //Bold = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/bold_font.xnb"));
        //Map1 = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/map1_font.xnb"));
        //Map2 = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/map2_font.xnb"));
        //Map3 = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/map3_font.xnb"));
        //Map4 = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/map4_font.xnb"));
        //Map5 = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/map5_font.xnb"));
        //Map6 = SpriteFont<Texture2D>.Create(device, await game.LoadFileObject<Binary_Xnb>("fonts/map6_font.xnb"));
    }
}