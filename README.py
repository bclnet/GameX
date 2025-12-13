import os
from gamex import Families

def writeFile(path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

def gamesBody():
    def single(s, value): return value if s and value in s else '-'
    def platform(s, value):
        values = [i[len(value) + 1:].split('/') for i in s if i.startswith(value)] if s else []
        values = values[0] if len(values) > 0 else values
        gl = 'gl' if 'GL' in values else '--'
        un = 'un' if 'UN' in values else '--'
        ur = 'ur' if 'UR' in values else '--'
        vk = 'vk' if 'VK' in values else '--'
        return f'{gl} {un} {ur}'
    b = ['''---
The following are the current games:\n
| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
''']
    for f in Families.values():
        print(f.id)
        b.append(f'| **{f.id}** | **{f.name}**\n')
        for g in [x for x in f.games.values()]:
            b.append(f'| [{g.id}]({g.urls[0] if g.urls else ''}) | {g.name} | {single(g.status, "open")} | {single(g.status, "read")} | {platform(g.status, "texture")} | {platform(g.status, "model")} | {platform(g.status, "level")}\n')
    return ''.join(b)
# body = gamesBody()
# print(body)
#writeFile('README.md', '## Games\n', body)