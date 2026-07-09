@echo off

pushd FarCry2-File-Lists
7z a -r ../../../python/gamex/resources/Crytek/FarCry2.zip -ir!*.filelist*
popd

pushd FarCry5-File-Lists\pc
7z a -r ../../../../python/gamex/resources/Crytek/FarCry5.zip -ir!*.filelist*
popd

pushd FarCry6-File-Lists\pc
7z a -r ../../../../python/gamex/resources/Crytek/FarCry6.zip -ir!*.filelist*
popd

pushd FarCryNewDawn-File-Lists\pc
7z a -r ../../../../python/gamex/resources/Crytek/FarCryNewDawn.zip -ir!*.filelist*
popd

pushd FarCryPrimal-File-Lists
7z a -r ../../../python/gamex/resources/Crytek/FarCryPrimal.zip -ir!*.filelist*
popd