docker build --platform linux/arm64 -t webshot .

echo上传docker中
"C:\Program Files\Git\bin\bash.exe" -c "docker save webshot | ssh root@192.168.100.1 'docker load && cd /mnt/share && docker compose up -d webshot'"
pause