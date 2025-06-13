using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Net;
using WebSocketSharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MoBot.Core.Models.Action;

namespace MoBot.Handle.Net
{
	public class WebSocketClient : IBotSocketClient
	{
		private Func<EventPacket, Task> _receiveMessage;
		private readonly IConfiguration _config;
		public Func<EventPacket, Task> ReceiveMsgAction { get => _receiveMessage; set { _receiveMessage = value; } }

		private Dictionary<string, TaskCompletionSource<ActionPacketRsp>> _echoResult = new();
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
			var ws_url = _config["Server:ws_url"];
			try
			{
				ws = new WebSocket(_config["Server:ws_url"]);
				ws.OnMessage += (s, e) =>
				{
					JObject json = JObject.Parse(e.Data);
					//判断是不是事件
					if (json.TryGetValue("post_type", StringComparison.CurrentCultureIgnoreCase, out _))
					{

						var eventJson = JsonConvert.DeserializeObject<EventPacket>(e.Data)!;
						Serilog.Log.Information($"收到事件：{e.Data}");
						_receiveMessage.Invoke(eventJson);
						return;
					}
					//判断是不是api回复
					if (json.TryGetValue("echo", StringComparison.CurrentCultureIgnoreCase, out _))
					{
						var actionJson = JsonConvert.DeserializeObject<ActionPacketRsp>(e.Data)!;
						Serilog.Log.Information($"收到api回复：{e.Data}");
						_echoResult[actionJson.Echo].SetResult(actionJson);
						_echoResult.Remove(actionJson.Echo);
						return;
					}

					Serilog.Log.Information($"收到未知消息：{e.Data}");

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

		public async Task<ActionPacketRsp> SendMessage(string action, ActionType actionType, ActionBase message)
		{
			TaskCompletionSource<ActionPacketRsp> echoPacket = new();
			string uuid = Guid.NewGuid().ToString();
			_echoResult.Add(uuid, echoPacket);
			string msg = JsonConvert.SerializeObject(new ActionPacketReq() { Action = action, Echo = uuid, Params = message });
			Serilog.Log.Debug($"发送消息{msg}");

			ws.Send(msg);

			return (await echoPacket.Task);
		}
	}
}
