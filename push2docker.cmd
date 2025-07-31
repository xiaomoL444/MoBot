@echo off
set /p num="0（或空）mobot，1 mobot_test："
set /p public="0（或空）Release， 1 Debug"

if "%public%"=="" goto Release
if "%public%"=="0" goto Release
if "%public%"=="1" goto Debug

goto Release

:Debug
echo 选择的是debug
dotnet publish -c Debug -r linux-arm64 --self-contained false -o ./publish
goto select

:Release
echo 选择的是Release
dotnet publish -c Release -r linux-arm64 --self-contained false -o ./publish
goto select

:select
if "%num%"=="0" goto Mobot
if "%num%"=="" goto Mobot
if "%num%"=="1" goto Mobot_Test

:Mobot_Test
docker build -f Dockerfile.dev --platform linux/arm64 -t mobot_test .
pause
exit

:Mobot
docker build --platform linux/arm64 -t mobot .

pause