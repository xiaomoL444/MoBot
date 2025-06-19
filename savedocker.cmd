@echo off
set /p num="0£¨»ò¿Õ£©mobot_test£¬1 mobot£º"

if "%num%"=="0" goto Mobot_Test
if "%num%"=="" goto Mobot_Test
if "%num%"=="1" goto Mobot

:Mobot_Test
docker save -o docker/mobot_test.rar mobot_test:latest

:Mobot
docker save -o docker/mobot.rar mobot:latest

pause