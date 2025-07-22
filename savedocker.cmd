@echo off
set /p num="0£¨»ò¿Õ£©mobot£¬1 mobot_test£º"

if "%num%"=="0" goto Mobot
if "%num%"=="" goto Mobot
if "%num%"=="1" goto Mobot_Test

:Mobot_Test
docker save -o docker/mobot_test.rar mobot_test:latest

:Mobot
docker save -o docker/mobot.rar mobot:latest

pause