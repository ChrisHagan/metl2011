@echo off
echo **********************************
echo * MeTL BuildScript
echo **********************************

SET build=%1
SET revision=%2
SHIFT & SHIFT

IF %revision%=="" GOTO ERROR
IF %build%=="prod" (
	SET buildconfig="Release"
)
IF %build%=="staging" (
	SET buildconfig="Debug"
)
IF NOT DEFINED %buildconfig% GOTO ERROR

REM Default option is to publish
SET buildtargets=Clean;Build;Publish

:LOOP
IF NOT "%1"=="" (
	IF "%1"=="-skippublish" (
		SET buildtargets=Clean;Build
		REM SHIFT
	)
	IF "%1"=="-skipupdate" (
		SET skipupdate=1
		REM SHIFT
	)
	IF "%1"=="-skippull" (
		SET skippull=1
		REM SHIFT
	)
	SHIFT
	GOTO LOOP
)

echo.
echo Building Configuration=%buildconfig% with ApplicationRevision=%revision%.

IF DEFINED %skippull% GOTO UPDATE

:PULL
echo.
echo Grabbing latest from source control
hg pull

IF DEFINED %skipupdate% GOTO BUILD

:UPDATE
echo.
echo Updating to last changeset 
hg update -C

:BUILD
echo Building...
echo.

CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

echo msbuild.exe MeTL.sln /l:FileLogger,Microsoft.Build.Engine;logfile=MeTLBuildLog.log /p:Configuration=%buildconfig% /p:Platform="Any CPU" /p:ApplicationRevision=%1 /t:%buildtargets%
GOTO SUCCESS

:SUCCESS
echo.
echo Done.
GOTO :EOF

:ERROR
echo BuildScript Help v0.3b
echo.
echo %0 staging|prod revision [-skippublish] [-skipupdate] [-skippull]
echo.
echo The following example will build a staging version with the revision number 289: 
echo %0 staging 289
echo.
echo -skippublish		Clean and build the target only.
echo.
echo -skipupdate		Build using the working directory.
echo.
echo -skippull			Do not update from source control.
echo.
echo.
