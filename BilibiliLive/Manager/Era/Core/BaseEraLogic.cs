using BilibiliLive.Constant;
using BilibiliLive.Interaction;
using BilibiliLive.Models;
using BilibiliLive.Models.Config;
using BilibiliLive.Models.Webshot;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Message;
using MoBot.Handle.Message;
using MoBot.Infra.PuppeteerSharp.Interface;
using MoBot.Infra.PuppeteerSharp.Interfaces;
using MoBot.Infra.PuppeteerSharp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using OneOf.Types;
using System.Collections.Concurrent;
using static BilibiliLive.Models.TaskInfoRsp.TaskInfoData.TaskElement;
using TaskStatus = BilibiliLive.Models.Webshot.TaskStatus;

namespace BilibiliLive.Manager.Era.Core
{
	public abstract class BaseEraLogic<T> : IEraLogic
	{
		protected IDataStorage _dataStorage = GlobalSetting.DataStorage;
		protected ILogger _logger = GlobalSetting.CreateLogger<T>();
		protected IWebshot _webshot = GlobalSetting.Webshot;
		protected IWebshotRequestStore _webshotRequestStore = GlobalSetting.WebshotRequestStore;

		protected abstract int TopicID { get; }//活动的topid，用来获取激励计划页面的
		protected abstract string GameName { get; }//自己的游戏名，比如genshin和starrail的

		public virtual async Task<OneOf<Success<string>, Error<string>>> QueryTasks(string uid)
		{
			//获取任务
			//sendMessage(MessageChainBuilder.Create().Text("正在获取中...请稍等......").Build());

			var (canQueryUser, userErrorMsg, user) = TryGetAuthorizedUser(uid);
			if (!canQueryUser)
			{
				return new Error<string>(userErrorMsg);
			}

			var (canQueryEra, eraErrorMsg, eraTaskData) = TryGetEraTaskData(GameName);
			if (!canQueryEra)
			{
				return new Error<string>(eraErrorMsg);
			}

			//组装信息
			var text = await AssembleMessage(user, eraTaskData);
			var userInfo = await UserInteraction.GetUserInfo(user.UserCredential);

			string dataUuid = Guid.NewGuid().ToString();
			string backgroundUuid = Guid.NewGuid().ToString();
			_webshotRequestStore.SetNewContent(dataUuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(new TaskStatus()
			{
				Text = text,
				Face = userInfo.Data.Face,
				Background = $"{_webshotRequestStore.GetIPAddress()}?id={backgroundUuid}"
			}));
			_webshotRequestStore.SetNewContent(backgroundUuid, HttpServerContentType.ImagePng, RandomImage.GetBytes());
			//准备绘画
			try
			{
				string base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/TaskStatus?id={dataUuid}");
				return new Success<string>(base64);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "截图失败");
				return new Error<string>("截图失败");
			}
		}
		/// <summary>
		/// 组装消息
		/// </summary>
		/// <param name="user">用户信息</param>
		/// <param name="eraTaskData">era的任务数据</param>
		/// <returns>返回组装的文本消息</returns>
		protected abstract Task<string> AssembleMessage(AccountConfig.User user, EraTaskConfig.EraTaskData eraTaskData);

