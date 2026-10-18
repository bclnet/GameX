from __future__ import annotations
import os
from decimal import Decimal
from zipfile import ZipFile

# S.Event
class Event:
    def __init__(self, frame: int, music: Decimal, sound: Decimal, subtitle: str = None):
        self.frame = frame
        self.music = music
        self.sound = sound
        self.subtitle = subtitle

_files = None
_events: dict[str, list] = {}

def _zipFiles() -> dict[str, object]:
    global _files
    if _files == None:
        arc = ZipFile(os.path.join(os.path.dirname(__file__), 'S.zip'), 'r')
        _files = { os.path.basename(info.filename): info for info in arc.infolist() if not info.is_dir() }
        _zipFiles.arc = arc
    return _files

# Keyed by the zip entry name. A missing path is cached as None.
def getEvents(path: str):
    if path in _events: return _events[path]
    entry = _zipFiles().get(path)
    if entry == None:
        _events[path] = None
        return None
    value = []
    with _zipFiles.arc.open(entry) as raw:
        text = raw.read().decode('utf-8-sig')
    for line in text.splitlines():
        p = line.split(' ')
        evnt = Event(int(p[0]), Decimal(-1), Decimal(-1))
        for i in range(1, len(p)):
            part = p[i]
            if part[0] == '#': pass
            # decimal.Parse already includes the sign; the unary minus flips it back.
            elif part[0] == '-': evnt.music = -Decimal(part)
            elif part[0].isdigit(): evnt.sound = Decimal(part)
            else: evnt.subtitle = part
        value.append(evnt)
    _events[path] = value
    return value
