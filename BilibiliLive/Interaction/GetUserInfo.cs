using BilibiliLive.Constant;
using BilibiliLive.Models;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{/// <summary>
	 /// 获取用户个人信息
	 /// </summary>
	 /// <param name="userCredential">用户信息</param>
	 /// <returns>返回用户信息data，若获取失败则返回new</returns>
		public static async Task<UserInfoRsp> GetUserInfo(UserCredential userCredential)
		{
			var wbi = await GetWbi(new() { { "mid", $"{userCredential.DedeUserID}" } });
			_logger.LogDebug("Wbi={wbiurl}", wbi);
			var userInfoReqMsg = new HttpRequestMessage(HttpMethod.Get, $"{Constants.GetUserInfo}?{wbi}");
			userInfoReqMsg.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_ticket={userCredential.Bili_Jct};bili_jct={userCredential.Bili_Jct};sid=328");
			userInfoReqMsg.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
			var userInfoRsp = await Tool.HttpClient.SendAsync(userInfoReqMsg);
			_logger.LogDebug("获取{user}个人信息的回复{@response}", userCredential.DedeUserID, (await userInfoRsp.Content.ReadAsStringAsync()).TryPraseToJson());

			return JsonConvert.DeserializeObject<UserInfoRsp>((await userInfoRsp.Content.ReadAsStringAsync())) ?? new();
		}
	}
}
