import os
from gamex import Families

def writeFile(path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

def gamesBody():
    def single(stat, key): return key if key in stat else '-'
    def platform(stat, key):
        values = [i[len(key) + 1:].split('/') for i in stat if i.startswith(key)]
        values = values[0] if len(values) > 0 else values
        gl = 'gl' if 'GL' in values else '--'
        un = 'un' if 'UN' in values else '--'
        ur = 'ur' if 'UR' in values else '--'
        return f'{gl} {un} {ur}'
    b = ['''---
The following are the current games:\n
| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
''']
    for f in Families.values():
        # print(f.id)
        b.append(f'| **{f.id}** | **{f.name}**\n')
        for g in [s for s in f.games.values() if s.files]:
            stat = g.status.split(';') if g.status else []
            b.append(f'| [{g.id}]({g.urls[0] if g.urls else ''}) | {g.name} | {single(stat, "open")} | {single(stat, "read")} | {platform(stat, "tex")} | {platform(stat, "model")} | {platform(stat, "level")}\n')
    return ''.join(b)
body = gamesBody()
# print(body)
writeFile('README.md', '## Games\n', body)