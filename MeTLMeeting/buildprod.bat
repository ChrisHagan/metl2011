@echo off
echo **********************************
echo * MeTL Production BuildScript
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
IF /I "%2"=="nopublish" GOTO NOPUBLISH ELSE GOTO PUBLISH

:PUBLISH
msbuild.exe MeTL.sln /l:FileLogger,Microsoft.Build.Engine;logfile=MeTLBuildLog.log /p:Configuration=Release /p:Platform="Any CPU" /p:ApplicationRevision=%1 /t:Clean;Build;Publish
GOTO SUCCESS

:NOPUBLISH
msbuild.exe MeTL.sln /l:FileLogger,Microsoft.Build.Engine;logfile=MeTLBuildLog.log /p:Configuration=Release /p:Platform="Any CPU" /p:ApplicationRevision=%1 /t:Clean;Build
GOTO SUCCESS

:SUCCESS
echo.
echo Done.
GOTO :EOF

:ERROR
echo BuildScript Help v0.1b
echo.
echo %0 [version number] [nopublish]
echo.
