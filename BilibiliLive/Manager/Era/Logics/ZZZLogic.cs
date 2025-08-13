using BilibiliLive.Interaction;
using BilibiliLive.Manager.Era.Core;
using BilibiliLive.Models;
using BilibiliLive.Models.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenBLive.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Manager.Era.Logics
{
	public class ZZZLogic : BaseEraLogic<ZZZLogic>
	{
		protected override int TopicID => 1200649;

		protected override string GameName => "zzz";

		private const string DailyStr = "daily";
		private const string LiveStr = "live";


		protected override async Task<string> AssembleMessage(AccountConfig.User user, EraTaskConfig.EraTaskData eraTaskData)
		{
			var userCredential = user.UserCredential;
			var userInfo = await UserInteraction.GetUserInfo(userCredential);
			//每日任务
			var dailyTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[DailyStr]);
			if (dailyTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}每日任务错误CODE:{code}", userInfo.Data.Name, dailyTask.Code);
			}
			//直播任务
			var liveTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[LiveStr]);
			if (liveTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}直播任务信息错误CODE:{code}", userInfo.Data.Name, liveTask.Code);
			}

			string GetStatusWithColor(TaskInfoRsp.TaskInfoData.TaskCompleteStatus status) => $"<span {(status == TaskInfoRsp.TaskInfoData.TaskCompleteStatus.已领取 ? "style='color:#FFD700;'" : "")}>{status}</span>";

			//组装信息
			var startChar = " ♪ ";
			var text = $@"<div>[{userInfo.Data.Name}]
每日任务：
{string.Join("\n", dailyTask.Data.List.Select(s => $"{startChar}{s.TaskName}：{s.CheckPoints[0].AwardName}({GetStatusWithColor(s.TaskStatus)})"))}
直播里程碑任务 完成天数：[{liveTask.Data.List[0].AccumulativeCount}]
{string.Join("\n", liveTask.Data.List[0].AccumulativeCheckPoints.Select(s => $"{startChar}{s.AwardName}({GetStatusWithColor(s.Status)})"))}</div>";

			return text;
		}

		protected override async Task<List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>> GetDailyTaskCheckPointElementList(UserCredential userCredential)
		{
			var taskList = TryGetEraTaskData(GameName).eraTaskData.TaskIDs[DailyStr];

			return (await UserInteraction.GetTaskInfo(userCredential, taskList)).Data.List.Select(q => q.CheckPoints[0]).ToList();
		}

		protected override async Task<List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>> GetFinallyTaskCheckPointElementList(UserCredential userCredential, string awardName)
		{
			var eraData = TryGetEraTaskData(GameName).eraTaskData;
			//正常情况下应该只有live一种方式
			switch (awardName)
			{
				case "live":
					var liveCheckPoint = (await UserInteraction.GetTaskInfo(userCredential, eraData.TaskIDs[LiveStr])).Data.List.Select(x => x.AccumulativeCheckPoints ?? x.CheckPoints).SelectMany(x => x).ToList();
					return liveCheckPoint;
				default:
					_logger.LogWarning("输入了不存在的领奖名");
					return null;
			}
		}

		protected override async Task<EraTaskConfig.EraTaskData> SetTaskIDs(string eraPageData, EraTaskConfig.EraTaskData eraTaskData)
		{
			//获取直播奖励页面链接
			var json = JObject.Parse(eraPageData);
			var liveTaskUrl = json["layerTree"][0]["slots"][0]["children"].FirstOrDefault(q => q["alias"] != null && q["alias"].Value<string>() == "直播委托")["slots"][0]["children"][0]["slots"][0]["children"][0]["props"]["jumpAddress"].Value<string>();

			//下载直播奖励页面html
			var htmlContent = await GetHtmlContent(liveTaskUrl);
			var initialStateString = ExtractJsonAfter(htmlContent, "window.__initialState = ");
			var taskElements = JObject.Parse(initialStateString)["EvaTaskButton"];

			var taskData = taskElements.Select(s => new { taskID = (string?)s["taskItem"]["taskId"], taskName = s["taskItem"]["taskName"].Value<string>() }).GroupBy(x => x.taskID).Select(s => s.First());

			_logger.LogInformation("获取到的所有直播任务：{@datas}", taskData);

			eraTaskData.TaskIDs.Add(DailyStr, taskData.Take(4).Select(x => x.taskID).ToList());//每日奖励
			eraTaskData.TaskIDs.Add(LiveStr, taskData.Skip(4).Take(1).Select(x => x.taskID).ToList());//直播奖励

			return eraTaskData;
		}
	}
}
