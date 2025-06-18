# 使用 Microsoft 官方 .NET 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0

#RUN sed -i 's/deb.debian.org/mirrors.aliyun.com/g' /etc/apt/sources.list && apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

ENV TZ=Asia/Shanghai
RUN apt-get update && \
    apt-get install -y tzdata && \
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && \
    echo $TZ > /etc/timezone

# 设置工作目录
WORKDIR /app

# 将你发布后的文件复制到容器中
COPY ./publish/ .

# 设置容器启动时运行的命令
ENTRYPOINT ["dotnet", "MoBot.Test.dll"]
