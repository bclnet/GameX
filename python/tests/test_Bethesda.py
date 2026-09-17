from gamex import family

# get family
family = family.getFamily('Bethesda')
print(f'studio: {family.studio}')

file = ('game:/Morrowind.esm#Morrowind', 'sample:0')
# file = ('game:/Oblivion - Meshes.bsa#Oblivion', 'GRAPH/particles/DEFAULT.jpg')
# file = ('game:/Fallout - Meshes.bsa#Fallout3', 'strings/english_m.lang')
# file = ('game:/Fallout4 - Meshes.ba2#Fallout4', 'Meshes/Marker_Error.NIF')

# get arc with game:/uri
archive = family.getArchive(file[0])
sample = archive.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'arc: {archive}, {sample}')

# get file
data = archive.getData(sample)
print(f'dat: {data}')