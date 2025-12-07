from gamex import family

# get family
family = family.getFamily('Arkane')
print(f'studio: {family.studio}')

file = ('game:/#AF', 'sample:0')
# file = ('game:/master.index#D2', 'strings/english_m.lang')

# get arc with game:/uri
archive = family.getArchive(file[0])
sample = archive.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'arc: {archive}, {sample}')

# get file
data = archive.getData(sample)
print(f'dat: {data}')