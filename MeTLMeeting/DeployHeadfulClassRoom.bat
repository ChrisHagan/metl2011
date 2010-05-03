@echo off
echo Don't use this command unless you're ready to overwrite 
echo the currently deployed HeadfulClassroom on the server (drawkward.adm) 
echo Press Ctrl+C to exit this.  Any other keypress will continue
pause
pscp.exe -r -pw deploy "C:\sandRibbon\HeadfulClassRoom\publish\Application Files" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLHeadfulClassroom/
pscp.exe -pw deploy "C:\sandRibbon\HeadfulClassRoom\publish\index.html" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLHeadfulClassroom/
pscp.exe -pw deploy "C:\sandRibbon\HeadfulClassRoom\publish\HeadlessClassRoom.application" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLHeadfulClassroom/
pscp.exe -pw deploy "C:\sandRibbon\HeadfulClassRoom\publish\setup.exe" deploy@drawkward.adm.monash.edu.au:/srv/www/MeTLHeadfulClassroom/
