from gamex import family

# get family
family = family.getFamily('Bullfrog')
print(f'studio: {family.studio}')

# file = ('game:/#P', 'sample:0')
# file = ('game:/#P2', 'sample:0')
file = ('game:/#S', 'sample:0')
# file = ('game:/#MC', 'sample:0')
# file = ('game:/#TP', 'sample:0')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
sample = pakFile.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'pak: {pakFile}, {sample}')

# get file
# data = pakFile.loadFileData(sample)
# print(f'dat: {data}')

# get file
obj = pakFile.loadFileObject(object, sample)
print(f'obj: {obj}')