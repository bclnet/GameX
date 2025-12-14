import sys, os, re, qrcode
sys.path.append('..')
from gamex import Families

def writeFile(z, path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

#region build

def getUrl(url):
    if url == '': return ''
    file = f'{url.replace('https://', '').replace('http://', '').replace(':', '').replace('_', '').replace('/', '_').replace('?', '+').replace('.', '').replace('&', '+').replace("'", '+')}.png'
    path = os.path.join('.', 'qrcodes', file)
    if not os.path.exists(path):
        img = qrcode.make(url, box_size=5)
        img.save(path)
    return f'image:qrcodes/{file}[width=100,height=100]'

def escape(s): return s.replace('! ', '\\! ')

def gameFamily(f):
    b = ['\n']
    b.append('[cols="1a"]\n|===\n')
    b.append(f'|{f.id}\n')
    b.append(f'|name: {f.name}\n')
    b.append(f'|studio: {f.studio}\n')
    b.append(f'|description: {f.description}\n')
    b.append(f'|{'\n\n'.join([getUrl(s) for s in f.urls]) if f.urls else None}\n')
    b.append('|===\n')
    b.append(f'\n')
    if f.engines:
        b.append(f'=== List of Engines\n')
        b.append(f'\n')
        b.append('[cols="1,1"]\n|===\n')
        b.append(f'|ID |Name\n')
        for s in f.engines.values(): b.append(f'\n|{s.id}\n|{s.name}\n')
        b.append('|===\n\n')
    if f.games:
        b.append(f'=== List of Games\n')
        b.append(f'\n')
        b.append('[cols="1,3,1,1,1a"]\n|===\n')
        b.append(f'|ID |Name |Date |Exts |Urls\n')
        for s in f.games.values():
            multi = s.key or s.editions or s.dlcs or s.locales or s.detectors or s.files or s.virtuals or s.ignores or s.filters
            engine = f' +\nEngine: {s.engine[0]}{':' + s.engine[1] if s.engine[1] else ''}' if s.engine else ''
            # open game
            b.append('\n')
            if multi: b.append('.2+')
            b.append(f'|{s.id}\n')
            b.append(f'|{escape(s.name)}{engine}\n')
            b.append(f'|{s.date.strftime('%b %d, %Y') if s.date else '-'}\n')
            b.append(f'|{', '.join(s.arcExts) if s.arcExts else '-'}\n')
            b.append(f'|{' +\n'.join([getUrl(x) for x in s.urls]) if s.urls else '-'}\n'); i = 0
            if not multi: continue
            breakat = 24
            def break_(h0, h1):
                b.append('!===\n\n')
                # open game
                b.append(f'\n|{s.id} +\n(cont.)\n4+a|\n')
                b.append(h0)
                b.append(h1)
            b.append('\n')
            b.append(f'4+a|\n')
            # s.key
            if s.keyorig: b.append(f'{s.keyorig}\n')
            # editions
            if s.editions:
                b.append(h0 := '[cols="1,3"]\n!===\n')
                b.append(h1 := f'!Editions !Name\n'); i+=1
                for t in s.editions.values():
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{t.id}\n!{t.name}\n'); i+=1
                b.append('!===\n\n')
            # dlcs
            if s.dlcs:
                b.append(h0 := '[cols="1,2,2"]\n!===\n')
                b.append(h1 := '')
                b.append(h2 := f'!DLCs !Name !Path\n'); i+=1
                for t in s.dlcs.values():
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{t.id}\n!{t.name}\n!{t.path}\n'); i+=1
                b.append('!===\n\n')
            # locales
            if s.locales:
                b.append(h0 := '[cols="1,4"]\n!===\n')
                b.append(h1 := f'!Locales !Name\n'); i+=1
                for t in s.locales.values():
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{t.id}\n!{t.name}\n'); i+=1
                b.append('!===\n\n')
            # detectors
            if s.detectors:
                b.append(h0 := '[cols="1,4"]\n!===\n')
                b.append(h1 := f'!Detectors !Name\n'); i+=1
                for k,v in s.detectors.items():
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{v.id}\n!{v.name}\n'); i+=1
                b.append('!===\n\n')
            # files
            if s.files:
                b.append(h0 := '[cols="1,4"]\n!===\n')
                b.append(h1 := f'!Files !Value\n'); i+=1
                for t in [z.split(':') for z in s.files.keys or []]:
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{t[0]}\n!{escape(t[1])}\n'); i+=1
                if s.files.paths: b.append(f'\n2+!{' +\n'.join(s.files.paths)}\n')
                b.append('!===\n\n')
            # virtuals
            if s.virtuals:
                b.append(h0 := '[cols="1,1"]\n!===\n')
                b.append(h1 := f'!Virtuals !Value\n'); i+=1
                for k,v in s.virtuals.items():
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{k}\n!{v}\n'); i+=1
                b.append('!===\n\n')
            # ignores
            if s.ignores:
                b.append(h0 := '[cols="1"]\n!===\n')
                b.append(h1 := f'!Ignores\n'); i+=1
                for k in s.ignores:
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{k}\n'); i+=1
                b.append('!===\n\n')
            # filters
            if s.filters:
                b.append(h0 := '[cols="1,1"]\n!===\n')
                b.append(h1 := f'!Filters !Value\n'); i+=1
                for k,v in s.filters.items():
                    if i >= breakat: break_(h0, h1); i = 1
                    b.append(f'\n!{k}\n!{v}\n'); i+=1
                b.append('!===\n\n')
        b.append('|===\n\n')
    return ''.join(b)

#endregion

ascBodys = []
for f in Families.values():
    if f.id != 'Arkane': continue
    # print(f.id)
    body = gameFamily(f)
    # print(body)
    writeFile(f, f'families/{f.id}/{f.id}.asc', '=== Family Info\n', body)
    ascBodys.append(f'include::{f.id}/{f.id}.asc[]\n')
body = '\n'.join(ascBodys)
writeFile(f, f'families/families.asc', '---\n', body)