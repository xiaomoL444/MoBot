using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Net;
using WebSocketSharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MoBot.Handle.Net
{
	public class WebSocketClient : IBotSocketClient
	{
		private Func<string, Task> _receiveMessage;
		private readonly IConfiguration _config;
		public Func<string, Task> ReceiveMsgAction { get => _receiveMessage; set { _receiveMessage = value; } }

		public WebSocketClient(
			IConfiguration config
			)
		{
			_config = config;

			_receiveMessage = (s) => { return Task.CompletedTask; };
		}

		private WebSocket ws;

		public void Initial()
		{
			Serilog.Log.Information("初始化WebSocketClient");

			_receiveMessage = (s) => { return Task.CompletedTask; };//重置一下获得消息后的事件

			//消息解析器，因为websocket会返回echo码，所以要把码和对应的结果作为键值保存起来，等待取出
			_receiveMessage += (s) =>
			{
				try
				{
					var json = JsonConvert.DeserializeObject<ActionPacketRsp>(s);
				}
				catch (Exception ex)
				{
					Serilog.Log.Error($"消息解析失败，{ex}");
				}

				return Task.CompletedTask;
			};
			var ws_url = _config["Server:ws_url"];
			try
			{
				ws = new WebSocket(_config["Server:ws_url"]);
				ws.OnMessage += (s, e) =>
				{
					Serilog.Log.Information($"收到消息：{e.Data}");
					_receiveMessage.Invoke(e.Data);
				};
				ws.Connect();
			}
			catch (Exception ex)
			{
				Serilog.Log.Error($"ws_url:{{{ws_url}}}连接ws服务器失败，程序返回，{ex}");
				return;
			}
			Serilog.Log.Information($"正在监听{ws_url}");

		}

		public Task<string> SendMessage(string action, ActionType actionType, string message)
		{
			Serilog.Log.Debug($"发送消息{message}");
			return Task.FromResult("");
		}
	}
}
