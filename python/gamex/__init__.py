from ._config import __title__, __version__, option, familyKeys
from .core.desser import *
from .core.family import *
from .core.meta import *
from .core.pak import *
# from .core.platform import PlatformX
# from .core.util import _value

init()

unknown = getFamily('Unknown')
unknownPakFile = unknown.openPakFile('game:/#APP', False)
