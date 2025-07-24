using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		//获取激励计划的任务
		public static async Task<TaskInfoRsp> GetTaskInfo(UserCredential userCredential, List<string> taskIds)
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, $"{Constants.GetEraTask}?task_ids={String.Join(",", taskIds)}");
			request.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata}");
			using var response = await Tool.HttpClient.SendAsync(request);
			var responseString = await response.Content.ReadAsStringAsync();
			_logger.LogDebug("获取{user}任务：{@taskID}的任务结果{Info}", userCredential.DedeUserID, taskIds, responseString);

			return JsonConvert.DeserializeObject<TaskInfoRsp>(responseString);
		}
	}
}
