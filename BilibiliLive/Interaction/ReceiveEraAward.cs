using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		public static async Task ReceiveAward(UserCredential userCredential, string taskID, string activityID, string activityName, string taskName, string rewaredName, int duration = 60)
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
			request.Content = new FormUrlEncodedContent(body);

			var result = Tool.HttpClient.SendAsync(request);
			_logger.LogInformation("用户{user}领取的任务id：{}活动id：{}活动名称：{}任务名称：{}奖励名称：{}", userCredential.DedeUserID, taskID, activityID, activityName, taskName, rewaredName);
		}
	}
}
