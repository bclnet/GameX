using GameX.Eng;
using GameX.Xbox.Formats;
using GameX.Xbox.Formats.Xna;
using System.Threading.Tasks;

namespace GameX.Origin.Renderers.UO;

public static class Fonts<Texture2D> {
    public static SpriteFont Regular;
    public static SpriteFont Bold;
    public static SpriteFont Map1;
    public static SpriteFont Map2;
    public static SpriteFont Map3;
    public static SpriteFont Map4;
    public static SpriteFont Map5;
    public static SpriteFont Map6;

    public static async Task Load(Archive game, GraphicsDevice device) {
        Regular = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/regular_font.xnb")).Obj;
        Bold = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/bold_font.xnb")).Obj;
        Map1 = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/map1_font.xnb")).Obj;
        Map2 = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/map2_font.xnb")).Obj;
        Map3 = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/map3_font.xnb")).Obj;
        Map4 = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/map4_font.xnb")).Obj;
        Map5 = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/map5_font.xnb")).Obj;
        Map6 = (SpriteFont)(await game.GetAsset<Binary_Xnb>("fonts/map6_font.xnb")).Obj;
    }
}