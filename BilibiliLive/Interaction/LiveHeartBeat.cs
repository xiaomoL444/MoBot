using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf.Types;
using OpenBLive.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		private static List<string> _device = ["AUTO4517492120454296", "666a13e3-242f-4c31-9729-c5aacb69d85a"];

		public static async Task<(int code, string msg, long ts, int timeInterval, string secret_key)> LiveEHeartBeat(UserCredential userCredential, int livePart, int areaV2, long room)
		{
			//E登录
			var Eparam = new Dictionary<string, string>() {
					{ "id", $"[ {livePart}, {areaV2}, 0, {room}]" },
					{ "device",  JsonConvert.SerializeObject(_device)},
					{ "ruid", room.ToString() },
					{ "ts", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
					{ "is_patch", "0" },
					{ "heart_beat", "[]" },
					{ "ua", Constants.UserAgent },
					{ "web_location", "444.8" },
					{ "csrf", userCredential.Bili_Jct },
				};
			var Ewbi = await UserInteraction.GetWbi(Eparam);
			var Erequest = new HttpRequestMessage(HttpMethod.Post, Constants.EHeartBeat + "?" + Ewbi);
			Erequest.Headers.Add("Origin", "https://live.bilibili.com");
			Erequest.Headers.Add("Referer", $"https://live.bilibili.com/{room}");
			Erequest.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
			Erequest.Headers.Add("user-agent", Constants.UserAgent);

			var Eresponse = await Tool.HttpClient.SendAsync(Erequest);
			var Eresult = await Eresponse.Content.ReadAsStringAsync();
			_logger.LogDebug("直播进入房间回复{@res}", Eresult.TryPraseToJson());
			var Ejson = JObject.Parse(Eresult);
			return new(((int)Ejson["code"]), ((string?)Ejson["message"]), (long)Ejson["data"]["timestamp"], (int)Ejson["data"]["heartbeat_interval"], (string?)Ejson["data"]["secret_key"]);
		}

		public static async Task<(int code, string msg, long ts, int timeInterval, string secret_key)> LiveXHeratBeat(UserCredential userCredential, int livePart, int areaV2, int index, long room, long ets, string benchmark, int timeInterval)
		{
			var ts = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			//X登录
			var Eparam = new Dictionary<string, string>() {
					{ "s",await GenerateS(livePart,areaV2,index,room,ets,benchmark,timeInterval,ts) },
					{ "id", $"[ {livePart}, {areaV2}, {index}, {room}]" },
					{ "device",  JsonConvert.SerializeObject(_device)},
					{ "ruid", "1954352837" },
					{ "ets", ets.ToString() },
					{ "benchmark", benchmark },
					{ "time", timeInterval.ToString() },
					{ "ts",ts.ToString() },
					{ "ua", Constants.UserAgent },
					{ "trackid", "-99998" },
					{ "web_location", "444.8" },
					{ "csrf", userCredential.Bili_Jct },
				};
			var Xwbi = await UserInteraction.GetWbi(Eparam);
			var Xrequest = new HttpRequestMessage(HttpMethod.Post, Constants.XHeartBeat + "?" + Xwbi);
			Xrequest.Headers.Add("Origin", "https://live.bilibili.com");
			Xrequest.Headers.Add("Referer", $"https://live.bilibili.com/{room}");
			Xrequest.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
			Xrequest.Headers.Add("user-agent", Constants.UserAgent);

			var Xresponse = await Tool.HttpClient.SendAsync(Xrequest);
			var Xresult = await Xresponse.Content.ReadAsStringAsync();
			_logger.LogDebug("直播心跳回复{@res}", Xresult.TryPraseToJson());

			var Xjson = JObject.Parse(Xresult);
			return new((int)Xjson["code"], ((string?)Xjson["message"]), (long)Xjson["data"]["timestamp"], (int)Xjson["data"]["heartbeat_interval"], (string?)Xjson["data"]["secret_key"]);
		}

		private static async Task<string> GenerateS(int livePart, int areaV2, int index, long room, long ets, string benchmark, int timeInterval, long ts)
		{
			var json = new
			{
				t = new
				{
					id = new List<long>() { livePart, areaV2, index, room },
					device = _device,
					ets = ets,
					benchmark = benchmark,
					time = timeInterval,
					ts = ts,
					ua = Constants.UserAgent
				},
				r = new List<int>() { 2, 5, 1, 4 }
			};
			using var request = new HttpRequestMessage(HttpMethod.Post, $"{Constants.LiveHeartGenerateSUrl}/enc");
			request.Content = JsonContent.Create(json);
			using var response = await Tool.HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync();
			_logger.LogDebug("json:{@json}的结果是{result}", json, result);
			return ((string?)JObject.Parse(result)["s"]);
		}
	}
}
