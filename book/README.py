import sys, os, re, qrcode
sys.path.append('..')
from gamex import Families

def writeFile(z, path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

def getUrl(url):
    if url == '': return ''
    file = f'{url.replace(':', '').replace('_', '').replace('/', '_').replace('?', '+').replace('.', '').replace('&', '+').replace("'", '+')}.png'
    path = os.path.join('.', 'qrcodes', file)
    if not os.path.exists(path):
        img = qrcode.make(url, box_size=5)
        img.save(path)
    return f'image:qrcodes/{file}[width=100,height=100]'

def escape(s): return s.replace('!', '_')

def gameFamily(f):
    b = ['\n']
    b.append('[cols="1a"]\n')
    b.append('|===\n')
    b.append(f'|{f.id}\n')
    b.append(f'|name: {f.name}\n')
    b.append(f'|studio: {f.studio}\n')
    b.append(f'|description: {f.description}\n')
    b.append(f'|{'\n\n'.join([getUrl(s) for s in f.urls]) if f.urls else None}\n')
    b.append('|===\n')
    b.append(f'\n')
    if f.engines:
        b.append(f'==== List of Engines\n')
        b.append(f'\n')
        b.append('[cols="1,1"]\n')
        b.append('|===\n')
        b.append(f'|ID |Name\n')
        for s in f.engines.values():
            b.append('\n')
            b.append(f'|{s.id}\n')
            b.append(f'|{s.name}\n')
        b.append('|===\n')
        b.append(f'\n')
    if f.games:
        b.append(f'==== List of Games\n')
        b.append(f'\n')
        b.append('[cols="1,3,1,1,1a"]\n')
        b.append('|===\n')
        b.append(f'|ID |Name |Date |Ext |Url\n')
        for s in f.games.values():
            multi = s.key or s.editions or s.dlcs or s.locales or s.detectors or s.files or s.virtuals or s.ignores or s.filters
            engine = f' +\nEngine: {s.engine[0]}{':' + s.engine[1] if s.engine[1] else ''}' if s.engine else ''
            b.append('\n')
            if multi: b.append('.2+')
            b.append(f'|{s.id}\n')
            b.append(f'|{escape(s.name)}{engine}\n')
            b.append(f'|{s.date.strftime('%b %d, %Y') if s.date else '-'}\n')
            b.append(f'|{', '.join(s.arcExts) if s.arcExts else '-'}\n')
            b.append(f'|{'\n\n'.join([getUrl(x) for x in s.urls]) if s.urls else '-'}\n')
            if multi:
                b.append('\n')
                b.append(f'4+a|\n')
                # s.key
                if s.keyorig: b.append(f'{s.keyorig}\n')
                # editions
                if s.editions:
                    b.append('[cols="1,3"]\n')
                    b.append('!===\n')
                    b.append(f'!Editions !Name\n')
                    for t in s.editions.values():
                        b.append('\n')
                        b.append(f'!{t.id}\n')
                        b.append(f'!{t.name}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # dlcs
                if s.dlcs:
                    b.append('[cols="1,2,2"]\n')
                    b.append('!===\n')
                    b.append(f'!DLCs !Name !Path\n')
                    for t in s.dlcs.values():
                        b.append('\n')
                        b.append(f'!{t.id}\n')
                        b.append(f'!{t.name}\n')
                        b.append(f'!{t.path}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # locales
                if s.locales:
                    b.append('[cols="1,4"]\n')
                    b.append('!===\n')
                    b.append(f'!Locales !Name\n')
                    for t in s.locales.values():
                        b.append('\n')
                        b.append(f'!{t.id}\n')
                        b.append(f'!{t.name}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # detectors
                if s.detectors:
                    b.append('[cols="1,4"]\n')
                    b.append('!===\n')
                    b.append(f'!Detectors !Name\n')
                    for k,v in s.detectors.items():
                        b.append('\n')
                        # b.append(f'!{k}\n')
                        b.append(f'!{v.id}\n')
                        b.append(f'!{v.name}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # files
                if s.files:
                    b.append('[cols="1,4"]\n')
                    b.append('!===\n')
                    b.append(f'!Files !Value\n')
                    if s.files.keys:
                        for t in [z.split(':') for z in s.files.keys]:
                            b.append('\n')
                            b.append(f'!{t[0]}\n')
                            b.append(f'!{escape(t[1])}\n')
                    if s.files.paths:
                        b.append('\n')
                        b.append(f'2+!{' +\n'.join(s.files.paths)}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # virtuals
                if s.virtuals:
                    b.append('[cols="1,1"]\n')
                    b.append('!===\n')
                    b.append(f'!Virtuals !Value\n')
                    for k,v in s.virtuals.items():
                        b.append('\n')
                        b.append(f'!{k}\n')
                        b.append(f'!{v}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # ignores
                if s.ignores:
                    b.append('[cols="1"]\n')
                    b.append('!===\n')
                    b.append(f'!Ignores\n')
                    for k in s.ignores:
                        b.append('\n')
                        b.append(f'!{k}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # filters
                if s.filters:
                    b.append('[cols="1,1"]\n')
                    b.append('!===\n')
                    b.append(f'!Filters !Value\n')
                    for k,v in s.filters.items():
                        b.append('\n')
                        b.append(f'!{k}\n')
                        b.append(f'!{v}\n')
                    b.append('!===\n')
                    b.append(f'\n')

        b.append('|===\n')
        b.append(f'\n')
    return ''.join(b)

ascBodys = []
for f in Families.values():
    if f.id != 'Rockstar': continue
    # print(f.id)
    body = gameFamily(f)
    # print(body)
    writeFile(f, f'families/{f.id}/{f.id}.asc', '==== Family Info\n', body)
    ascBodys.append(f'include::{f.id}/{f.id}.asc[]\n')
body = '\n'.join(ascBodys)
writeFile(f, f'families/families.asc', '---\n', body)