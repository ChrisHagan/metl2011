@echo off

IF "%3"==""     GOTO err_arg_cnt
IF NOT "%4"=="" GOTO err_arg_cnt

IF NOT EXIST %1 GOTO err_file_not_exist
IF NOT EXIST %2 GOTO err_file_not_exist
IF NOT EXIST %3 GOTO err_file_not_exist

SET my=%1
SET orig=%2
SET your=%3
SET merged=%my%.hgmerge

IF EXIST %merged% (del /q %merged%)
diff3 -L my -L orig -L your -E -m %my% %orig% %your% > %merged%

IF ERRORLEVEL 1 (
    echo C %my%
    move /y %merged% %my%
    EXIT 1
) ELSE (
    echo M %my%
    move /y %merged% %my%
)
EXIT 0

:err_arg_cnt
  echo Wrong arg count!
  echo You run:
  echo ^> %0 %*
  echo Must:
  echo ^> %0 my orig your
EXIT 1

:err_file_not_exist
  echo One of '%*' files not exist.
EXIT 1