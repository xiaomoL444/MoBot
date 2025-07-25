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

#下载依赖
RUN apt-get update && apt-get install -y \
  ca-certificates \
  fonts-liberation \
  libasound2 \
  libatk-bridge2.0-0 \
  libatk1.0-0 \
  libc6 \
  libcairo2 \
  libcups2 \
  libdbus-1-3 \
  libexpat1 \
  libfontconfig1 \
  libgbm1 \
  libgcc1 \
  libglib2.0-0 \
  libgtk-3-0 \
  libnspr4 \
  libnss3 \
  libpango-1.0-0 \
  libpangocairo-1.0-0 \
  libstdc++6 \
  libx11-6 \
  libx11-xcb1 \
  libxcb1 \
  libxcomposite1 \
  libxcursor1 \
  libxdamage1 \
  libxext6 \
  libxfixes3 \
  libxi6 \
  libxrandr2 \
  libxrender1 \
  libxss1 \
  libxtst6 \
  lsb-release \
  wget \
  xdg-utils && \
  rm -rf /var/lib/apt/lists/*

# 设置工作目录
WORKDIR /app

# 将你发布后的文件复制到容器中
COPY ./publish/ .

# 设置容器启动时运行的命令
ENTRYPOINT ["dotnet", "MoBot.dll"]
