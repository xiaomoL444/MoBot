using BilibiliLive.Interaction;
using BilibiliLive.Manager.Era.Core;
using BilibiliLive.Models;
using BilibiliLive.Models.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Manager.Era.Logics
{
	public class StarRailLogic : BaseEraLogic<StarRailLogic>
	{
		protected override int TopicID => 1032671;

		protected override string GameName => "starrail";

		private const string NewComerStr = "newcomer";
		private const string DailyStr = "daily";
		private const string LimitTimeStr = "limitTime";
		private const string LiveStr = "live";

		protected override async Task<string> AssembleMessage(AccountConfig.User user, EraTaskConfig.EraTaskData eraTaskData)
		{
			var userCredential = user.UserCredential;
			var userInfo = await UserInteraction.GetUserInfo(userCredential);
			//新人任务
			var newComerTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[NewComerStr]);
			if (newComerTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}新人任务错误CODE:{code}", userInfo.Data.Name, newComerTask.Code);
			}
			//每日任务
			var dailyTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[DailyStr]);
			if (dailyTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}每日任务错误CODE:{code}", userInfo.Data.Name, newComerTask.Code);
			}
			//直播任务
			var liveTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[LiveStr]);
			if (liveTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}直播任务信息错误CODE:{code}", userInfo.Data.Name, liveTask.Code);
			}

			//限时任务
			var limitTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[LimitTimeStr]);
			if (limitTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}限时任务信息错误CODE:{code}", userInfo.Data.Name, limitTask?.Code);
			}

			string GetStatusWithColor(TaskInfoRsp.TaskInfoData.TaskCompleteStatus status) => $"<span {(status == TaskInfoRsp.TaskInfoData.TaskCompleteStatus.已领取 ? "style='color:#c0c0c0'" : "")}>{status}</span>";

			//组装信息
			var startChar = " ♪ ";
			var text = $@"<div>[{userInfo.Data.Name}]
新人任务：完成天数[{newComerTask.Data.List[0].AccumulativeCount}]
{string.Join("\n", newComerTask.Data.List[0].AccumulativeCheckPoints.Select(s => $"{startChar}{s.AwardName}({GetStatusWithColor(s.Status)})"))}
每日任务：
{string.Join("\n", dailyTask.Data.List.Select(s => $"{startChar}{s.TaskName}：{s.CheckPoints[0].AwardName}({GetStatusWithColor(s.TaskStatus)})"))}</div>
<div style='display: flex;gap:1vw;'><div style='flex: 0 1 auto;white-space: pre-wrap;'>直播里程碑任务 上半：[{liveTask.Data.List[0].AccumulativeCount}] 下半：[{liveTask.Data.List[2].AccumulativeCount}]
{string.Join("\n", liveTask.Data.List[0].AccumulativeCheckPoints.Concat(liveTask.Data.List[2].AccumulativeCheckPoints).Select(s => $"{startChar}{s.AwardName}({GetStatusWithColor(s.Status)})"))}</div><div style='flex: 0 1 auto;white-space: pre-wrap;'>限时直播任务：
{string.Join("\n", limitTask.Data.List.Select(s => $"{startChar}{s.TaskName}：{s.CheckPoints[0].AwardName}({GetStatusWithColor(s.TaskStatus)})"))}</div></div>";

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
					var newComerCheckPoint = (await UserInteraction.GetTaskInfo(userCredential, eraData.TaskIDs[NewComerStr])).Data.List[0].AccumulativeCheckPoints;

					var limitCheckPoint = (await UserInteraction.GetTaskInfo(userCredential, eraData.TaskIDs[LimitTimeStr])).Data.List.Select(s => s.CheckPoints[0]).ToList();

					var liveCheckPoint = (await UserInteraction.GetTaskInfo(userCredential, eraData.TaskIDs[LiveStr])).Data.List.Select(x => x.AccumulativeCheckPoints ?? x.CheckPoints).SelectMany(x => x).ToList();

					return (new List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>[] { newComerCheckPoint, limitCheckPoint, liveCheckPoint }).SelectMany(x => x).ToList();
				default:
					_logger.LogWarning("输入了不存在的领奖名");
					return null;
			}
		}

		protected override async Task<EraTaskConfig.EraTaskData> SetTaskIDs(string eraPageData, EraTaskConfig.EraTaskData eraTaskData)
		{
			//获取直播奖励页面链接
			var json = JObject.Parse(eraPageData);
			var liveTaskUrl = JObject.Parse(eraPageData)["layerTree"][0]["slots"][0]["children"].FirstOrDefault(q => q["alias"] != null && q["alias"].Value<string>() == "直播里程碑任务")["slots"][0]["children"][0]["slots"][0]["children"][0]["props"]["jumpAddress"].Value<string>();

			//下载直播奖励页面html
			var htmlContent = await GetHtmlContent(liveTaskUrl);
			var initialStateString = ExtractJsonAfter(htmlContent, "window.__initialState = ");
			var taskElements = JObject.Parse(initialStateString)["EvaTaskButton"];

			var taskData = taskElements.Select(s => new { taskID = (string?)s["taskItem"]["taskId"], taskName = s["taskItem"]["taskName"].Value<string>() }).GroupBy(x => x.taskID).Select(s => s.First());

			_logger.LogInformation("获取到的所有直播任务：{@datas}", taskData);

			eraTaskData.TaskIDs.Add(NewComerStr, taskData.Take(1).Select(x => x.taskID).ToList());//新人奖励
			eraTaskData.TaskIDs.Add(DailyStr, taskData.Skip(1).Take(2).Select(x => x.taskID).ToList());//每日奖励
			eraTaskData.TaskIDs.Add(LimitTimeStr, taskData.Skip(3).Take(2).Select(x => x.taskID).ToList());//限时奖励
			eraTaskData.TaskIDs.Add(LiveStr, taskData.Skip(5).Take(4).Select(x => x.taskID).ToList());//直播奖励

			return eraTaskData;
		}
	}
}
