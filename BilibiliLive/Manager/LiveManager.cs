using BilibiliLive.Constant;
using BilibiliLive.Handle;
using BilibiliLive.Interaction;
using BilibiliLive.Models;
using BilibiliLive.Models.Config;
using BilibiliLive.Models.Live;
using BilibiliLive.Models.Webshot;
using BilibiliLive.Session;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models;
using MoBot.Core.Models.Message;
using MoBot.Handle.Message;
using MoBot.Infra.PuppeteerSharp.Interface;
using MoBot.Infra.PuppeteerSharp.Interfaces;
using MoBot.Infra.PuppeteerSharp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using OneOf.Types;
using OpenBLive.Client;
using OpenBLive.Client.Data;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Manager
{
	/// <summary>
	/// 管理直播回话的类
	/// </summary>
	public static class LiveManager
	{

		private static readonly ILogger _logger = GlobalSetting.CreateLogger(typeof(LiveManager));
		private static readonly IDataStorage _dataStorage = GlobalSetting.DataStorage;
		private static readonly IWebshot _webshot = GlobalSetting.Webshot;
		private static readonly IWebshotRequestStore _webshotRequestStore = GlobalSetting.WebshotRequestStore;

		public static bool IsLive { get => _sessions.Count != 0; }//若存在会话则是在开播，不管会话是否异常退出

		private static (string version, string build) _livehimeVersion = new("7.19.0.9432", "9432");
		private static bool _isGameStart = false;//B站的开发平台的玩法，我用来检测弹幕了（）
		private static SourceStreamSession _sourceStreamSession = new();//源推流会话
		private static List<LiveStreamSession> _sessions = new();//直播会话

		private static List<ViewStreamSession> _viewsSessions = new();//看播会话
#if DEBUG
		private static bool isDebug = true; //主程序的推流-f flv 要记得改 和 mpegts
#endif
#if !DEBUG
		private static bool isDebug = false; //主程序的推流-f flv 要记得改 和 mpegts
#endif

		private static string streamOpenTime = "";//开启直播的时间，用来记录Data

		private static readonly List<(LiveDanmukuType danmukuType, string msg)> _danmukus = [
			new(LiveDanmukuType.Text, "UpUp我喜欢你"),
			new(LiveDanmukuType.Text, "(●• ̀ω•́ )✧"),
			new(LiveDanmukuType.Text, "ヾ(✿ﾟ▽ﾟ)ノ"),
			new(LiveDanmukuType.Text, "ε(*･ω･)_/ﾟ:･☆"),
			new(LiveDanmukuType.Text, "ο(=·ω＜=)☆kira"),
			new(LiveDanmukuType.Text, "₍˄·͈༝·͈˄*₎◞ ̑̑"),
			new(LiveDanmukuType.Emotion,"upower_[崩坏3·光辉矢愿_比心]"),
			new(LiveDanmukuType.Emotion,"upower_[崩坏3·光辉矢愿_遨游]"),
			new(LiveDanmukuType.Emotion,"upower_[崩坏3·光辉矢愿_回眸]"),
			new(LiveDanmukuType.Emotion,"upower_[崩坏3_吃咸鱼]"),
			new(LiveDanmukuType.Emotion,"upower_[崩坏：星穹铁道_心]")
		];

		private static bool isViewLive = false;//是否在看直播

		//开放平台参数
		static IBApiClient bApiClient = new BApiClient();
		static WebSocketBLiveClient m_WebSocketBLiveClient;
		static InteractivePlayHeartBeat m_PlayHeartBeat;
		static string appId = "";
		static string gameId = "";

		/// <summary>
		/// 开启直播
		/// <paramref name="uidList"/>需要开播的用户id列表
		/// <paramref name="liveArea"/>直播分区，通常为genshin与starrail
		/// </summary>
		/// <returns>返回图查的base64</returns>
		public static async Task<OneOf<Success<string>, Error<string>>> StartLive(List<string> uidList, string liveArea)
		{

			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");

			if (uidList.Count <= 0)
			{
				_logger.LogWarning("提供的uid数量为零！");
				return new Error<string>("提供的uid数量为零！");
			}

			//开启直播

			MultiInfoView multiInfoView = new MultiInfoView();
			var backgroundPath = RandomImage.GetImagePath();
			var backgroundUuid = Guid.NewGuid().ToString();
			_webshotRequestStore.SetNewContent(backgroundUuid, HttpServerContentType.ImagePng, File.ReadAllBytes(backgroundPath));
			multiInfoView.Background = $"{_webshotRequestStore.GetIPAddress()}?id={backgroundUuid}";


			var sourceStreamInitializeResult = await _sourceStreamSession.Initialize();

			if (sourceStreamInitializeResult.IsT0)
			{
				//若成功
			}
			else
			{
				//若失败
				return new Error<string>(sourceStreamInitializeResult.AsT1.Value);
			}

			//获取直播姬版本
			try
			{
				var versionData = JObject.Parse(await (await Tool.HttpClient.SendAsync(new(HttpMethod.Get, $"{Constants.GetLivehimeVersion}?system_version=2"))).Content.ReadAsStringAsync());
				_livehimeVersion = new(((string?)versionData["data"]["curr_version"]), ((string?)versionData["data"]["build"]));
			}
			catch (Exception ex)
			{
				//若失败
				_logger.LogError(ex, "获取直播姬版本失败");
				return new Error<string>("请求版本失败，可能是没有网络连接(｡•́︿•̀｡) ");
			}

			//开启直播
			foreach (var account_uid in uidList)
			{
				//校验用户是否存在
				if (!accountConfig.Users.Any(q => q.Uid == account_uid))
				{
					_logger.LogDebug("{uid}用户不存在", account_uid);
					var uuid = Guid.NewGuid().ToString();
					_webshotRequestStore.SetNewContent(uuid, HttpServerContentType.ImagePng, File.ReadAllBytes("./Assets/BilibiliLive/icon/transparent.png"));
					multiInfoView.Data.Add(new() { Info = "不存在用户!", Name = account_uid, Face = $"{_webshotRequestStore.GetIPAddress()}?id={uuid}" });
					continue;
				}

				//校验用户是否有开播分区的数据
				var account = accountConfig.Users.FirstOrDefault(q => q.Uid == account_uid)!;
				if (!account.LiveDatas.Any(q => q.LiveArea == liveArea))
				{
					_logger.LogDebug("{uid}没有该分区的开播数据", account.Uid, liveArea);
					var uuid = Guid.NewGuid().ToString();
					_webshotRequestStore.SetNewContent(uuid, HttpServerContentType.ImagePng, File.ReadAllBytes("./Assets/BilibiliLive/icon/transparent.png"));
					multiInfoView.Data.Add(new() { Info = "没有该分区的开播数据!", Name = account.Uid, Face = $"{_webshotRequestStore.GetIPAddress()}?id={uuid}" });
					continue;
				}

				//查找用户是否已在直播中
				var userCredential = account.UserCredential;
				var userInfo = await UserInteraction.GetUserInfo(userCredential);
				var session = _sessions.FirstOrDefault(q => q.UserCredential.DedeUserID == userCredential.DedeUserID);
				if (session is not null)
				{
					if (session.IsLive)
					{
						_logger.LogInformation("[{user}]已经在直播中，跳过重复开播", userInfo.Data.Name);
						multiInfoView.Data.Add(new() { Info = "已经在直播中，跳过重复开播", Name = userInfo.Data.Name, Face = userInfo.Data.Face });
						//msgChain.Text($"[{userInfo.Data.Name}]：已在直播中\n");
						continue;
					}
					else
					{
						_logger.LogInformation("[{user}]直播异常，尝试重新开播", userInfo.Data.Name);
						//移除list后重新开播
						_sessions.Remove(session);
					}
				}
				else
				{
					_logger.LogInformation("[{user}]不在直播，创建直播会话中", userInfo.Data.Name);
				}

				//开启直播会话
				var result = await CreateLiveSession(userCredential, streamConfig.LiveAreas.FirstOrDefault(q => q.AreaName == liveArea)!.Area, liveArea);
				result.Switch(session =>
				{
					_logger.LogInformation("开播成功");
					_sessions.Add(session);
					multiInfoView.Data.Add(new() { Info = "开播成功!\n", Name = userInfo.Data.Name, Face = userInfo.Data.Face });
				}, errorMsg =>
				{
					_logger.LogError("开播失败");
					multiInfoView.Data.Add(new() { Info = $"开播失败\n{errorMsg.Value}\n", Name = userInfo.Data.Name, Face = userInfo.Data.Face });
				});
			}

			//开启子ffmpeg程序
			var sourceStreamResult = await _sourceStreamSession.Open();
			multiInfoView.ExtraInfos.Add(sourceStreamResult);

			//开启开发平台玩法：
			string openLiveMsg = "开发平台玩法：";
			if (_isGameStart)
			{
				openLiveMsg += "已开启，无需重复开启";
			}
			else
			{
				var startLiveEventResult = await StartOpenLive();
				startLiveEventResult.Switch(none => { _isGameStart = true; openLiveMsg += "已开启"; }, error => { openLiveMsg += "开启失败！"; });
			}
			multiInfoView.ExtraInfos.Add(openLiveMsg);

			//开启看播
			//msgChain.Text("看播：");
			foreach (var uid in uidList)
			{
				string viewLiveMsg = string.Empty;
				var user = accountConfig.Users.FirstOrDefault(q => q.Uid == uid)!;
				var userInfo = await UserInteraction.GetUserInfo(user.UserCredential);
				foreach (var viewUid in user.LiveDatas.FirstOrDefault(q => q.LiveArea == liveArea)!.ViewLiveUsers)
				{
					//判断用来看播的用户是否存在
					if (!accountConfig.Users.Any(q => q.Uid == viewUid))
					{
						_logger.LogWarning("看播用户{uid}不存在！", viewUid);
						viewLiveMsg += $"看播用户{viewUid}不存在！";
						continue;
					}

					var viewUser = accountConfig.Users.FirstOrDefault(q => q.Uid == viewUid)!;
					var viewUserInfo = await UserInteraction.GetUserInfo(viewUser.UserCredential);
					//是否已经看播中
					var viewSession = _viewsSessions.FirstOrDefault(q => q.UserCredential.DedeUserID == viewUser.Uid && q.TargetRoomID == userInfo.Data.LiveRoom.RoomId);
					if (viewSession is not null)
					{
						if (viewSession.IsView)
						{
							_logger.LogInformation("user[{user}]:room[{room}]正在被观看，跳过", user.Uid, userInfo.Data.LiveRoom.RoomId);
							continue;
						}
						else
						{
							_logger.LogWarning("user[{user}]:room[{room}]已关闭，重新启动", user.Uid, userInfo.Data.LiveRoom.RoomId);
							_viewsSessions.Remove(viewSession);
						}
					}
					var liveAreaData = streamConfig.LiveAreas.FirstOrDefault(q => q.AreaName == liveArea);
					//判断Stream里是否有这个参数
					if (liveAreaData == null)
					{
						_logger.LogWarning("StreamConfig中没有这个直播游戏分区{part}", liveArea);
						multiInfoView.Data.Add(new() { Info = $"StreamConfig中没有这个直播游戏分区{liveArea}\n", Name = userInfo.Data.Name, Face = userInfo.Data.Face });
						continue;
					}
					_logger.LogDebug("添加[{uid}]的看[{targetRoomID}]直播间，大分区{part}，小分区{area}", viewUid, userInfo.Data.LiveRoom.RoomId, liveAreaData.LivePart, liveAreaData.Area);
					//新增看播会话
					var session = new ViewStreamSession(viewUser.UserCredential, viewUserInfo.Data.Name, userInfo.Data.Name, userInfo.Data.LiveRoom.RoomId, liveAreaData.LivePart, liveAreaData.Area);
					session.Start();
					_viewsSessions.Add(session);
					viewLiveMsg += $"<img src='data:image/png;base64,{Convert.ToBase64String((await Tool.HttpClient.SendAsync(new(HttpMethod.Get, viewUserInfo.Data.Face))).Content.ReadAsByteArrayAsync().Result)}' style='padding-left:2vw; vertical-align: middle; width: 3vw;'/><span style='vertical-align: middle;'>[{viewUserInfo.Data.Name}]正在观看直播间</span>\n";
				}
				multiInfoView.Data.FirstOrDefault(q => q.Name == userInfo.Data.Name)!.Info += viewLiveMsg;
			}

			var multiInfoUuid = Guid.NewGuid().ToString();
			_webshotRequestStore.SetNewContent(multiInfoUuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(multiInfoView));

			var base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/MultiInfoView?id={multiInfoUuid}");

			return new Success<string>(base64);

			//sendMessage(msgChain.Build());
		}

		/// <summary>
		/// 关闭直播，返回关闭信息（text）
		/// </summary>
		/// <param name="sendMessage"></param>
		/// <returns></returns>
		public static async Task<string> StopLive()
		{
			_logger.LogInformation("关闭直播间");

			var msg = "直播关闭中，关闭状态\n";

			foreach (var session in _sessions.ToList())
			{
				_logger.LogInformation("关闭{user}的直播中", session.UserCredential.DedeUserID);
				var userInfo = await UserInteraction.GetUserInfo(session.UserCredential);
				msg += $"[{userInfo.Data.Name}]:";

				bool isSessionStop = await session.StopAsync();
				if (!isSessionStop)
				{
					msg += "串流程序关闭失败！\n";
					continue;
				}

				var stopliveResult = await CloseLiveRoom(session.UserCredential);
				stopliveResult.Switch(
					successMsg =>
					{
						msg += $"关闭成功{(string.IsNullOrEmpty(successMsg.Value) ? "" : $"({successMsg.Value})")}";
						_sessions.Remove(session);
					}, errorMsg =>
					{
						msg += errorMsg.Value;
					});
				msg += "\n";
			}

			_logger.LogInformation("关闭看播");
			foreach (var session in _viewsSessions.ToList())
			{
				session.Stop();
				_viewsSessions.Remove(session);
				msg += $"关闭[{session.UserName}]看[{session.TargetUserName}]直播间\n";
				msg += string.Join("\n", session.HeartResult.GroupBy(g => g).Select(r => $"{r.Key.msg}x{r.Count()}")) + "\n";
			}

			_logger.LogInformation("关闭玩法中");
			var gameResultMsgChain = MessageChainBuilder.Create();
			msg += "开发平台玩法：";
			if (_isGameStart)
			{
				var stopGameResult = await StopOpenLive();
				_logger.LogInformation("Result:{result}", stopGameResult);
				msg += (stopGameResult ? "关闭成功" : "关闭失败");

				if (!_isGameStart)
				{
					//若活动玩法成功关闭就发送
					var liveLogs = _dataStorage.Load<LiveEventLog>($"BilibiliLive_{streamOpenTime}", MoBot.Core.Models.DirectoryType.Data).logs;
					var showLogNum = 4;
					gameResultMsgChain.Text(@$"这次的推流很成功哦~
(●• ̀ω•́ )✧末酱在直播间看到了一些有趣的消息
-------------
{String.Join("\n", liveLogs.Take(showLogNum).ToDictionary().Values)}{(liveLogs.Count > showLogNum ? $"\n以及其他{liveLogs.Count - showLogNum}条信息......" : "")}
-------------
可以来这里查看哦*⸜( •ᴗ• )⸝*:BilibiliLive_{streamOpenTime}");
				}
			}
			else
			{
				msg += "关闭状态，未开启";
			}
			msg += "\n";

			await _sourceStreamSession.Close();

			return msg;

			//_ = Task.Run(async () =>
			//{
			//	await Task.Delay(Random.Shared.Next(500, 1500));
			//	sendMessage(gameResultMsgChain.Build());
			//});
		}

		public static async Task ViewLiveState(Action<List<MessageSegment>> sendMessage)
		{
			var msgChain = MessageChainBuilder.Create();
			msgChain.Text($"串流程序状态：{(_sourceStreamSession.IsStream ? "已退出或不存在（请及时关闭推流）" : "Alive! >w<")}\n");
			foreach (var session in _sessions)
			{
				var userInfo = await UserInteraction.GetUserInfo(session.UserCredential);
				var userLiveRoomInfo = await UserInteraction.GetUserRoomInfo(session.UserCredential.DedeUserID);
				msgChain.Text($"[{userInfo.Data.Name}]：会话：{(session.IsLive ? "Alive! >w<" : "已退出")}，直播间(未做)：{(userLiveRoomInfo.Data.LiveStatus == 0 ? "未开播" : "已开播")}");
				msgChain.Text("\n");
			}
			msgChain.Text($"玩法：{(_isGameStart ? "Alive >w<" : "已关闭")}").Text("\n");
			msgChain.Text($"看播：\n{string.Join("\n", _viewsSessions.Select(s => $"♪[{s.UserName}]观看[{s.TargetUserName}]的直播间：{(s.IsView ? "存活" : "已关闭")}\n  ♫  {string.Join("  ♫  ", s.HeartResult.GroupBy(g => g).Select(r => $"{r.Key.msg}x{r.Count()}"))}"))}");

			sendMessage(msgChain.Build());
			return;
		}

		public static async Task<OneOf<Success<string>, Error<string>>> FinishGiftTask()
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			MultiInfoView multiInfoView = new();
#if DEBUG
			multiInfoView = new()
			{
				Background = "",
				ExtraInfos = ["测试"],
				Data = [
					new()
				{
					Name = "测试",
					Info = "发送弹幕：\r\n ♪ [戀祈]直播间\r\n ♫  点赞成功x3\r\n ♫  您发送弹幕的频率过快x3\r\n ? [心爱子]直播间\r\n ♫  点赞成功x4\r\n ?  您发送弹幕的频率过快x2\r\n发送牛蛙：\r\n ♪ [心爱子]直播间\r\n ♫  投喂牛蛙成功",
					Face = "http://i1.hdslb.com/bfs/face/b94d505e6be9b2504f6fa23c0030751b23f54e5f.jpg" },
					new()
				{
					Name = "测试",
					Info = "发送弹幕：\r\n ♪ [戀祈]直播间\r\n ♫  点赞成功x3\r\n ♫  您发送弹幕的频率过快x3\r\n ? [心爱子]直播间\r\n ♫  点赞成功x4\r\n ?  您发送弹幕的频率过快x2\r\n发送牛蛙：\r\n ♪ [心爱子]直播间\r\n ♫  投喂牛蛙成功",
					Face = "http://i1.hdslb.com/bfs/face/b94d505e6be9b2504f6fa23c0030751b23f54e5f.jpg" },
					new()
				{
					Name = "测试",
					Info = "发送弹幕：\r\n ♪ [戀祈]直播间\r\n ♫  点赞成功x3\r\n ♫  您发送弹幕的频率过快x3\r\n ? [心爱子]直播间\r\n ♫  点赞成功x4\r\n ?  您发送弹幕的频率过快x2\r\n发送牛蛙：\r\n ♪ [心爱子]直播间\r\n ♫  投喂牛蛙成功",
					Face = "http://i1.hdslb.com/bfs/face/b94d505e6be9b2504f6fa23c0030751b23f54e5f.jpg" }
					]

			};
#else
			if (_sessions.Count <= 0)
			{
				_logger.LogWarning("没有正在开播的会话");
				return new Error<string>("没有开播的会话哦");
			}
			//发送礼物（查找开播中的用户和分区）
			foreach (var session in _sessions)
			{
				//查找用户合法性
				var user = accountConfig.Users.FirstOrDefault(q => q.Uid == session.UserCredential.DedeUserID);//正在直播的用户
				if (user == null)
				{
					_logger.LogWarning("查找用户失败！{uid}", session.UserCredential.DedeUserID);
					continue;
				}
				var userCredential = session.UserCredential;
				var userInfo = await UserInteraction.GetUserInfo(userCredential);
				var liveAreaData = user.LiveDatas.FirstOrDefault(q => q.LiveArea == session.LiveArea);
				if (liveAreaData == null)
				{
					_logger.LogWarning("{uid}不存在直播分区数据！", session.UserCredential.DedeUserID);
					continue;
				}

				//判断会话是否存活
				if (!session.IsLive)
				{
					_logger.LogWarning("会话关闭，无法投喂");
					multiInfoView.Data.Add(new() { Name = userInfo.Data.Name, Face = userInfo.Data.Face, Info = "会话已关闭，无法投喂" });
					continue;
				}

				var room = userInfo.Data.LiveRoom.RoomId;

				string msg = string.Empty;
				//发送弹幕
				if (liveAreaData.SendUserDanmuku.Count != 0)
					msg += "发送弹幕：\n";
				foreach (var sendConfig in liveAreaData.SendUserDanmuku)
				{
					var danmukuUser = accountConfig.Users.FirstOrDefault(q => q.Uid == sendConfig.Key);
					if (danmukuUser == null)
					{
						_logger.LogWarning("找不到用户{uid}", sendConfig.Key);
						msg += $"用户不存在{sendConfig}\n";
						continue;
					}
					var danmukuUserInfo = await UserInteraction.GetUserInfo(danmukuUser.UserCredential);

					List<(int code, string msg)> danmukuResult = new();
					//发送六次弹幕
					for (int time = 0; time < sendConfig.Value; time++)
					{
						var danmukuList = danmukuUser.Uid == "609872107" ? _danmukus : _danmukus.Where(q => q.danmukuType == LiveDanmukuType.Text).ToList();
						var danmuku = danmukuList[Random.Shared.Next(0, danmukuList.Count)];
						_logger.LogDebug("{senduser}给{room}发送弹幕{@danmuku}", danmukuUserInfo.Data.Name, room, danmuku);
						await Task.Delay(Random.Shared.Next(4 * 1000, 7 * 1000));
						try
						{
							var match = await UserInteraction.SendDanmuka(danmukuUser.UserCredential, room.ToString(), danmuku.danmukuType, danmuku.msg);
							match.Switch(
								None =>
								{
									danmukuResult.Add(new(0, "弹幕发送成功~"));
								}, Error =>
								{
									danmukuResult.Add(new(Error.Value.code, Error.Value.msg));
								});
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "发送弹幕请求失败");
							danmukuResult.Add(new(-1, "发送弹幕请求失败"));

						}

					}

					var iconBase64 = Convert.ToBase64String(await (await Tool.HttpClient.SendAsync(new(HttpMethod.Get, danmukuUserInfo.Data.Face))).Content.ReadAsByteArrayAsync());
					msg += string.Join("\n", danmukuResult.GroupBy(q => q).Select(s => $"<img src='data:image/png;base64,{iconBase64}' style='padding-left:2vw; vertical-align: middle; width: 3vw;'/><span style='vertical-align: middle;'>[{danmukuUserInfo.Data.Name}]：{s.Key.msg} x {s.Count()}</span>")) + "\n";
				}

				//发送牛蛙
				if (liveAreaData.GiftUsers.Count != 0)
					msg += "发送牛蛙：\n";
				foreach (var sendConfig in liveAreaData.GiftUsers)
				{
					var giftUser = accountConfig.Users.FirstOrDefault(q => q.Uid == sendConfig.Key);
					if (giftUser == null)
					{
						_logger.LogWarning("找不到用户{uid}", sendConfig.Key);
						msg += $"用户不存在{sendConfig}\n";
						continue;
					}
					var giftUserInfo = await UserInteraction.GetUserInfo(giftUser.UserCredential);
					List<(int code, string msg)> result = new();
					for (int time = 0; time < sendConfig.Value; time++)
					{
						await Task.Delay(Random.Shared.Next(500, 1000));
						try
						{
							var match = await UserInteraction.SendLiveGift(giftUser.UserCredential, user.Uid, room.ToString(), "31039");
							match.Switch(
								None =>
								{
									result.Add(new(0, "投喂牛蛙成功"));
								}, Error =>
								{
									result.Add(new(Error.Value.code, Error.Value.msg));
								});
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "投喂礼物请求失败");
							result.Add(new(-1, "投喂礼物请求失败"));
						}

					}
					//牛蛙结束，汇总
					var iconBase64 = Convert.ToBase64String(await (await Tool.HttpClient.SendAsync(new(HttpMethod.Get, giftUserInfo.Data.Face))).Content.ReadAsByteArrayAsync());
					msg += string.Join("\n", result.GroupBy(q => q).Select(s => $"<img src='data:image/png;base64,{iconBase64}' style='padding-left:2vw; vertical-align: middle; width: 3vw;'/><span style='vertical-align: middle;'>[{giftUserInfo.Data.Name}]：{s.Key.msg} x {s.Count()}</span>")) + "\n";
				}

				multiInfoView.Data.Add(new() { Name = userInfo.Data.Name, Face = userInfo.Data.Face, Info = msg });
			}
#endif
			//截图界面
			string uuid = Guid.NewGuid().ToString();
			_webshotRequestStore.SetNewContent(uuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(multiInfoView));

			var base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/MultiInfoView?id={uuid}");

			return new Success<string>(base64);
			//sendMessage(MessageChainBuilder.Create().Image("base64://" + base64).Build());
		}

		/// <summary>
		/// 开启直播间
		/// <paramref name="userCredential"/>用户凭证（登录信息）
		/// <paramref name="areaV2"/>直播分区
		/// </summary>
		/// <returns>直播会话，或者一个错误信息返回</returns>
		static async Task<OneOf<LiveStreamSession, Error<string>>> CreateLiveSession(UserCredential userCredential, int areaV2, string liveArea)
		{
			try
			{
				//挨个获取roomID和rtmp_url

				var getLiveRoomData = await UserInteraction.GetUserRoomInfo(userCredential.DedeUserID);

				//校验直播信息是否正确
				if (getLiveRoomData is not { Code: 0 })
				{
					_logger.LogError("获取{user}直播信息错误", userCredential.DedeUserID);
					return new Error<string>(getLiveRoomData?.Message ?? "获取直播间信息遇到未知错误");
				}
				var roomID = getLiveRoomData.Data.RoomId;

				//尝试开启直播间
				var startliveRequest = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStartLiveAPI}");
				startliveRequest.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
				Dictionary<string, string> param = new() {
						{ "room_id", $"{roomID}" },
						{ "area_v2", $"{areaV2}" },
						{ "platform", "pc_link" },
						{ "csrf", $"{userCredential.Bili_Jct}" },
						{ "csrf_token", $"{userCredential.Bili_Jct}" },
						{ "version", $"{_livehimeVersion.version}" },
						{ "build", $"{_livehimeVersion.build}" },
						{ "ts", $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}" },
				};
				var app_key = "aae92bc66f3edfab";
				var app_sec = "af125a0d5279fd576c1b4418a3e8276d";
				param = UserInteraction.AppSign(param, app_key, app_sec);
				startliveRequest.Content = new FormUrlEncodedContent(param);
				var startliveResponse = await Tool.HttpClient.SendAsync(startliveRequest);
				var startliveResponseString = await startliveResponse.Content.ReadAsStringAsync();
				_logger.LogDebug("开启{uid}直播间的回复{@response}", userCredential.DedeUserID, startliveResponseString);

				var startliveData = JsonConvert.DeserializeObject<StartLiveRsp>(startliveResponseString);
				//校验开播信息是否正确
				if (startliveData is not { Code: 0 })
				{
					_logger.LogError("{user}开播失败", userCredential.DedeUserID);

					switch (startliveData?.Code)
					{
						case 60024:
							_logger.LogError("人脸验证链接：{url}", startliveData.Data.Qr);
							break;
						default:
							break;
					}
					return new Error<string>(startliveData?.Message ?? "获取开播信息遇到未知错误");
				}
				_logger.LogInformation("直播间{uid}开启成功", userCredential.DedeUserID);

				//下方开启会话直播
				var rtmp = startliveData?.Data.Rtmp.Addr + JsonConvert.DeserializeObject<string>($"\"{startliveData?.Data.Rtmp.Code}\"");
				LiveStreamSession liveStreamSession = new(userCredential, rtmp, liveArea);

				return liveStreamSession;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "开启{uid}直播间失败", userCredential.DedeUserID);
				return new Error<string>("开启直播间遇到未知错误");
			}
		}

		/// <summary>
		/// 关闭远程直播间
		/// </summary>
		/// <returns></returns>
		static async Task<OneOf<Success<string>, Error<string>>> CloseLiveRoom(UserCredential userCredential)
		{

			try
			{
				var session = _sessions.FirstOrDefault(q => q.UserCredential == userCredential);
				if (session is null)
				{
					_logger.LogError("不存在id为{UID}的用户会话", userCredential.DedeUserID);
					return new Error<string>("不存在用户会话");
				}
				//其实换成获取用户个人信息就可以获取直播间
				var userData = await UserInteraction.GetUserInfo(userCredential);
				var roomID = userData.Data.LiveRoom.RoomId;

				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStopLiveApi}?room_id={roomID}&csrf={userCredential.Bili_Jct}&platform=pc_link");
				httpRequestMessage.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
				var response = await Tool.HttpClient.SendAsync(httpRequestMessage);

				var stopliveResponseString = await response.Content.ReadAsStringAsync();
				_logger.LogDebug("关闭{user}直播间的回复{@response}", userCredential.DedeUserID, stopliveResponseString);
				var responseJson = JsonConvert.DeserializeObject<StopLiveRsp>(stopliveResponseString);

				if (responseJson is not { Code: 0 })
				{
					return new Error<string>($"关闭失败，CODE：{responseJson.Code}，Msg：{responseJson.Message}");
				}
				return new Success<string>(responseJson.Message);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "关闭{uid}直播间失败", userCredential.DedeUserID);
				return new Error<string>("关闭失败，未知错误");
			}
		}


		/// <summary>
		/// 开启监控直播间事件
		/// </summary>
		/// <returns></returns>
		static async Task<OneOf<None, Error<string>>> StartOpenLive()
		{
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");
			string AccessKeyId = streamConfig.LiveOpenPlatForm.AccessKeyId;
			string AccessKeySecret = streamConfig.LiveOpenPlatForm.AccessKeySecret;
			string AppId = streamConfig.LiveOpenPlatForm.AppId;
			string Code = streamConfig.LiveOpenPlatForm.Code;

			string game_id = string.Empty;

			//是否为测试环境（一般用户可无视，给专业对接测试使用）
			BApi.isTestEnv = false;

			SignUtility.accessKeyId = AccessKeyId;
			SignUtility.accessKeySecret = AccessKeySecret;
			appId = AppId;
			var code = Code;


			var startInfo = new AppStartInfo();

			//Console.WriteLine("请输入自动关闭时间,不输入默认30秒");
			//var closeTimeStr = Console.ReadLine();
			//if (string.IsNullOrEmpty(closeTimeStr))
			//{
			//	closeTimeStr = "30";
			//}

			if (!string.IsNullOrEmpty(appId))
			{
				startInfo = await bApiClient.StartInteractivePlay(code, appId);

				_logger.LogDebug("B站直播开放平台的开始信息{@startInfo}", startInfo);

				if (startInfo?.Code != 0)
				{
					_logger.LogError("B站直播开放平台的开始信息有误{startinfo}", startInfo?.Message);
					await StopOpenLive();
					return new Error<string>("开放平台信息有误");
				}

				gameId = startInfo?.Data?.GameInfo?.GameId;
				if (gameId != null)
				{
					game_id = gameId;
					_logger.LogInformation("B站直播开放平台成功开启，开始心跳，场次ID: ", gameId);

					//心跳API（用于保持在线）
					m_PlayHeartBeat = new InteractivePlayHeartBeat(gameId);
					m_PlayHeartBeat.HeartBeatError += (string json) => { JsonConvert.DeserializeObject<EmptyInfo>(json); _logger.LogWarning("{time}心跳失败{json}", DateTime.Now, json); };
					m_PlayHeartBeat.HeartBeatSucceed += () => { _logger.LogDebug("{time}心跳成功", DateTime.Now); };
					m_PlayHeartBeat.Start();

					streamOpenTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss_zz");

					Action<string> WriteLog = (string content) =>
					{
						var logList = _dataStorage.Load<LiveEventLog>($"BilibiliLive_{streamOpenTime}", MoBot.Core.Models.DirectoryType.Data);
						_logger.LogInformation(content);
						logList.logs.Add(new($"[{DateTime.Now.ToString("O")}]", $"{content}"));
						_dataStorage.Save($"BilibiliLive_{streamOpenTime}", logList, MoBot.Core.Models.DirectoryType.Data);
					};

					//长链接（用户持续接收服务器推送消息）

					m_WebSocketBLiveClient = new WebSocketBLiveClient(startInfo.GetWssLink(), startInfo.GetAuthBody());
					m_WebSocketBLiveClient.OnDanmaku += (dm) => { WriteLog($"[{dm.userName}]:{dm.msg}"); };//弹幕事件
					m_WebSocketBLiveClient.OnGift += (sendGift) => { WriteLog($"[{sendGift.userName}]:(赠送了{sendGift.giftNum}个[{sendGift.giftName}])"); };//礼物事件
					m_WebSocketBLiveClient.OnGuardBuy += (guard) => { WriteLog($"[{guard.userInfo.userName}]:(充值了{(guard.guardUnit == "月" ? (guard.guardNum + "个月") : guard.guardUnit.TrimStart('*'))}[{(guard.guardLevel == 1 ? "总督" : guard.guardLevel == 2 ? "提督" : "舰长")}]大航海)"); };//大航海事件
					m_WebSocketBLiveClient.OnSuperChat += (superChat) => { WriteLog($"[{superChat.userName}]:({superChat.rmb}元的醒目留言内容)：{superChat.message}"); };//SC事件
					m_WebSocketBLiveClient.OnLike += (like) => { WriteLog($"[{like.uname}]:(点赞了{like.unamelike_count}次)"); };//点赞事件(点赞需要直播间开播才会触发推送)
					m_WebSocketBLiveClient.OnEnter += (enter) => { WriteLog($"[{enter.uname}]进入房间"); };//观众进入房间事件
					m_WebSocketBLiveClient.OnLiveStart += (liveStart) => { WriteLog($"直播间[{liveStart.room_id}]开始直播，分区ID：【{liveStart.area_id}】,标题为【{liveStart.title}】"); };//直播间开始直播事件
					m_WebSocketBLiveClient.OnLiveEnd += (liveEnd) => { WriteLog($"直播间[{liveEnd.room_id}]直播结束，分区ID：【{liveEnd.area_id}】,标题为【{liveEnd.title}】"); };//直播间停止直播事件

					m_WebSocketBLiveClient.Connect(new TimeSpan(0, 30, 0));
					return new None();
				}
				else
				{
					_logger.LogError("开启玩法错误: {@info}", startInfo);
					await StopOpenLive();
					return new Error<string>("开启玩法错误");
				}
			}
			else
			{
				_logger.LogError("AppID不存在: {appId}", appId);
				return new Error<string>("AppID不存在");
			}
		}

		/// <summary>
		/// 关闭开放平台监控直播间事件
		/// </summary>
		/// <returns></returns>
		static async Task<bool> StopOpenLive()
		{
			try
			{
				var ret = await bApiClient.EndInteractivePlay(appId, gameId);
				if (m_PlayHeartBeat is not null)
				{
					m_PlayHeartBeat.Stop();
					m_PlayHeartBeat = null;
				}
				_logger.LogInformation("关闭玩法:{@ret} ", ret);
				_logger.LogInformation("断开心跳");

				_isGameStart = false;
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "关闭玩法失败");
				return false;
			}

		}
	}
}
