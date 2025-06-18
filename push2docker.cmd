@echo off
set /p num="0����գ�mobot_test��1 mobot��"
set /p public="0����գ�Debug�� 1 Release"

if "%public%"=="" goto Debug
if "%public%"=="0" goto Debug
if "%public%"=="1" goto Release

:Debug
dotnet publish -c Debug -r linux-arm64 --self-contained false -o publish

:Release
dotnet publish -c Release -r linux-arm64 --self-contained false -o publish

if "%num%"=="0" goto Mobot_Test
if "%num%"=="" goto Mobot_Test
if "%num%"=="1" goto Mobot

:Mobot_Test
docker build --platform linux/arm64 -t mobot_test .

:Mobot
docker build --platform linux/arm64 -t mobot .

pause