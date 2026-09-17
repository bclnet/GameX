from gamex import family

# get family
family = family.getFamily('Black')
print(f'studio: {family.studio}')

file = ('game:/MASTER.DAT#Fallout', 'COLOR.PAL')
# file = ('game:/master.dat#Fallout2', 'art/backgrnd/BACK1.FRM')

# get arc with game:/uri
archive = family.getArchive(file[0])
sample = archive.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'arc: {archive}, {sample}')

# get file
data = archive.getData(sample)
print(f'dat: {data}')