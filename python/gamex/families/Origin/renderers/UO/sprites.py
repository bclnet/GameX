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
        Fonts.regular = game.loadFileObject(Binary_Xnb, 'fonts/regular_font.xnb').objs[0]
        Fonts.bold = game.loadFileObject(Binary_Xnb, 'fonts/bold_font.xnb').objs[0]
        Fonts.map1 = game.loadFileObject(Binary_Xnb, 'fonts/map1_font.xnb').objs[0]
        Fonts.map2 = game.loadFileObject(Binary_Xnb, 'fonts/map2_font.xnb').objs[0]
        Fonts.map3 = game.loadFileObject(Binary_Xnb, 'fonts/map3_font.xnb').objs[0]
        Fonts.map4 = game.loadFileObject(Binary_Xnb, 'fonts/map4_font.xnb').objs[0]
        Fonts.map5 = game.loadFileObject(Binary_Xnb, 'fonts/map5_font.xnb').objs[0]
        Fonts.map6 = game.loadFileObject(Binary_Xnb, 'fonts/map6_font.xnb').objs[0]