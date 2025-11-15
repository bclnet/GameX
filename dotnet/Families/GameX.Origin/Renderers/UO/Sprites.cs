using GameX.Eng;
using GameX.Xbox.Formats;
using System.Threading.Tasks;

namespace GameX.Origin.Renderers.UO;

public static class Fonts<Texture2D> {
    public static Binary_Xnb.SpriteFont Regular;
    public static Binary_Xnb.SpriteFont Bold;
    public static Binary_Xnb.SpriteFont Map1;
    public static Binary_Xnb.SpriteFont Map2;
    public static Binary_Xnb.SpriteFont Map3;
    public static Binary_Xnb.SpriteFont Map4;
    public static Binary_Xnb.SpriteFont Map5;
    public static Binary_Xnb.SpriteFont Map6;

    public static async Task Load(PakFile game, GraphicsDevice device) {
        Regular = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/regular_font.xnb")).Obj;
        Bold = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/bold_font.xnb")).Obj;
        Map1 = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/map1_font.xnb")).Obj;
        Map2 = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/map2_font.xnb")).Obj;
        Map3 = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/map3_font.xnb")).Obj;
        Map4 = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/map4_font.xnb")).Obj;
        Map5 = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/map5_font.xnb")).Obj;
        Map6 = (Binary_Xnb.SpriteFont)(await game.LoadFileObject<Binary_Xnb>("fonts/map6_font.xnb")).Obj;
    }
}