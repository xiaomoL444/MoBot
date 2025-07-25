using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Action;
using MoBot.Core.Models.Event;
using MoBot.Core.Models.Message;
using MoBot.Core.Models.Net;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle.Net
{
	/// <summary>
	/// 哈哈没想到吧，搞了一个控制台发送消息，这样子就直接在控制台发送自己构造的消息就好了（）
	/// </summary>
	public class ConsoleClient : IBotSocketClient
	{
		private readonly IMoBotClient _moBotClient;
		private readonly ILogger<ConsoleClient> _logger;

		public ConsoleClient(
			IMoBotClient moBotClient,
			ILogger<ConsoleClient> logger)
		{
			_moBotClient = moBotClient;
			_logger = logger;
		}

		public void Initial()
		{

			_logger.LogInformation("2333启动了控制台Client");
			MessageSender.SocketClient = this;
			Task.Run(() =>
			{
				while (true)
				{
					try
					{
						var commond = Console.ReadLine();
						JObject json = JObject.Parse(commond);
						//判断是不是事件
						if (json.TryGetValue("post_type", StringComparison.CurrentCultureIgnoreCase, out _))
						{
							var eventJson = JsonConvert.DeserializeObject<EventPacketBase>(commond, new JsonSerializerSettings() { Converters = new List<JsonConverter> { new EventPacketConverter() } })!;

							_logger.LogInformation("收到事件：{PostType}->{@commond}", eventJson.PostType, json);

							_moBotClient.RouteAsync(eventJson);
							continue;
						}
						//判断是不是api回复
						if (json.TryGetValue("echo", StringComparison.CurrentCultureIgnoreCase, out _))
						{
							var actionJson = JsonConvert.DeserializeObject<ActionPacketRsp>(commond)!;
							_logger.LogInformation("收到api回复：{@commond}", json);
							continue;
						}

						_logger.LogWarning("收到未知消息：{@commond}", json);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "读取控制台错误");
					}
				}
			});
		}

		public Task<ActionPacketRsp> SendMessage(string action, ActionType actionType, object message)
		{
			_logger.LogInformation("发送消息{actionType} /{action} {@message}", actionType, action, message);
			return Task.FromResult(new ActionPacketRsp());
		}
	}
}
