@echo off
echo make sure you've already "published" from within VS2008.
echo
echo (make sure that when you published, you changed:
echo SandRibbon - Properties - Publish -
echo "Installation Folder URL"
echo to: http://metl.adm.monash.edu/MeTLPresenter/
echo Updates -
echo "Update Location"
echo to: http://metl.adm.monash.edu/MeTLPresenter/
echo Options -
echo Product name:
echo MeTL Presenter
echo Suite name:
echo {leave blank}
echo
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Clickonce on the server (drawkward.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\Application Files" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/MeTLPresenter/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\index.html" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/MeTLPresenter/
pscp.exe -r -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\office2007piaredist" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/MeTLPresenter/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\MeTL Presenter.application" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/MeTLPresenter/
pscp.exe -pw bananaman "C:\specialMeTL\MeTLMeeting\SandRibbon\publish\setup.exe" deploy@reviver.adm.monash.edu.au:/srv/racecarDeploy/MeTLPresenter/
