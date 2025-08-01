# 选择 nginx 官方镜像作为基础镜像
FROM nginx:alpine

RUN apt update && apt install -y socat && \
    rm -rf /var/lib/apt/lists/*

# 删除默认的 nginx 网站文件
RUN rm -rf /usr/share/nginx/html/*

COPY ./nginx.conf /etc/nginx/conf.d/default.conf

# 将本地的 dist 文件夹复制到 nginx 默认网页目录
COPY dist/ /usr/share/nginx/html/

# 启动 nginx（镜像默认命令）
CMD ["nginx", "-g", "daemon off;"]