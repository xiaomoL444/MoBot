# MoBot

沫沫的Bot

给自己写代码的规定：
Public变量小写开头，后面大写
私有变量下划线小写
局部变量小写后大写

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