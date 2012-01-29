@echo off
echo **********************************
echo * MeTL Staging BuildScript
echo **********************************

IF "%1"=="" GOTO ERROR

echo.
echo Setting ApplicationRevision to %1
echo.
echo Grabbing latest from source control
echo.

hg pull
hg update -C

echo Building...
echo.

CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
msbuild.exe MeTL.sln /l:FileLogger,Microsoft.Build.Engine;logfile=MeTLBuildLog.log /p:Configuration=Debug /p:Platform="Any CPU" /p:ApplicationRevision=%1 /t:Clean;Build;Publish
GOTO :EOF

echo Done.

:ERROR
echo BuildScript Help v0.1b
echo.
echo %0 [version number]
echo.
