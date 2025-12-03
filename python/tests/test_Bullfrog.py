from gamex import family

# get family
family = family.getFamily('Bullfrog')
print(f'studio: {family.studio}')

# file = ('game:/#P', 'sample:0')
# file = ('game:/#P2', 'sample:0')
file = ('game:/#S', 'sample:0')
# file = ('game:/#MC', 'sample:0')
# file = ('game:/#TP', 'sample:0')

# get arc with game:/uri
archive = family.openArchive(file[0])
sample = archive.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'arc: {archive}, {sample}')

# get file
# data = archive.getData(sample)
# print(f'dat: {data}')

# get file
obj = archive.getAsset(object, sample)
print(f'obj: {obj}')