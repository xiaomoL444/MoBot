# 使用 Microsoft 官方 .NET 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0
# 设置工作目录
WORKDIR /app

# 将你发布后的文件复制到容器中
COPY ./publish/ .

# 设置容器启动时运行的命令
ENTRYPOINT ["dotnet", "MoBot.dll"]
