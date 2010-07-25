@echo off
echo make sure you've already built from within VS2008.
echo
echo This will deploy to:
echo   https://drawkward.adm.monash.edu.au:1234/WaveInkGadget/SilverlightApplication1.xml
echo
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Gadget on the server (drawkward.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw bananaman "C:\specialMeTL\PressureSensitiveSilverlight\bin\Release\*.*" deploy@drawkward.adm.monash.edu.au:/srv/www/WaveInkGadget/
