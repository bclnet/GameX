import sys
from dataclasses import dataclass
from openx.core import log
from openx.client import IClientHost
from openx.engines.ex import ExClientHost
from gamex import getFamily, FamilyGame, option
from gamex.core.client import ClientState

@dataclass
class RunArgs:
    args: list[str]
    engine: str
    family: str
    game: str
    edition: str

def main(args2: list[str]) -> int:
    args = RunArgs(args2, option.Engine, option.Family, option.Game, option.Edition)
    return run(args)

clientHost: IClientHost

# factory
def _createClient(family: str, uri: str, args: list[str], tag: object) -> callable: 
    archive = getFamily(family).getArchive(uri)
    return lambda: archive.game.getClient(ClientState(archive, args, tag))

def _createClientHost(engine: str, client: callable) -> IClientHost:
    match engine:
        case 'GL': return ExClientHost(client)
        case _: raise Exception(f'engine OutOfRange {engine}')

def run(args: RunArgs) -> int:
    log.trace('Running game...')
    with _createClientHost(args.engine, _createClient(args.family, FamilyGame.toUri(args.game, args.edition), args.args, None)) as host: clientHost = host; clientHost.run()
    log.trace('Exiting game...')
    return 0

if __name__ == "__main__":
    sys.exit(main([]))