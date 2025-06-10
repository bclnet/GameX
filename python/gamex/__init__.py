from ._config import __title__, __version__, option, familyKeys
from .desser import *
from .family import *
from .meta import *
from .pak import *
# from .platform import PlatformX
# from .util import _value

init()

unknown = getFamily('Unknown')
unknownPakFile = unknown.openPakFile('game:/#APP', False)
