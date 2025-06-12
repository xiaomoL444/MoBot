using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Net;
using WebSocketSharp;
using Microsoft.Extensions.Configuration;

namespace MoBot.Handle.Net
{
	public class WebSocketClient : IBotSocketClient
	{
		private readonly ILogger<WebSocketClient> _logger;
		private Func<string, Task> _receiveMessage;
		private readonly IConfiguration _config;
		public Func<string, Task> ReceiveMsgAction { get => _receiveMessage; set { _receiveMessage = value; } }

		public WebSocketClient(
			ILogger<WebSocketClient> logger,
			IConfiguration config
			)
		{
			_logger = logger;
			_config = config;

			_receiveMessage = (s) => { return Task.CompletedTask; };
		}

		private WebSocket ws;

		public void Initial()
		{
			_logger.LogInformation("初始化WebSocketClient");
			var ws_url = _config["Server:ws_url"];
			try
			{
				var ws = new WebSocket(_config["Server:ws_url"]);
				ws.OnMessage += (s, e) =>
				{
					_logger.LogInformation($"收到消息：{e.Data}");
					_receiveMessage.Invoke(e.Data);
				};
				ws.Connect();
			}
			catch (Exception ex)
			{
				_logger.LogError($"ws_url:{{{ws_url}}}连接ws服务器失败，程序返回");
				return;
			}
			_logger.LogInformation($"正在监听{ws_url}");

		}

		public void SendMessage(string action, ActionType actionType, string message)
		{
			_logger.LogDebug($"发送消息{message}");
		}
	}
}
