from gamex import family

# get family
family = family.getFamily('Capcom')
print(f'studio: {family.studio}')

# file = ('game:/re_chunk_000.arc#Arcade', 'File0001.tex')
file = ('game:/arc/pc/game.arc#Fighting:C', 'common/pause_blur.sdl')

# get arc with game:/uri
archive = family.openArchive(file[0])
sample = archive.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'arc: {archive}, {sample}')

# get file
data = archive.getData(sample)
print(f'dat: {data}')