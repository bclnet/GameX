import sys
from dataclasses import dataclass
from openstk import debug
from gamex import getFamily
from gamex.core.client import GameController

@dataclass
class RunArgs:
    family: str
    uri: str

def main() -> int:
    args = RunArgs('Origin', 'game:/#UO')
    return run(args)

game: GameController

def run(args: RunArgs) -> int:
    pluginHost = None

    # get family
    family = getFamily(args.family)
    if not family: print(f'No family found named "{args.family}".'); return 0

    # get game
    game = family.openPakFile(args.uri)
    if not game: print(f'No game found named "{args.Uri}".'); return 0

    debug.trace('Running game...')
    with GameController(pluginHost) as g:
        game = g
        game.run()
    debug.trace('Exiting game...')
    return 0

if __name__ == "__main__":
    sys.exit(main())