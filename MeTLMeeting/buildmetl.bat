@echo off

SET build=%1
SET rev=%2
REM Defaults
SET branchname=MeTLOverLib
SET buildconfig=""
SET revision=
SHIFT & SHIFT

IF "%rev%"=="" GOTO INVALIDPARAMS
IF "%build%"=="prod" (
	SET buildconfig=Release
)
IF "%build%"=="staging" (
	SET buildconfig=Debug
)
IF "%buildconfig%"=="" GOTO INVALIDPARAMS

REM Default option is to publish
SET buildtargets=Clean;Build;Publish

:LOOP
IF NOT "%1"=="" (
	IF "%1"=="-nopublish" (
		SET buildtargets=Clean;Build
		REM SHIFT
	)
	IF "%1"=="-workingdir" (
		SET skipupdate=1
		REM SHIFT
	)
	IF "%1"=="-noremote" (
		SET skippull=1
		REM SHIFT
	)
	IF "%1"=="-branch" (
		SET branchname=%2
		SHIFT
	)
	IF "%1"=="-rev" (
		SET revision=-r %2
		SHIFT
	)
	SHIFT
	GOTO LOOP
)

echo.
echo Building Configuration=%buildconfig% with ApplicationRevision=%rev%.

IF DEFINED skippull GOTO UPDATE

:PULL
echo.
echo Grabbing latest from source control
hg pull

IF %errorlevel% NEQ 0 GOTO ERROR

:UPDATE
IF DEFINED skipupdate GOTO BUILD
echo.
echo Changing to branch %branchname%
hg update %revision% -C %branchname% 

IF %errorlevel% NEQ 0 GOTO ERROR

:BUILD
echo Building...
echo.

CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

msbuild.exe MeTL.sln /l:FileLogger,Microsoft.Build.Engine;logfile=MeTLBuildLog.log /p:Configuration=%buildconfig% /p:Platform="Any CPU" /p:ApplicationRevision=%rev% /t:%buildtargets%
IF %errorlevel% NEQ 0 GOTO ERROR

:SUCCESS
echo.
echo Done.
GOTO :EOF

:INVALIDPARAMS
echo BuildScript Help v0.5
echo.
echo buildmetl staging OR prod build# [-branch name] [-nopublish] [-workingdir] [-noremote]
echo.
echo Default branch is MeTLOverLib.
echo.
echo The following example will create a staging build with the version 
echo number 289 using the default branch: 
echo.
echo buildmetl staging 289
echo.
echo -branch			Update to the specified branch name.
echo.
echo -nopublish			Clean and build the target only.
echo.
echo -workingdir		Build using the working directory.
echo.
echo -noremote  		Do not update from source control.
echo.
echo.
GOTO :EOF

:ERROR
echo There was an error with the build.
