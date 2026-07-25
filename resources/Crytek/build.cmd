@echo off

pushd FarCry2/FarCry2-File-Lists
7z a -r ../../../../python/gamex/resources/Crytek/FarCry2.zip -ir!*.filelist*
popd
pushd FarCry2
7z a -r ../../../python/gamex/resources/Crytek/FarCry2.zip binary_classes.xml
popd

pushd FarCry3/FarCry3-File-Lists
7z a -r ../../../../python/gamex/resources/Crytek/FarCry3.zip -ir!*.filelist*
popd

pushd FarCry5/FarCry5-File-Lists\pc
7z a -r ../../../../../python/gamex/resources/Crytek/FarCry5.zip -ir!*.filelist*
popd

pushd FarCry6/FarCry6-File-Lists\pc
7z a -r ../../../../../python/gamex/resources/Crytek/FarCry6.zip -ir!*.filelist*
popd

pushd FarCryNewDawn/FarCryNewDawn-File-Lists\pc
7z a -r ../../../../../python/gamex/resources/Crytek/FarCryNewDawn.zip -ir!*.filelist*
popd

pushd FarCryPrimal/FarCryPrimal-File-Lists
7z a -r ../../../../python/gamex/resources/Crytek/FarCryPrimal.zip -ir!*.filelist*
popd