from gamex import Archive
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
    async def load(game: Archive, device: object):
        Fonts.regular = await game.getAsset(Binary_Xnb, 'fonts/regular_font.xnb').obj
        Fonts.bold = await game.getAsset(Binary_Xnb, 'fonts/bold_font.xnb').obj
        Fonts.map1 = await game.getAsset(Binary_Xnb, 'fonts/map1_font.xnb').obj
        Fonts.map2 = await game.getAsset(Binary_Xnb, 'fonts/map2_font.xnb').obj
        Fonts.map3 = await game.getAsset(Binary_Xnb, 'fonts/map3_font.xnb').obj
        Fonts.map4 = await game.getAsset(Binary_Xnb, 'fonts/map4_font.xnb').obj
        Fonts.map5 = await game.getAsset(Binary_Xnb, 'fonts/map5_font.xnb').obj
        Fonts.map6 = await game.getAsset(Binary_Xnb, 'fonts/map6_font.xnb').obj