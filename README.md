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

docker run -d --name mobot -v /mnt/mmc0-4/MoBot/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot/config:/app/config -v /mnt/mmc0-4/MoBot/data:/app/data -v /mnt/mmc0-4/MoBot/cache:/app/cache --dns 192.168.100.1 mobot

docker run -d --name mobot -v C:/Code/MoBot/docker/logs:/app/logs -v C:/Code/MoBot/docker/videos:/app/videos -v C:/Code/MoBot/docker/configs:/app/configs -v C:/Code/MoBot/docker/data:/app/data mobot



# 模版

DI注入可以获取如下接口：

ILogger

IDataStorage

(如下这两个没什么用)

IMoBotClient

IBotSocketClient

# 项目

## MoBot主项目

MoBot.Core MoBot的定义，装着接口，模型的定义，以及消息序列化行为（我不清楚我为什么要放在core里面，忘记了）

MoBot.Handle MoBot定义的视线，包括IDataStorage的Json储存实现，扩展函数（如group.IsGroup），消息发送处理，以及Bot的网络处理，Bot的实例

MoBot.Shared MoBot的共享库，目前没有自己写的代码，只有引用了NuGet包

MoBot MoBot的主程序

MoBot.Test MoBot的测试程序，因为不会写单元测试，所以用来直接做一个拷贝主程序调用控制台输入查看是否调用正确了

## 沫沫自己的模块程序

BilibiliLive bilibili直播模块，让Bot帮忙调用ffmpeg直播,srt-live-transmit会一直开启，主程序退出了也是（）还没打算修

DailyChat 关键词回复模块，我在做完了才想起这种功能叫关键词回复，所以当时命名的时候起了一个每日聊天（）其实可能本意就是聊天的时候发的消息吧

DailyChatSettingWinForm 不太会起名字，这个是用来修改DailyChat的配置文件的程序

DailyPoems 每日古诗，因为有很好的朋友建议说做一个每日夸夸功能，但是我没找到夸夸的api，也觉得自己可能写不好，思来想去换成定时发送古诗好了

# UNKNOW

docker run --privileged -v /dev:/dev -v /mnt/mmc0-4/MoBot\_test/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot\_test/configs:/app/configs -it mobot\_test

docker run  -v C:/Code/MoBot/docker/logs:/app/logs -v C:/Code/MoBot/docker/data:/app/data -v C:/Code/MoBot/docker/videos:/app/videos -v C:/Code/MoBot/docker/configs:/app/configs mobot -it mobot

docker run -d --name mobot\_test --privileged -v /mnt/mmc0-4/MoBot\_test/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot\_test/configs:/app/configs mobot\_test

docker run -v /mnt/mmc0-4/MoBot\_test/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot\_test/configs:/app/configs -it mobot\_test

docker run -d --name mobot -v /mnt/mmc0-4/MoBot/logs:/app/logs -v /mnt/mmc0-4/MoBot/videos:/app/videos -v /mnt/mmc0-4/MoBot/configs:/app/configs -v /mnt/mmc0-4/MoBot/data:/app/data mobot

h264\_v4l2m2m
mpegts
ffmpeg -re -i testvideo.mp4 -c:v libx264 -b:v 2000k -f mpegts udp://192.168.5.11:1111
ffmpeg -re -i testvideo.mp4 -c:v h264\_v4l2m2m -b:v 8000k -f mpegts udp://192.168.5.11:1111

ffmpeg -re -i testvideo.mp4 -c:v h264\_v4l2m2m -b:v 2000k -c:a aac -b:a 128k -f flv "rtmp://live-push.bilivideo.com/live-bvc/?streamname=live\_609872107\_38653366\&key=7aa98cc05cbfe4e878e3dbb821cfd13d\&schedule=rtmp\&pflag=1"



ffmpeg -re  -i testvideo.mp4 -c:v h264\_v4l2m2m -preset ultrafast -crf 51 -b:v 200k -c:a aac -b:a 128k -pix\_fmt yuv420p -bufsize 6000k -an -f flv "rtmp://live-push.bilivideo.com/live-bvc/?streamname=live\_609872107\_38653366\&key=7aa98cc05cbfe4e878e3dbb821cfd13d\&schedule=rtmp\&pflag=1"

ffmpeg -fflags +genpts -err\_detect ignore\_err -i udp://127.0.0.1:12345 -c copy -f flv "rtmp://live-push.bilivideo.com/live-bvc/?streamname=live\_609872107\_38653366\&key=7aa98cc05cbfe4e878e3dbb821cfd13d\&schedule=rtmp\&pflag=1"

ffmpeg -re -stream\_loop -1 -fflags +genpts -i "C:\\Code\\YT-dlp download video\\reencoded\\017 . video\_16\_fixed.mp4" -f mpegts -c copy -mpegts\_flags +initial\_discontinuity -muxpreload 0 -muxdelay 0  udp://127.0.0.1:12345

