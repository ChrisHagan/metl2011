@echo off
echo make sure you've already "published" from within VS2008.
echo
echo (make sure that when you published, you changed:
echo SandRibbon - Properties - Publish -
echo "Installation Folder URL"
echo to: http://metl.adm.monash.edu/SpecialMeTL/
echo Updates -
echo "Update Location"
echo to: http://metl.adm.monash.edu/SpecialMeTL/
echo Options -
echo Product name:
echo Special MeTL
echo Suite name:
echo MeTL Tools
echo
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Clickonce on the server (drawkward.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\Application Files" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/SpecialMeTL/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\index.html" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/SpecialMeTL/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\MeTL.application" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/SpecialMeTL/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\setup.exe" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/SpecialMeTL/
