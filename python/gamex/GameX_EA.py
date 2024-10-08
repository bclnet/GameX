import os
from gamex.pak import BinaryPakFile
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class state: pass
class FileSource: pass
class FileOption: pass

# EpicPakFile
class EpicPakFile(BinaryPakFile):
    def __init__(self, state: state):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        pass

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, callable):
        match _pathExtension(source.path).lower():
            case _: return (0, None)
    #endregion