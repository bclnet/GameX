from __future__ import annotations

# Database
class Database:
    archive = None
    palettes: dict[str, object] = {}

    @staticmethod
    def loaded(game) -> None:
        Database.archive = game.family.getArchive('game:/#P2')

    @staticmethod
    def getPalette(path: str, defaultValue: str):
        key = path or ''
        if key in Database.palettes: return Database.palettes[key]
        from openx.core import runSync
        from gamex.families.Uncore.formats.binary import Binary_Pal
        name = f'{key}.PAL' if len(key) > 0 and Database.archive.contains(f'{key}.PAL') else f'{defaultValue}.PAL'
        pal = runSync(Database.archive.getAsset(Binary_Pal, name)).convertVgaPalette()
        Database.palettes[key] = pal
        return pal
