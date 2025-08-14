using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Handle.Extensions;
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
		public static async Task<(int code, string msg)> ReceiveAward(UserCredential userCredential, string taskID, string activityID, string activityName, string taskName, string rewaredName)
		{
			Dictionary<string, string> body = new() {
				{ "task_id", taskID },
				{ "activity_id", activityID },
				{ "activity_name", activityName },
				{ "task_name", taskName },
				{ "reward_name", rewaredName },
				{ "gaia_vtoken", "" },//默认为空
				{ "receive_from", "missionPage" },
				{ "csrf", userCredential.Bili_Jct },
			};
			var wbi = UserInteraction.GetWbi(new());
			using var request = new HttpRequestMessage(HttpMethod.Post, $"{Constants.ReceiveAward}?{wbi}");
			request.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata}");
			request.Headers.Add("user-agent", Constants.UserAgent);
			request.Headers.Add("referer", $"https://www.bilibili.com/blackboard/era/award-exchange.html?task_id={taskID}");
			request.Content = new FormUrlEncodedContent(body);

			var response = await Tool.HttpClient.SendAsync(request);
			var result = await response.Content.ReadAsStringAsync();
			var json = JObject.Parse(result);
			_logger.LogInformation("用户{user}领取的任务id：{taskid}的回复{@response}", userCredential.DedeUserID, taskID, json);

			return new(((int)json["code"]), ((string?)json["message"]));
		}
	}
}
