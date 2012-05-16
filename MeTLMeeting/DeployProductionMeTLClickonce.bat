@echo off
echo make sure you've already "published" from within Visual Studio.
echo
echo (make sure that when you published, you changed:
echo SandRibbon - Properties - Publish -
echo "Installation Folder URL"
echo to: http://metl.adm.monash.edu/MeTL2011/
echo Updates -
echo "Update Location"
echo to: http://metl.adm.monash.edu/MeTL2011/
echo Options -
echo Product name:
echo MeTL Presenter
echo Suite name:
echo {leave blank}
echo
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Clickonce on the server (metl.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause

set BUILDDIR=..\..\BuildMeTL

rem Steps for production roll
rem update manifest using buildmetl updatemanifest batch
rem plink copy all files from refer ProdTesting to refer MeTL2011 making a backup on the way
rem pscp the updated manifest to refer MeTL2011 overwriting the old ProdTesting manifest 

pscp.exe -pw bananaman deploy@refer.adm.monash.edu.au:"/srv/racecarDeploy/MeTLProdTesting/MeTL\ Presenter.application" "MeTL Presenter.application"
call %BUILDDIR%\updatemanifest.bat
pscp.exe -pw bananaman "MeTL Presenter.application" deploy@refer.adm.monash.edu.au:"/srv/racecarDeploy/MeTLProdTesting/MeTL\ Presenter.application"

