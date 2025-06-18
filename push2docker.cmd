dotnet publish -c Release -r linux-arm64 --self-contained false -o publish
docker build --platform linux/arm64 -t mobot .
pause