# 使用 Microsoft 官方 .NET 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0

ENV TZ=Asia/Shanghai
RUN apt-get update && apt-get install -y ffmpeg tzdata && rm -rf /var/lib/apt/lists/* && \
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && \
    echo $TZ > /etc/timezone

#设置chrome
#RUN apt-get update && apt-get install -y curl && \
#    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
#    apt-get install -y nodejs && \
#    npm install -g playwright && \
#    playwright install chromium && \
#    npx playwright install-deps

# 设置工作目录
WORKDIR /app

# 将你发布后的文件复制到容器中
COPY ./publish/ .
