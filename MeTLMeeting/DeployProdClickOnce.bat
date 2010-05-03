@echo off
echo make sure you've already "published" from within VS2008.
echo
echo (make sure that when you published, you changed:
echo Make sure that when you published you had your chosen mode set to Release and not Debug or else the prod clickonce will point to the staging server
echo SandRibbon - Properties - Publish -
echo "Installation Folder URL"
echo to: http://drawkward.adm.monash.edu/MeTL/
echo Updates -
echo "Update Location"
echo to: http://drawkward.adm.monash.edu/MeTL/
echo Options -
echo Product name:
echo MeTL
echo
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Clickonce on the server (drawkward.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw deploy "C:\sandRibbon\SandRibbon\publish\Application Files" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTL/
pscp.exe -pw deploy "c:\sandRibbon\SandRibbon\publish\index.html" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTL/
pscp.exe -pw deploy "c:\sandRibbon\SandRibbon\publish\MeTL.application" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTL/
pscp.exe -pw deploy "c:\sandRibbon\SandRibbon\publish\setup.exe" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTL/
