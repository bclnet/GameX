from __future__ import annotations
import asyncio
from typing import TYPE_CHECKING, Any, Optional, cast
from argparse import ArgumentParser, _SubParsersAction
from pydantic import BaseModel
from .. import getFamily, PlatformX, FileSystem, FileOption
from ..Base.app import ExportManager

class ProgramState:
    @staticmethod
    def load(func: callable, defaultValue: object):
        try:
            if os.path.exists('./lastChunk.txt'):
                with open('./lastChunk.txt', 'r') as s:
                    return func(s.read())
        except: pass
        return defaultValue

    @staticmethod
    def store(func: callable):
        try:
            data = func()
            with open('./lastChunk.txt', 'w') as s:
                s.write(data)
        except: clear()

    @staticmethod
    def clear():
        try:
            if os.path.exists('./lastChunk.txt'): os.remove('./lastChunk.txt')
        except: pass

def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    sub = subparser.add_parser("get", help="get files contents")
    sub.add_argument("-f", "--family", type=str, required=True, help="Family")
    sub.add_argument("-u", "--uri", type=str, required=True, help="Uri to extract")
    sub.add_argument("-m", "--match", type=str, help="Match")
    sub.add_argument("-o", "--option", type=str, default="Default", help="Option")
    sub.add_argument("-p", "--path", type=str, default="./out", help="Output folder")
    sub.set_defaults(func=get, args_model=CLIExportArgs)

class CLIExportArgs(BaseModel):
    family: str
    uri: str
    path: Optional[str] = None
    match: Optional[str] = None
    option: Optional[str] = None

@staticmethod
def get(args: CLIExportArgs) -> None:
    args.option = FileOption[args.option]
    from_ = ProgramState.load(lambda x: x, 0)
    
    # get family
    family = getFamily(args.family)
    if not family: print(f'No family found named "{args.family}".'); return 0

    # get resource
    res = family.parseResource(args.uri)
    path = PlatformX.decodePath(args.path)
    match = FileSystem.createMatcher(args.match) if args.match else None

    # export
    asyncio.run(ExportManager.exportAsync(family, res, path, match, from_, args.option))

    ProgramState.clear()