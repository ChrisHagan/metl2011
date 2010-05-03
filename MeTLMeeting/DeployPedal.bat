@echo off
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed Pedal on the server (drawkward.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw deploy "C:\sandRibbon\Pedal\Pedal\publish\Application Files" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLContentInjection/
pscp.exe -pw deploy "C:\sandRibbon\Pedal\Pedal\publish\index.html" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLContentInjection/
pscp.exe -pw deploy "C:\sandRibbon\Pedal\Pedal\publish\Pedal.application" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLContentInjection/
pscp.exe -pw deploy "C:\sandRibbon\Pedal\Pedal\publish\setup.exe" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLContentInjection/
