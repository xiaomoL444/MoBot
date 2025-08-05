const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    host: '0.0.0.0',          // 允许通过 webshot.lan 或其他 IP 访问
    allowedHosts: 'all',      // 放行所有 Host 请求头，解决 Invalid Host header 报错
    port: 8080                // 可选：指定端口（默认就是 8080）
  }
})
