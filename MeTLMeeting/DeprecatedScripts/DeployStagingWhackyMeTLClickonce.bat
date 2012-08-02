@echo off
echo make sure you've already "published" from within VS2010.
echo
echo (make sure that when you published, you changed:
echo SandRibbon - Properties - Publish -
echo "Installation Folder URL"
echo to: http://metl.adm.monash.edu/whackyMeTLStaging/
echo Updates -
echo "Update Location"
echo to: http://metl.adm.monash.edu/whackyMeTLStaging/
echo Options -
echo Product name:
echo WhackyMeTL
echo Suite name:
echo {leave blank}
echo
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Clickonce on the server (metl.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\Application Files" deploy@refer.adm.monash.edu.au:/srv/racecarDeploy/whackyMeTLStaging/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\index.html" deploy@refer.adm.monash.edu.au:/srv/racecarDeploy/whackyMeTLStaging/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\WhackyMeTL.application" deploy@refer.adm.monash.edu.au:/srv/racecarDeploy/whackyMeTLStaging/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\setup.exe" deploy@refer.adm.monash.edu.au:/srv/racecarDeploy/whackyMeTLStaging/
