using MoBot.Core.Interfaces;
using MoBot.Core.Models.Action;
using MoBot.Core.Models.Event;
using MoBot.Core.Models.Net;
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
		public IMoBotClient MoBotClient { get; set; }

		public void Initial()
		{
			Serilog.Log.Information("2333启动了控制台Client");
			while (true)
			{
				var commond = Console.ReadLine();
				JObject json = JObject.Parse(commond);
				//判断是不是事件
				if (json.TryGetValue("post_type", StringComparison.CurrentCultureIgnoreCase, out _))
				{
					var eventJson = JsonConvert.DeserializeObject<EventPacketBase>(commond, new JsonSerializerSettings() { Converters = new List<JsonConverter> { new EventPacketConverter() } })!;


					Serilog.Log.Information($"收到事件：{eventJson.PostType}->{commond}");

					MoBotClient.RouteAsync(eventJson);
					return;
				}
				//判断是不是api回复
				if (json.TryGetValue("echo", StringComparison.CurrentCultureIgnoreCase, out _))
				{
					var actionJson = JsonConvert.DeserializeObject<ActionPacketRsp>(commond)!;
					Serilog.Log.Information($"收到api回复：{commond}");
					return;
				}

				Serilog.Log.Information($"收到未知消息：{commond}");
			}
		}

		public Task<ActionPacketRsp> SendMessage(string action, ActionType actionType, ActionBase message)
		{
			return Task.FromResult(new ActionPacketRsp());
		}
	}
}
