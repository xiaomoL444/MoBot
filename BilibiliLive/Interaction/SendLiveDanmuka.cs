using BilibiliLive.Constant;
using BilibiliLive.Models;
using BilibiliLive.Models.Live;
using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
using Newtonsoft.Json.Linq;
using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		public static async Task<OneOf<None, Error<(int code, string msg)>>> SendDanmuka(UserCredential userCredential, string roomID, LiveDanmukuType danmukuType, string msg)
		{
			Dictionary<string, string> sendGiftParam = new()
			{
				{"bubble",userCredential.DedeUserID},
				{"msg",msg},
				{"color","16777215"},
				{"mode","1"},
				{"reportParams","[object Object]"},
				{"fontsize","25"},
				{"rnd",DateTimeOffset.Now.ToUnixTimeSeconds().ToString()},
				{"roomid",roomID},
				{"csrf",userCredential.Bili_Jct},
				{"csrf_token",userCredential.Bili_Jct},
			};

			switch (danmukuType)
			{
				case LiveDanmukuType.Text:
					sendGiftParam.Add("room_type", "0");
					sendGiftParam.Add("jumpfrom", "0");
					sendGiftParam.Add("reply_mid", "0");
					sendGiftParam.Add("reply_attr", "0");
					sendGiftParam.Add("replay_dmid", "");
					sendGiftParam.Add("statistics", "{\"appId\":100,\"platform\":5}");
					sendGiftParam.Add("reply_type", "0");
					sendGiftParam.Add("reply_uname", "");
					break;
				case LiveDanmukuType.Emotion:
					sendGiftParam.Add("dm_type", "1");
					sendGiftParam.Add("emoticonOptions", "[object Object]");
					break;
				default:
					break;
			}


			var wbi = await GetWbi(sendGiftParam);
			var sendGiftRequest = new HttpRequestMessage(HttpMethod.Post, $"{Constants.SendDanmaku}?{wbi}");
			sendGiftRequest.Headers.Add("Cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
			sendGiftRequest.Headers.UserAgent.TryParseAdd(Constants.UserAgent);

			var sendGiftResponse = await Tool.HttpClient.SendAsync(sendGiftRequest);
			var sendGiftResponseString = await sendGiftResponse.Content.ReadAsStringAsync();

			_logger.LogDebug("发送弹幕消息：{@res}", sendGiftResponseString.TryPraseToJson());

			var json = JObject.Parse(sendGiftResponseString);
			if (((int)json["code"]) == 0)
			{
				return new None();
			}
			return new Error<(int, string)>(((int)json["code"], (string?)json["msg"]));
		}
	}
}
