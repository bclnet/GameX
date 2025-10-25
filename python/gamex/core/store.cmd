@echo off
echo build Blizzard.proto
protoc -I ..\..\Context\Gamespec.Core\StoreManagers --python_out=. Blizzard.proto