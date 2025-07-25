using BilibiliLive.Constant;
using BilibiliLive.Interaction;
using BilibiliLive.Models;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using HttpClient = BilibiliLive.Tool.HttpClient;

namespace BilibiliLive.Handle
{
	public class EraHandle : IMessageHandle<Group>
	{
		private List<string> commands = ["/更新激励计划", "/查询任务"];

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;


		public EraHandle(
			ILogger<StreamHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public async Task Initial()
		{
			//开启http伺服器给webshot传数据
			HttpServer.Start();

			return;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.GroupId == 1079464803 && message.Sender.UserId == Constants.OPAdmin && commands.Contains(message.RawMessage))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}


		public async Task HandleAsync(Group message)
		{
			switch (message.RawMessage)
			{
				case "/更新激励计划":
					await RefreshEraData(message);
					break;
				case "/查询任务":
					await QueryTasks(message);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 刷新活动信息
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		async Task RefreshEraData(Group group)
		{
			var eraTaskConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);
			//获取原神激励计划的url
			using var getEraUrlResponse = await HttpClient.SendAsync(new(HttpMethod.Get, $"{Constants.GetEraUrl}?topic_id=3219"));
			getEraUrlResponse.EnsureSuccessStatusCode();
			var getEraUrlResponseString = await getEraUrlResponse.Content.ReadAsStringAsync();
			_logger.LogDebug("获取原神eraUrl信息{url}", getEraUrlResponseString);
			var eraUrl = ((string?)JObject.Parse(getEraUrlResponseString)["data"]["functional_card"]["traffic_card"]["jump_url"]);

			//下载html内容
			async Task<string> GetHtmlContent(string url)
			{
				using var eraUrlHtmlResponse = await HttpClient.SendAsync(new(HttpMethod.Get, url));
				eraUrlHtmlResponse.EnsureSuccessStatusCode();
				return await eraUrlHtmlResponse.Content.ReadAsStringAsync();
			}
			//获取原神激励计划页面的html
			var eraUrlHtmlContent = await GetHtmlContent(eraUrl);

			//获得h5链接的跳转连接
			var h5Url = "";
			var match = System.Text.RegularExpressions.Regex.Match(eraUrlHtmlContent, @"var\s+jumpUrl\s*=\s*'([^']+)'");
			if (match.Success)
			{
				h5Url = match.Groups[1].Value;
			}

			//获取原神激励计划页面的html
			var eraUrlH5HtmlContent = await GetHtmlContent(h5Url);

			//获得html内的json元素（）若抓取为空会报错
			string ExtractJsonAfter(string html, string marker)
			{
				int start = html.IndexOf(marker);
				if (start == -1) return null;

				int braceStart = html.IndexOf('{', start);
				if (braceStart == -1) return null;

				int braceCount = 0;
				bool inString = false;
				char lastChar = '\0';

				for (int i = braceStart; i < html.Length; i++)
				{
					char c = html[i];

					if (c == '"' && lastChar != '\\')
					{
						inString = !inString;
					}

					if (!inString)
					{
						if (c == '{') braceCount++;
						else if (c == '}') braceCount--;
					}

					if (braceCount == 0 && !inString)
					{
						return html.Substring(braceStart, i - braceStart + 1);
					}

					lastChar = c;
				}

				return null; // 如果沒匹配到完整的
			}
			var eraPageDataString = ExtractJsonAfter(eraUrlH5HtmlContent, "window.__BILIACT_EVAPAGEDATA__ = ");

			_logger.LogDebug("获取到的活动Data:{data}", eraPageDataString);

			//解析每个元件
			var eraPageElementData = JObject.Parse(eraPageDataString)["layerTree"][0]["slots"][0]["children"];
			var taskJsonPath = "$.slots[0].children[1].slots[0].children[0]";
			string liveTaskUrl = ((string?)eraPageElementData.FirstOrDefault(q => ((string?)q.SelectToken($"{taskJsonPath}.alias")) == "直播任务").SelectToken($"{taskJsonPath}.props.jumpAddress"));
			var viewLiveTaskUrl = ((string?)eraPageElementData.FirstOrDefault(q => ((string?)q.SelectToken($"{taskJsonPath}.alias")) == "看播任务").SelectToken($"{taskJsonPath}.props.jumpAddress"));
			var activityID = string.Empty;
			var activityIDMatch = System.Text.RegularExpressions.Regex.Match(eraUrlH5HtmlContent, @"var\s+activityPageEvaConfigActivityId\s*=\s*[""']([^""']+)[""']");
			if (activityIDMatch.Success)
			{
				activityID = activityIDMatch.Groups[1].Value;
			}
			eraTaskConfig.ActivityID = activityID;

			var eraTitleString = ExtractJsonAfter(eraUrlH5HtmlContent, "window.__BILIACT_PAGEINFO__ = ");
			var eraTitle = (string?)JObject.Parse(eraTitleString)["title"];
			eraTaskConfig.TaskTitle = eraTitle;
			_logger.LogInformation("获得的标题：{title}，活动id：{activity_id},直播任务url:{liveurl}，看播任务url:{viewurl}", eraTitle, activityID, liveTaskUrl, viewLiveTaskUrl);

			//处理直播任务url
			var liveUrlHtmlContent = await GetHtmlContent(liveTaskUrl);
			var liveInitailStateString = ExtractJsonAfter(liveUrlHtmlContent, "window.__initialState = ");
			var liveTaskElementsData = JObject.Parse(liveInitailStateString)["EvaTaskButton"];

			var liveTaskIDs = liveTaskElementsData.Select(s => ((string?)s["taskItem"]["taskId"])).Distinct().ToList();
			_logger.LogInformation("获取到的所有直播任务id：{@ids}", liveTaskIDs);
			eraTaskConfig.LiveTaskIDs = liveTaskIDs;

			//同样处理看播任务url
			var viewUrlHtmlContent = await GetHtmlContent(viewLiveTaskUrl);
			var viewInitailStateString = ExtractJsonAfter(viewUrlHtmlContent, "window.__initialState = ");
			var viewTaskElementsData = JObject.Parse(viewInitailStateString)["EvaTaskButton"];

			var viewTaskIDs = viewTaskElementsData.Select(s => ((string?)s["taskItem"]["taskId"])).Distinct().ToList();
			_logger.LogInformation("获取到的所有看播任务id：{@ids}", viewTaskIDs);
			eraTaskConfig.ViewTaskIDs = viewTaskIDs;

			_dataStorage.Save(Constants.EraFile, eraTaskConfig);

			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text($"[{eraTitle}]更新成功啦~").Build());

			return;
		}