		public virtual async Task<OneOf<Success<string>, Error<string>>> ReceiveDailyEraAward(List<string> uidLis)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var eraConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);

			var (canQueryEra, eraErrorMsg, eraTaskData) = TryGetEraTaskData(GameName);
			if (!canQueryEra)
			{
				return new Error<string>(eraErrorMsg);
			}

			var multiInfoView = new MultiInfoView() { Background = "" };

			//对每个uid进行领取奖励
			foreach (var uid in uidLis)
			{
				//查询用户是否合法
				var (canQuery, msg, user) = TryGetAuthorizedUser(uid);
				if (!canQuery)
				{
					//添加错误信息
					multiInfoView.Data.Add(new() { Name = uid, Face = MoBot.Infra.PuppeteerSharp.Constant.Constants.WhiteTransParentBase64, Info = msg });
					continue;
				}

				var checkPointElements = await GetDailyTaskCheckPointElementList(user.UserCredential);
				_logger.LogDebug("获取到的checkPoint{@cP}", checkPointElements);
				//领奖
				string text = string.Empty;
				for (int i = 0; i < checkPointElements.Count; i++)
				{
					var checkPointElement = checkPointElements[i];
					var (code, message) = await UserInteraction.ReceiveAward(user.UserCredential, checkPointElement.Sid, eraTaskData.ActivityID, eraTaskData.TaskTitle, checkPointElement.Alias, checkPointElement.AwardName);
					text += $"{checkPointElement.AwardName}:{(code == 0 ? "领取成功" : message)}\n";
					await Task.Delay(Random.Shared.Next(1100, 1500));
				}
				//添加领取信息
				var userInfo = await UserInteraction.GetUserInfo(user.UserCredential);
				multiInfoView.Data.Add(new() { Name = userInfo.Data.Name, Face = userInfo.Data.Face, Info = text });
			}

			//绘图
			var multiInfoUuid = Guid.NewGuid().ToString();
			_webshotRequestStore.SetNewContent(multiInfoUuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(multiInfoView));
			var base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/MultiInfoView?id={multiInfoUuid}");
			return new Success<string>(base64);

			//sendMessage(MessageChainBuilder.Create().Text($"领取成功啦~请输入/查询任务查看领取状态").Build());
		}
		/// <summary>
		/// 获取每日奖励的CheckPointElement(注意！可能会需要一点时间完成，会阻塞)
		/// </summary>
		/// <returns>返回每日奖励的CheckPointElements</returns>
		protected abstract Task<List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>> GetDailyTaskCheckPointElementList(UserCredential userCredential);

		public virtual async Task<OneOf<Success<string>, Error<string>, None>> ReceiveFinallylEraAward(List<string> uidList, string awardName)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var eraConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);

			var (canQueryEra, eraErrorMsg, eraTaskData) = TryGetEraTaskData(GameName);
			if (!canQueryEra)
			{
				return new Error<string>(eraErrorMsg);
			}

			ConcurrentDictionary<string, string> resultDic = new();
			int userNum = uidList.Count;
			foreach (var uid in uidList)
			{
				var (canQuery, msg, user) = TryGetAuthorizedUser(uid);
				if (!canQuery)
				{
					//添加用户无法获取信息
					//Interlocked.Decrement(ref userNum);
					resultDic.TryAdd(user.Uid, $"无法获取用户{user.Uid}信息");
					continue;
				}

				_ = Task.Run(async () =>
				{
					try
					{
						//锁定任务
						var checkPointElements = await GetFinallyTaskCheckPointElementList(user.UserCredential, awardName);
						_logger.LogDebug("获取到的checkPoint{@cP}", checkPointElements);

						var targetCheckPoints = new List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>();
						foreach (var checkPoint in checkPointElements)
						{
							if (checkPoint.Status == TaskInfoRsp.TaskInfoData.TaskCompleteStatus.已完成但未领取)
							{
								_logger.LogDebug("匹配到{@msg}", checkPoint);
								targetCheckPoints.Add(checkPoint);
							}
						}

						if (targetCheckPoints.Count <= 0)
						{
							//没有未领取的任务或者是没有完成任务
							_logger.LogInformation("user[{user}]没有未领取的任务或者是没有完成任务", user.Uid);
							Interlocked.Decrement(ref userNum);
							//添加用户没有未领取的任务或完成的任务信息
							return;
						}

						//依照每个CheckPointElement进行领取
						var text = string.Empty;
						foreach (var checkPoint in targetCheckPoints)
						{
							List<(int code, string msg)> resultList = new();
							for (int i = 0; i < 50; i++)
							{
								_logger.LogInformation("[{user}]尝试抢中...{index}", user.Uid, i);
								var result = await UserInteraction.ReceiveAward(user.UserCredential, checkPoint.Sid, eraTaskData.ActivityID, eraTaskData.TaskTitle, checkPoint.Alias, checkPoint.AwardName);
								resultList.Add(result);
								if (result.code == 0 || result.code == 75255)
								{
									_logger.LogDebug("退出领取，已领取或库存使用完");
									break;
								}

								await Task.Delay(Random.Shared.Next(250, 750));

							}
							var receiveMsg = $"任务名：{(string.IsNullOrEmpty(checkPoint.Alias) ? "（原神无法获取）" : checkPoint.Alias)}\n任务奖励：{checkPoint.AwardName}\n" + string.Join("", resultList.GroupBy(q => q).Select(s => $" ♪ {(s.Key.msg == "0" ? "领取成功" : s.Key.msg)}x{s.Count()}\n"));
							text += receiveMsg;
							_logger.LogInformation("[{user}]领取情况{msg}", user.Uid, receiveMsg);
						}
						//全部领取完了
						resultDic.TryAdd(user.Uid, text);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "{user}抢{type}出现错误", user.Uid, awardName);
						Interlocked.Decrement(ref userNum);
					}
				});
			}

			//进入等待循环
			bool isWait = true;
			while (isWait)
			{
				if (resultDic.Count == userNum)
				{

					isWait = false;

					//注意count=0
					if (userNum == 0)
					{
						_logger.LogInformation("今日所有人都不用抢");
						return new None();
					}

					Interlocked.Exchange(ref userNum, 0);

					//编辑图片发送

					var multiInfoView = new MultiInfoView();
					//组装消息
					foreach (var result in resultDic)
					{
						var userInfo = await UserInteraction.GetUserInfo(accountConfig.Users.FirstOrDefault(q => q.Uid == result.Key).UserCredential);
						multiInfoView.Data.Add(new()
						{
							Face = userInfo.Data.Face,
							Name = userInfo.Data.Name,
							Info = result.Value
						});
					}
					var uuid = Guid.NewGuid().ToString();
					_webshotRequestStore.SetNewContent(uuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(multiInfoView));
					var base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/MultiInfoView?id={uuid}");

					return new Success<string>(base64);
				}
				await Task.Delay(2 * 1000);
				_logger.LogDebug("循环等待抢中...");
			}
			return new Error<string>("错误结束抢激励计划");
			//await MessageSender.SendGroupMsg(groupID, MessageChainBuilder.Create().Text($"领取成功啦~请输入/查询任务查看领取状态").Build());
		}

		protected abstract Task<List<TaskInfoRsp.TaskInfoData.TaskElement.CheckPointElement>> GetFinallyTaskCheckPointElementList(UserCredential userCredential, string awardName);

		public virtual async Task<OneOf<Success<string>, Error<string>>> RefreshEraData()
		{
			var eraTaskConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);
			var eraTaskData = new EraTaskConfig.EraTaskData() { GameName = GameName };

			//获取h5激励计划内容
			var h5Content = await GetEraH5Content();

			//设置基本信息，标题和activityID
			SetBaseInfo(h5Content, ref eraTaskData);
			//设置活动id
			var eraPageDataString = ExtractJsonAfter(h5Content, "window.__BILIACT_EVAPAGEDATA__ = ");
			_logger.LogDebug("获取到的活动Data:{@data}", eraPageDataString);
			eraTaskData = await SetTaskIDs(eraPageDataString, eraTaskData);

			//获取完毕后先移除再复制
			eraTaskConfig.EraTaskDatas.Remove(eraTaskConfig.EraTaskDatas.FirstOrDefault(q => q.GameName == GameName));
			eraTaskConfig.EraTaskDatas.Add(eraTaskData);
			_dataStorage.Save(Constants.EraFile, eraTaskConfig);

			return new Success<string>($"{GameName}激励计划更新成功啦~");
		}
		/// <summary>
		/// 下载html内容
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		protected async Task<string> GetHtmlContent(string url)
		{
			using var eraUrlHtmlResponse = await Tool.HttpClient.SendAsync(new(HttpMethod.Get, url));
			eraUrlHtmlResponse.EnsureSuccessStatusCode();
			return await eraUrlHtmlResponse.Content.ReadAsStringAsync();
		}
		protected virtual async Task<string> GetEraH5Content()
		{
			//获取激励计划的url
			using var getEraUrlResponse = await Tool.HttpClient.SendAsync(new(HttpMethod.Get, $"{Constants.GetEraUrl}?topic_id={TopicID}"));
			getEraUrlResponse.EnsureSuccessStatusCode();
			var getEraUrlResponseString = await getEraUrlResponse.Content.ReadAsStringAsync();
			_logger.LogDebug("获取{gameName}的eraUrl信息{url}", GameName, getEraUrlResponseString);
			var eraUrl = (string?)JObject.Parse(getEraUrlResponseString)["data"]["functional_card"]["traffic_card"]["jump_url"];
			//获取原神激励计划页面的html
			var eraUrlHtmlContent = await GetHtmlContent(eraUrl);
			//获得h5链接的跳转连接
			var h5Url = "";
			var match = System.Text.RegularExpressions.Regex.Match(eraUrlHtmlContent, @"var\s+jumpUrl\s*=\s*'([^']+)'");
			if (match.Success)
			{
				h5Url = match.Groups[1].Value;
			}
			//获取激励计划页面的html
			return (await GetHtmlContent(h5Url));
		}
		/// <summary>
		/// 设置基本信息
		/// </summary>
		/// <param name="h5Content">h5网页内容</param>
		/// <param name="eraTaskData">活动数据</param>
		protected virtual void SetBaseInfo(string h5Content, ref EraTaskConfig.EraTaskData eraTaskData)
		{
			//设置avtivityID
			var activityID = string.Empty;
			var activityIDMatch = System.Text.RegularExpressions.Regex.Match(h5Content, @"var\s+activityPageEvaConfigActivityId\s*=\s*[""']([^""']+)[""']");
			if (activityIDMatch.Success)
			{
				activityID = activityIDMatch.Groups[1].Value;
			}
			eraTaskData.ActivityID = activityID;

			//设置title
			var eraTitleString = ExtractJsonAfter(h5Content, "window.__BILIACT_PAGEINFO__ = ");
			var eraTitle = (string?)JObject.Parse(eraTitleString)["title"];
			eraTaskData.TaskTitle = eraTitle;
			_logger.LogInformation("获得的标题：{title}，活动id：{activity_id}", eraTitle, activityID);
		}

		/// <summary>
		/// 设置任务的id，要求自己在里面解析Json数据并进入激励计划的详细页再获取h5数据再设置id
		/// </summary>
		/// <param name="eraPageData">激励计划h5网页内容中的EvaPageData</param>
		/// <param name="eraTaskData">活动数据</param>
		/// <returns>返回被修改过的EraTaskData</returns>
		protected abstract Task<EraTaskConfig.EraTaskData> SetTaskIDs(string eraPageData, EraTaskConfig.EraTaskData eraTaskData);

		/// <summary>
		/// 获得html内的json元素（）若抓取为空会报错
		/// </summary>
		/// <param name="html"></param>
		/// <param name="marker"></param>
		/// <returns></returns>
		protected virtual string ExtractJsonAfter(string html, string marker)
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

		/// <summary>
		/// 查看用户是否可以提取
		/// </summary>
		/// <param name="uid">用户id</param>
		/// <returns></returns>
		protected virtual (bool canQuery, string msg, AccountConfig.User user) TryGetAuthorizedUser(string uid)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var user = accountConfig.Users.FirstOrDefault(q => q.Uid == uid);
			if (user == null)
			{
				_logger.LogWarning("{uid}用户不存在！", uid);
				return new(false, $"{uid}用户不存在！", null);
			}

			if (!user.LiveDatas.Any(q => q.LiveArea == GameName))
			{
				_logger.LogDebug("{user}未开启该分区{area}的直播", user.Uid, GameName);
				return new(false, $"{user.Uid}未开启该分区{GameName}的直播", null);
			}
			return new(true, "", user);
		}

		/// <summary>
		/// 查看游戏的活动是否存在
		/// </summary>
		/// <param name="gameName">游戏名</param>
		/// <returns></returns>
		protected virtual (bool canQuery, string msg, EraTaskConfig.EraTaskData eraTaskData) TryGetEraTaskData(string gameName)
		{
			var eraConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);
			var eraTaskData = eraConfig.EraTaskDatas.FirstOrDefault(q => q.GameName == GameName);
			if (eraTaskData == null)
			{
				_logger.LogWarning("未获取到era信息，可能需要先刷新era数据");
				return new(false, "未获取到era信息，可能需要先刷新era数据", null);
			}
			return new(true, "", eraTaskData);
		}
	}
}
