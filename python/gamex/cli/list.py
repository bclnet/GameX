from __future__ import annotations
import os
from typing import TYPE_CHECKING, Any, Optional, cast
from argparse import ArgumentParser, _SubParsersAction
from pydantic import BaseModel
from collections import Counter
from .. import families, getFamily

def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    sub = subparser.add_parser("list", help="list files contents")
    # optional
    sub.add_argument("-f", "--family", type=str, help="Family")
    sub.add_argument("-u", "--uri", type=str, help="Uri to list")
    sub.set_defaults(func=list, args_model=CLIListArgs)

class CLIListArgs(BaseModel):
    family: Optional[str] = None
    uri: Optional[str] = None

@staticmethod
def list(args: CLIListArgs) -> None:
    # list families
    if not args.family:
        print('Families installed:')
        for id,val in families.items(): print(f'  {id} - {val.name}')
        return 1
    
    # get family
    family = getFamily(args.family, False)
    if not family: print(f'No family found named "{args.family}".'); return 0
    
    # list found paths in family
    if not args.uri:
        print(f'{family.id}')
        print(f'  name: {family.name}')
        print(f'  description: {family.description}')
        print(f'  studio: {family.studio}')
        print('\nGames:')
        for game in family.games.values():
            print(f'{game.name}')
            if not game.found: continue
            print(f'  urls: {', '.join(game.paks)}')
            print(f'  root: {game.found.root}')
        return
    
    # list files in pack for family
    else:
        print(f'{family.name} - {args.uri}')
        with family.openPakFile(args.uri) as s:
            # if s.count == 0: print('Nothing found.'); return
            for p in sorted(s.files, key=lambda x: x.path):
                print(f'{p.path}')
                pak = p.pak
                if not pak: continue
                pak.open()
                exts = Counter([os.path.splitext(x.path)[1] for x in s.files])
                for x in exts:
                    print(f'  {x}: {exts[x]}')