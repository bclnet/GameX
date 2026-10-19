@echo off
setlocal EnableExtensions

rem Repo root is three levels above engines\Unreal\Shell3D.
pushd "%~dp0..\..\.."
set "ROOT=%CD%"
popd

set "BRIDGE=%ROOT%\OpenX\dotnet\Engines\Unreal\CliBridge\GameX.Unreal.CliBridge.vcxproj"
set "LOADER=%ROOT%\OpenX\dotnet\Engines\Unreal\CliBridge\GameX.Unreal.CliLoader.vcxproj"
set "OUT=%ROOT%\OpenX\dotnet\Engines\Unreal\CliBridge\bin\x64\Debug"
set "DEST=%~dp0Plugins\GameXUnrealBridge\Binaries\Win64"

set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSBUILD%" call :FindMsBuild
if not exist "%MSBUILD%" (
    echo Visual Studio 2022 or newer with the C++ desktop workload is required.
    exit /b 1
)

"%MSBUILD%" "%BRIDGE%" /nologo /v:m /p:Configuration=Debug /p:Platform=x64
if errorlevel 1 exit /b 1
"%MSBUILD%" "%LOADER%" /nologo /v:m /p:Configuration=Debug /p:Platform=x64
if errorlevel 1 exit /b 1

if not exist "%OUT%\GameX.Unreal.CliLoader.exe" (
    echo GameX.Unreal.CliLoader.exe is missing under %OUT%
    exit /b 1
)

pushd "%OUT%"
GameX.Unreal.CliLoader.exe
set "LOADER_EXIT=%ERRORLEVEL%"
popd
if not "%LOADER_EXIT%"=="0" (
    echo GameX.Unreal.CliLoader.exe exited %LOADER_EXIT%.
    exit /b %LOADER_EXIT%
)

if not exist "%DEST%" mkdir "%DEST%"
call :CopyIfExists GameX.Unreal.CliBridge.dll
call :CopyIfExists GameX.Unreal.CliBridge.pdb
rem MSBuild writes Ijwhost.dll. Windows treats that as ijwhost.dll.
call :CopyIfExists Ijwhost.dll
call :CopyIfExists GameX.Unreal.CliBridge.runtimeconfig.json
call :CopyIfExists GameX.Unreal.CliBridge.deps.json

if not exist "%DEST%\GameX.Unreal.CliBridge.dll" (
    echo Missing GameX.Unreal.CliBridge.dll. The mixed DLL will not load in Unreal without ijwhost beside it.
    exit /b 1
)
if not exist "%DEST%\Ijwhost.dll" (
    echo Missing ijwhost.dll. The mixed DLL will not load in Unreal without ijwhost beside it.
    exit /b 1
)

echo %DEST%
echo GameX C++/CLI
exit /b 0

:CopyIfExists
if exist "%OUT%\%~1" copy /Y "%OUT%\%~1" "%DEST%\%~1" >nul
exit /b 0

:FindMsBuild
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" set "VSWHERE=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" exit /b 0
for /f "usebackq delims=" %%I in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"`) do set "MSBUILD=%%I"
exit /b 0
