using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		public static async Task<int> GetGlodWallet(UserCredential userCredential)
		{
			var userInfoReqMsg = new HttpRequestMessage(HttpMethod.Get, $"{Constants.GetGlodWallet}");
			userInfoReqMsg.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_ticket={userCredential.Bili_Jct};bili_jct={userCredential.Bili_Jct};sid=328");
			userInfoReqMsg.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
			var userInfoRsp = await Tool.HttpClient.SendAsync(userInfoReqMsg);
			_logger.LogDebug("获取{user}个人信息的回复{@response}", userCredential.DedeUserID, (await userInfoRsp.Content.ReadAsStringAsync()).TryPraseToJson());

			return JObject.Parse((await userInfoRsp.Content.ReadAsStringAsync()))["data"]["gold"]?.Value<int>() / 100 ?? -1;
		}
	}
}
