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

docker run -d --name mobot -v /root/MoBotLog:/app/logs -v /root/StreamVideo:/app/StreamVideo mobot

# 模版
DI注入可以获取如下接口：

ILogger

IDataStorage

(如下这两个没什么用)

IMoBotClient

IBotSocketClient