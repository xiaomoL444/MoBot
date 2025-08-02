@echo off
set /p num="0����գ�mobot��1 mobot_test��"
set /p public="0����գ�Release�� 1 Debug"

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyy-MM-dd-HH-mm-ss"') do set TAG=%%i

if "%num%"=="1" (
	set IMAGE_NAME=mobot:test
	set TAG_NAME=mobot:test
	set DOCKER_COMPOSE_NAME=mobot_test
) else (
	set IMAGE_NAME=mobot:%TAG%
	set TAG_NAME=mobot:latest
	set DOCKER_COMPOSE_NAME=mobot
)

if "%public%"=="1" (
	set PUBLICH=Debug
) else (
	set PUBLICH=Release
)

echo ���ڹ���%IMAGE_NAME%��%PUBLICH%�汾
dotnet publish -c %PUBLICH% -r linux-arm64 --self-contained false -o ./publish

echo ���ڴ��docker��...
docker build --platform linux/arm64 -t %IMAGE_NAME% .

echo �����ϴ�������...
"C:\Program Files\Git\bin\bash.exe" -c "docker save %IMAGE_NAME% | ssh root@192.168.100.1 'docker load && docker tag %IMAGE_NAME% %TAG_NAME% && cd /mnt/share && docker compose up -d %DOCKER_COMPOSE_NAME%'"

pause