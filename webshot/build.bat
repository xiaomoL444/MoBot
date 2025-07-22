docker build --platform linux/arm64 -t webshot .
docker save -o ../docker/webshot.rar webshot:latest
pause