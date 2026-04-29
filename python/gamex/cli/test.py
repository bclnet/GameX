from __future__ import annotations
from typing import TYPE_CHECKING, Any, Optional, cast
import asyncio
from argparse import ArgumentParser, _SubParsersAction
from pydantic import BaseModel

def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    sub = subparser.add_parser("test", help="test")
    # required
    sub.add_argument("-f", "--family", type=str, default="Bethesda", help="Family")
    sub.add_argument("-u", "--uri", type=str, default="game:/Morrowind.bsa#Morrowind", help="Arc file to be extracted")
    def func(args: CLITestArgs) -> None: asyncio.run(testAsync(args))
    sub.set_defaults(func=func, args_model=CLITestArgs)

class CLITestArgs(BaseModel):
    family: str
    uri: str

@staticmethod
async def testAsync(args: CLITestArgs) -> None:
    print(args)