		/// <summary>
		/// 获得任务信息
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		async Task QueryTasks(Group group)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var eraConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);

			//获取任务
			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("正在获取中...请稍等......").Build());
			foreach (var account in accountConfig.Users)
			{
				if (!account.IsQureyTask)
				{
					_logger.LogDebug("{user}未开启查询任务", account.Uid);
					return;
				}
				var userCredential = account.UserCredential;
				var userInfo = await UserInteraction.GetUserInfo(userCredential);

				//直播任务
				var liveRes = await UserInteraction.GetTaskInfo(userCredential, eraConfig.LiveTaskIDs);
				if (liveRes is not { Code: 0 })
				{
					_logger.LogError("获取{user}直播任务信息错误CODE:{code}", userCredential.DedeUserID, liveRes?.Code);
				}

				//看播任务
				var viewRes = await UserInteraction.GetTaskInfo(userCredential, eraConfig.ViewTaskIDs);
				if (viewRes is not { Code: 0 })
				{
					_logger.LogError("获取{user}看播任务信息错误CODE:{code}", userCredential.DedeUserID, viewRes?.Code);
				}

				var startChar = " ♪ ";
				var text = $@"[{userInfo.Data.Name}]
新人任务：
{startChar}{liveRes.Data.List[0].TaskName}：{liveRes.Data.List[0].CheckPoints[0].AwardName}({liveRes.Data.List[0].TaskStatus})
每日任务：
{string.Join("\n", liveRes.Data.List.Skip(1).Take(4).Select(s => $"{startChar}{s.TaskName}：{s.CheckPoints[0].AwardName}({s.TaskStatus})"))}
完成“每日直播任务” ————完成天数[{liveRes.Data.List[5].AccumulativeCount}]
{string.Join("\n", liveRes.Data.List[5].AccumulativeCheckPoints.Select(s => $"{startChar}{s.AwardName}({s.Status})"))}";

				string dataUuid = Guid.NewGuid().ToString();
				string backgroundUuid = Guid.NewGuid().ToString();
				string faceUuid = Guid.NewGuid().ToString();
				HttpServer.SetNewContent(dataUuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(new
				{
					text = text,
					face = userInfo.Data.Face,
					background = $"{HttpServer.GetIPAddress()}?id={backgroundUuid}"
				}));
				//HttpServer.SetNewContent(faceUuid, HttpServerContentType.ImagePng, await icon.Content.ReadAsByteArrayAsync());
				HttpServer.SetNewContent(backgroundUuid, HttpServerContentType.ImagePng, File.ReadAllBytes("./Asserts/images/MyLover.png"));

				//准备绘画
				_ = Task.Run(async () =>
				{
					string base64 = await Webshot.ScreenShot($"{Webshot.GetIPAddress()}/TaskStatus?id={dataUuid}");
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Image("base64://" + base64).Build());
				});
			}



			return;
		}
	}
}
