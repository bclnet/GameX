from gamex import family

# get family
family = family.getFamily('Valve')
print(f'studio: {family.studio}')

file = ('game:/#HL', 'COLOR.PAL')

# get arc with game:/uri
archive = family.openArchive(file[0])
sample = archive.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'arc: {archive}, {sample}')

# get file
data = archive.getData(sample)
print(f'dat: {data}')