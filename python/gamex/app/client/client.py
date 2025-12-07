import sys
from dataclasses import dataclass
from openstk import log
from openstk.core.client import IClientHost
from gamex import getFamily, FamilyGame, option
from gamex.core.client import ClientState
from openstk.platforms.client_ex import ExClientHost

@dataclass
class RunArgs:
    args: list[str]
    platform: str
    family: str
    game: str
    edition: str

def main(args2: list[str]) -> int:
    args = RunArgs(args2, option.Platform, option.Family, option.Game, option.Edition)
    return run(args)

clientHost: IClientHost

# factory
def _createClient(family: str, uri: str, args: list[str], tag: object) -> callable: 
    archive = getFamily(family).getArchive(uri)
    return lambda: archive.game.getClient(ClientState(archive, args, tag))

def _createClientHost(platform: str, client: callable) -> IClientHost:
    match platform:
        case 'GL': return ExClientHost(client)
        case _: raise Exception(f'platform OutOfRange {platform}')

def run(args: RunArgs) -> int:
    log.trace('Running game...')
    with _createClientHost(args.platform, _createClient(args.family, FamilyGame.toUri(args.game, args.edition), args.args, None)) as host: clientHost = host; clientHost.run()
    log.trace('Exiting game...')
    return 0

if __name__ == "__main__":
    sys.exit(main([]))