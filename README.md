# MoBot

沫沫的Bot

给自己写代码的规定：
Public变量小写开头，后面大写
私有变量下划线小写
局部变量小写后大写

# Docker 编译

VERSION=$(git rev-parse --short HEAD)

dotnet publish -c Release -r linux-arm64 --self-contained false -o publish

docker build --platform linux/arm64 -t mobot .

docker run -d --name mobot --privileged -v /mnt/mmc0-4/MoBot/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot/configs:/app/configs mobot


docker run -d --name mobot -v C:/Code/MoBot/docker/logs:/app/logs -v C:/Code/MoBot/docker/videos:/app/videos -v C:/Code/MoBot/docker/configs:/app/configs mobot


# 模版
DI注入可以获取如下接口：

ILogger

IDataStorage

(如下这两个没什么用)

IMoBotClient

IBotSocketClient



docker run --privileged -v /dev:/dev -v /mnt/mmc0-4/MoBot_test/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot_test/configs:/app/configs -it mobot_test

docker run  -v C:/Code/MoBot/docker/logs:/app/logs -v C:/Code/MoBot/docker/videos:/app/videos -v C:/Code/MoBot/docker/configs:/app/configs mobot -it mobot

docker run -d --name mobot_test --privileged -v /mnt/mmc0-4/MoBot_test/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot_test/configs:/app/configs mobot_test

docker run -v /mnt/mmc0-4/MoBot_test/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot_test/configs:/app/configs -it mobot_test

h264_v4l2m2m
mpegts
 ffmpeg -re -i testvideo.mp4 -c:v libx264 -b:v 2000k -f mpegts udp://192.168.5.11:1111
 ffmpeg -re -i testvideo.mp4 -c:v h264_v4l2m2m -b:v 8000k -f mpegts udp://192.168.5.11:1111

ffmpeg -re -i testvideo.mp4 -c:v h264_v4l2m2m -b:v 2000k -c:a aac -b:a 128k -f flv "rtmp://live-push.bilivideo.com/live-bvc/?streamname=live_609872107_38653366&key=7aa98cc05cbfe4e878e3dbb821cfd13d&schedule=rtmp&pflag=1"



ffmpeg -re  -i testvideo.mp4 -c:v h264_v4l2m2m -preset ultrafast -crf 51 -b:v 200k -c:a aac -b:a 128k -pix_fmt yuv420p -bufsize 6000k -an -f flv "rtmp://live-push.bilivideo.com/live-bvc/?streamname=live_609872107_38653366&key=7aa98cc05cbfe4e878e3dbb821cfd13d&schedule=rtmp&pflag=1"

ffmpeg -fflags +genpts -err_detect ignore_err -i udp://127.0.0.1:12345 -c copy -f flv "rtmp://live-push.bilivideo.com/live-bvc/?streamname=live_609872107_38653366&key=7aa98cc05cbfe4e878e3dbb821cfd13d&schedule=rtmp&pflag=1"

ffmpeg -re -stream_loop -1 -fflags +genpts -i "C:\Code\YT-dlp download video\reencoded\017 . video_16_fixed.mp4" -f mpegts -c copy -mpegts_flags +initial_discontinuity -muxpreload 0 -muxdelay 0  udp://127.0.0.1:12345