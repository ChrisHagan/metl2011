@echo off
echo **********************************
echo * Building MeTL Production
echo **********************************
echo Setting ApplicationRevision to "%1"
echo.

IF "%1"=="" GOTO ERROR

CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
msbuild.exe MeTL.sln /p:Configuration=Release /p:Platform="Any CPU" /p:ApplicationRevision=%1 /t:Clean;Publish
GOTO END

:ERROR
echo BuildScript Help v0.1b
echo.
echo "%0" <version number>

:END
