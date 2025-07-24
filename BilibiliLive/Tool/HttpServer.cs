using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BilibiliLive.Tool
{
	/// <summary>
	/// 专门用来和Webshot交互的伺服器（）
	/// </summary>
	public static class HttpServer
	{
		private static ILogger _logger = GlobalLogger.CreateLogger(typeof(HttpServer));
		private static Dictionary<string, string> _contents = new();
		private static object _lock = new();

		public static void Start()
		{

			// 创建一个 HttpListener 实例
			HttpListener listener = new HttpListener();
			// 指定监听的 URL，通常是 "http://localhost:端口号/"
			listener.Prefixes.Add("http://localhost:5416/");
			// 启动监听器
			listener.Start();
			_logger.LogInformation("服务器正在监听 http://localhost:5416/");

			_ = Task.Run(() =>
			{
				for (; ; )
				{
					// 等待接收请求
					HttpListenerContext context = listener.GetContext();
					HttpListenerRequest request = context.Request;
					HttpListenerResponse response = context.Response;

					_logger.LogDebug("收到请求{url}", request.Url);
					string query = request.Url.Query;
					var queryParams = HttpUtility.ParseQueryString(query); // 解析查询字符串为键值对
					lock (_lock)
					{

						//key不存在
						if (!queryParams.AllKeys.Any(q => q == "id"))
						{
							Response(response, HttpStatusCode.BadRequest, "{}");
							continue;
						}
						string id = queryParams["id"];
						if (!_contents.ContainsKey(id))
						{
							_logger.LogError("键不存在！{key}", id);
							Response(response, HttpStatusCode.NotFound, "{}");
							continue;
						}
						Response(response, HttpStatusCode.OK, _contents[id]);
					}
				}
			});
		}
		static void Response(HttpListenerResponse response, HttpStatusCode httpStatus, string message)
		{
			// 设置响应头和状态
			response.ContentType = "text/plain";
			response.StatusCode = (int)httpStatus;

			try
			{

				// 编写响应内容
				string responseMessage = message;
				byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);

				// 写入响应
				response.ContentLength64 = buffer.Length;
				response.Headers.Add("Access-Control-Allow-Origin", "*");//允许跨域
				response.OutputStream.Write(buffer, 0, buffer.Length);
				response.OutputStream.Close();
				_logger.LogDebug("已响应请应：{@result}", responseMessage);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "返回消息时错误");
			}

		}
		public static void SetNewContent(string uuid, string content)
		{
			lock (_lock)
			{
				if (_contents.ContainsValue(uuid))
				{
					_logger.LogWarning("存在键值配对{key},{@value}", uuid, content);
					return;
				}
				_contents.Add(uuid, content);
				_ = Task.Run(async () =>
				{
					await Task.Delay(2 * 60 * 1000);//等待两分钟后删除数据
					if (!_contents.ContainsKey(uuid))
					{
						_logger.LogWarning("键值配对不存在{key},{@value}", uuid, content);
						return;
					}
					_contents.Remove(uuid);
				});
			}
		}
	}
}
