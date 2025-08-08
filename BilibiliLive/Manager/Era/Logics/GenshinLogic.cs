using BilibiliLive.Interaction;
using BilibiliLive.Manager.Era.Core;
using BilibiliLive.Models;
using BilibiliLive.Models.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BilibiliLive.Models.Config.AccountConfig;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BilibiliLive.Manager.Era.Logics
{
	public class GenshinLogic : BaseEraLogic<GenshinLogic>
	{
		protected override int TopicID => 3219;
		protected override string GameName => "genshin";

		private const string NewComerStr = "newcomer";
		private const string DailyStr = "daily";
		private const string LiveStr = "live";
		private const string ViewStr = "view";

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

			//看播任务
			var viewTask = await UserInteraction.GetTaskInfo(userCredential, eraTaskData.TaskIDs[ViewStr]);
			if (viewTask is not { Code: 0 })
			{
				_logger.LogError("获取{user}看播任务信息错误CODE:{code}", userInfo.Data.Name, viewTask?.Code);
			}

			string GetStatusWithColor(TaskInfoRsp.TaskInfoData.TaskCompleteStatus status) => $"<span {(status == TaskInfoRsp.TaskInfoData.TaskCompleteStatus.已领取 ? "style='color:#FFD700;'" : "")}>{status}</span>";

			//组装信息
			var startChar = " ♪ ";
			var text = $@"<div>[{userInfo.Data.Name}]
新人任务：
{startChar}{newComerTask.Data.List[0].TaskName}：{newComerTask.Data.List[0].CheckPoints[0].AwardName}({GetStatusWithColor(newComerTask.Data.List[0].TaskStatus)})
每日任务：
{string.Join("\n", dailyTask.Data.List.Select(s => $"{startChar}{s.TaskName}：{s.CheckPoints[0].AwardName}({GetStatusWithColor(s.TaskStatus)})"))}</div>
<div style='display: flex;gap:1vw;'><div style='flex: 0 1 auto;white-space: pre-wrap;'>完成“每日直播任务” 完成天数[{liveTask.Data.List[0].AccumulativeCount}]
{string.Join("\n", liveTask.Data.List[0].AccumulativeCheckPoints.Select(s => $"{startChar}{s.AwardName}({GetStatusWithColor(s.Status)})"))}</div><div style='flex: 0 1 auto;white-space: pre-wrap;'>完成看播任务 完成天数[{viewTask.Data.List[0].AccumulativeCount}]
{string.Join("\n", viewTask.Data.List[0].AccumulativeCheckPoints.Select(s => $"{startChar}{s.AwardName}({GetStatusWithColor(s.Status)})"))}</div></div>";

			return text;
		}

		protected override async Task<EraTaskConfig.EraTaskData> SetTaskIDs(string eraPageData, EraTaskConfig.EraTaskData eraTaskData)
		{
			//解析每个元件
			var eraPageElementData = JObject.Parse(eraPageData)["layerTree"][0]["slots"][0]["children"];
			var taskJsonPath = "$.slots[0].children[1].slots[0].children[0]";
			string liveTaskUrl = (string?)eraPageElementData.FirstOrDefault(q => (string?)q.SelectToken($"{taskJsonPath}.alias") == "直播任务").SelectToken($"{taskJsonPath}.props.jumpAddress");
			var viewLiveTaskUrl = (string?)eraPageElementData.FirstOrDefault(q => (string?)q.SelectToken($"{taskJsonPath}.alias") == "看播任务").SelectToken($"{taskJsonPath}.props.jumpAddress");

			//处理直播任务url
			var liveUrlHtmlContent = await GetHtmlContent(liveTaskUrl);
			var liveInitailStateString = ExtractJsonAfter(liveUrlHtmlContent, "window.__initialState = ");
			var liveTaskElementsData = JObject.Parse(liveInitailStateString)["EvaTaskButton"];

			var liveTaskIDs = liveTaskElementsData.Select(s => (string?)s["taskItem"]["taskId"]).Distinct().ToList();
			_logger.LogInformation("获取到的所有直播任务id：{@ids}", liveTaskIDs);
			eraTaskData.TaskIDs.Add(NewComerStr, liveTaskIDs.Take(1).ToList());//新人奖励
			eraTaskData.TaskIDs.Add(DailyStr, liveTaskIDs.Skip(1).Take(4).ToList());//每日奖励
			eraTaskData.TaskIDs.Add(LiveStr, liveTaskIDs.Skip(5).Take(1).ToList());//开播奖励

			//同样处理看播任务url
			var viewUrlHtmlContent = await GetHtmlContent(viewLiveTaskUrl);
			var viewInitailStateString = ExtractJsonAfter(viewUrlHtmlContent, "window.__initialState = ");
			var viewTaskElementsData = JObject.Parse(viewInitailStateString)["EvaTaskButton"];

			var viewTaskIDs = viewTaskElementsData.Select(s => (string?)s["taskItem"]["taskId"]).Distinct().ToList();
			_logger.LogInformation("获取到的所有看播任务id：{@ids}", viewTaskIDs);
			eraTaskData.TaskIDs.Add(ViewStr, viewTaskIDs);

			return eraTaskData;
		}

		protected override async Task<List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>> GetDailyTaskCheckPointElementList(UserCredential userCredential)
		{
			var taskList = TryGetEraTaskData(GameName).eraTaskData.TaskIDs[DailyStr];
			return (await UserInteraction.GetTaskInfo(userCredential, taskList)).Data.List.Select(q => q.CheckPoints[0]).ToList();
			//return (await Task.WhenAll(taskList.Select(async s => (await UserInteraction.GetTaskInfo(userCredential, new() { s })).Data.List[0].CheckPoints))).SelectMany(x => x).ToList();
		}

		protected override async Task<List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>> GetFinallyTaskCheckPointElementList(UserCredential userCredential, string awardName)
		{
			var eraData = TryGetEraTaskData(GameName).eraTaskData;
			switch (awardName)
			{
				case "live":
					//直播的话，返回新人和直播任务
					var liveTask = eraData.TaskIDs[LiveStr][0];
					var newComerTask = eraData.TaskIDs[NewComerStr][0];
					//直播
					var liveTaskCheckPoints = (await UserInteraction.GetTaskInfo(userCredential, new() { liveTask })).Data.List[0].AccumulativeCheckPoints;
					//新人
					var newComerTaskCheckPoints = (await UserInteraction.GetTaskInfo(userCredential, new() { newComerTask })).Data.List[0].CheckPoints;
					return (new[] { liveTaskCheckPoints, newComerTaskCheckPoints }).SelectMany(x => x).ToList();
				case "view":
					//看播的话返回看播任务
					var viewTask = eraData.TaskIDs[ViewStr][0];
					//看播
					var viewTaskCheckPoints = (await UserInteraction.GetTaskInfo(userCredential, new() { viewTask })).Data.List[0].AccumulativeCheckPoints;
					return viewTaskCheckPoints;
				default:
					_logger.LogWarning("输入了不存在的领奖名");
					return null;
			}

		}
	}
}
