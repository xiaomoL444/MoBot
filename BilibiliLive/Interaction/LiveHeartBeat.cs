using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
using OpenBLive.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		private static string _device = "[\"AUTO4517492120454296\",\"666a13e3-242f-4c31-9729-c5aacb69d85a\"]";

		public static async Task LiveEHeartBeat(UserCredential userCredential, string livePart, string areaV2, string room)
		{
			//E登录
			var Eparam = new Dictionary<string, string>() {
					{ "id", $"[ {livePart}, {areaV2}, 0, {room}]" },
					{ "device",  _device},
					{ "ruid", room },
					{ "ts", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
					{ "is_patch", "0" },
					{ "heart_beat", "[]" },
					{ "ua", Constants.UserAgent },
					{ "web_location", "444.8" },
					{ "csrf", userCredential.Bili_Jct },
				};
			var Ewbi = UserInteraction.GetWbi(Eparam);
			var Erequest = new HttpRequestMessage(HttpMethod.Post, Constants.EHeartBeat + "?" + Ewbi);
			Erequest.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
			Erequest.Headers.Add("user-agent", Constants.UserAgent);

			var Eresponse = await Tool.HttpClient.SendAsync(Erequest);
			var Eresult = await Eresponse.Content.ReadAsStringAsync();
			_logger.LogDebug("直播进入房间回复{res}", Eresult.TryPraseToJson());
		}

		public static async Task LiveXHeratBeat(UserCredential userCredential, string livePart, string areaV2, string index, string room,string ets,string benchmark,string time)
		{
			//X登录
			var Eparam = new Dictionary<string, string>() {
					{ "id", $"[ {livePart}, {areaV2}, {index}, {room}]" },
					{ "device",  _device},
					{ "ruid", "1954352837" },
					{ "ets", ets },
					{ "benchmark", benchmark },
					{ "time", time },
					{ "ts", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
					{ "ua", Constants.UserAgent },
					{ "trackid", "-99998" },
					{ "web_location", "444.8" },
					{ "csrf", userCredential.Bili_Jct },
				};
			var Xwbi = UserInteraction.GetWbi(Eparam);
			var Xrequest = new HttpRequestMessage(HttpMethod.Post, Constants.XHeartBeat + "?" + Xwbi);
			Xrequest.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
			Xrequest.Headers.Add("user-agent", Constants.UserAgent);

			var Xresponse = await Tool.HttpClient.SendAsync(Xrequest);
			var Xresult = await Xresponse.Content.ReadAsStringAsync();
			_logger.LogDebug("直播心跳回复{res}", Xresult.TryPraseToJson());
		}
	}
}
