@echo off
set /p num="0£¨»ò¿Õ£©mobot£¬1 mobot_test£º"
set /p public="0£¨»ò¿Õ£©Release£¬ 1 Debug"

if "%public%"=="" goto Release
if "%public%"=="0" goto Release
if "%public%"=="1" goto Debug

:Debug
dotnet publish -c Debug -r linux-arm64 --self-contained false -o ./publish

:Release
dotnet publish -c Release -r linux-arm64 --self-contained false -o ./publish

if "%num%"=="0" goto Mobot_Test
if "%num%"=="" goto Mobot_Test
if "%num%"=="1" goto Mobot

:Mobot_Test
docker build --platform linux/arm64 -t mobot_test .

:Mobot
docker build --platform linux/arm64 -t mobot .

pause