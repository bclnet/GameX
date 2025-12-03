from gamex import family

def test_haversine():
    # get Black family
    family = family.getFamily('Black')
    print(f'studio: {family.studio}')

    # get arc with resource
    res = family.parseResource('game:/MASTER.DAT#Fallout')
    pakFile1 = family.openArchive(res)
    print(f'arc: {pakFile1}')

    # get arc with game:/uri
    pakFile2 = family.openArchive('game:/MASTER.DAT#Fallout')
    print(f'arc: {pakFile2}')
    # Amsterdam to Berlin

    assert family


# # get Black family
# family = FamilyManager.getFamily('Arkane')
# print(f'studio: {family.studio}')

# # get arc with game:/uri
# archive = family.openArchive('game:/#AF')
# print(f'{archive}')