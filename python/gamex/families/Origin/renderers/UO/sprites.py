from gamex import PakFile
from gamex.families.Xbox.formats.binary import Binary_Xnb

# Fonts
class Fonts:
    regular = None
    bold = None
    map1 = None
    map2 = None
    map3 = None
    map4 = None
    map5 = None
    map6 = None

    @staticmethod
    def load(game: PakFile, device: object):
        data = game.loadFileObject(Binary_Xnb, 'fonts/regular_font.xnb')
        print(data)
        #Regular = SpriteFont<Texture2D>.Create(device, game.LoadFileObject("fonts/regular_font.xnb"))
        #Bold = SpriteFont<Texture2D>.Create(device, "fonts/bold_font.xnb")
        #Map1 = SpriteFont<Texture2D>.Create(device, "fonts/map1_font.xnb")
        #Map2 = SpriteFont<Texture2D>.Create(device, "fonts/map2_font.xnb")
        #Map3 = SpriteFont<Texture2D>.Create(device, "fonts/map3_font.xnb")
        #Map4 = SpriteFont<Texture2D>.Create(device, "fonts/map4_font.xnb")
        #Map5 = SpriteFont<Texture2D>.Create(device, "fonts/map5_font.xnb")
        #Map6 = SpriteFont<Texture2D>.Create(device, "fonts/map6_font.xnb")