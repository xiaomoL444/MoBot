using BilibiliLive.Constant;
using BilibiliLive.Handle;
using BilibiliLive.Interaction;
using BilibiliLive.Models;
using BilibiliLive.Models.config;
using BilibiliLive.Models.Live;
using BilibiliLive.Session;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Message;
using MoBot.Handle.Message;
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
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Manager
{
	/// <summary>
	/// 管理直播回话的类
	/// </summary>
	public static class LiveManager
	{

		private static readonly ILogger _logger = GlobalLogger.CreateLogger(typeof(LiveManager));
		private static readonly IDataStorage _dataStorage = GlobalDataStorage.DataStorage;

		private static Process? _childProcess;//子推流程序1

		private static (string version, string build) _livehimeVersion = new("7.19.0.9432", "9432");
		private static List<string> _streamVideoPaths = new();
		private static bool _isGameStart = false;//B站的开发平台的玩法，我用来检测弹幕了（）
		private static List<LiveStreamSession> _sessions = new();

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

		public static async Task StartLive(Action<List<MessageSegment>> sendMessage)
		{

			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");

			//校验参数
			if (!Directory.Exists(streamConfig.StreamVideoDirectory))
			{
				_logger.LogError("视频目录->{path}不存在，请检查配置文件", streamConfig.StreamVideoDirectory);
				sendMessage(MessageChainBuilder.Create().Text("视频目录不存在...߹ - ߹，勾修金sama请检查一下吧").Build());
				return;
			}

			//读取视频文件
			_streamVideoPaths = Directory.GetFiles(streamConfig.StreamVideoDirectory).Where(q => q.EndsWith(".mp4")).OrderBy(q => Path.GetFileName(q)).ToList();
			_logger.LogDebug("找到的所有视频文件{paths}", _streamVideoPaths);

			if (_streamVideoPaths.Count <= 0)
			{
				_logger.LogError("视频目录->{path}下不存在视频源", streamConfig.StreamVideoDirectory);
				sendMessage(MessageChainBuilder.Create().Text("不存在视频源...߹ - ߹，勾修金sama请检查一下吧").Build());
				return;
			}

			//开启直播
			var msgChain = MessageChainBuilder.Create().Text("直播开启中(＾ω＾)，直播状态\n");

			//获取直播姬版本
			try
			{
				var versionData = JObject.Parse(await (await Tool.HttpClient.SendAsync(new(HttpMethod.Get, $"{Constants.GetLivehimeVersion}?system_version=2"))).Content.ReadAsStringAsync());
				_livehimeVersion = new(((string?)versionData["data"]["curr_version"]), ((string?)versionData["data"]["build"]));
			}
			catch (Exception ex)
			{
				sendMessage(MessageChainBuilder.Create().Text("请求版本失败，可能是没有网络连接(｡•́︿•̀｡) ").Build());
				return;
			}

			//开启直播
			foreach (var account in accountConfig.Users)
			{
				if (!account.IsStartLive)
				{
					_logger.LogInformation("{uid}未开启直播选项", account.Uid);
					continue;
				}
				var userCredential = account.UserCredential;
				var userInfo = await UserInteraction.GetUserInfo(userCredential);
				var session = _sessions.FirstOrDefault(q => q.UserCredential.DedeUserID == userCredential.DedeUserID);
				if (session is not null)
				{
					if (session.IsLive)
					{
						_logger.LogInformation("[{user}]已经在直播中，跳过重复开播", userInfo.Data.Name);
						msgChain.Text($"[{userInfo.Data.Name}]：已在直播中\n");
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
				//开启直播
				var result = await CreateLiveSession(userCredential, streamConfig.AreaV2, streamConfig.Platform);
				result.Switch(session =>
				{
					_logger.LogInformation("开播成功");
					_sessions.Add(session);
					msgChain.Text($"[{userInfo.Data.Name}]:开播成功\n");
					return;
				}, errorMsg =>
				{
					_logger.LogError("开播失败");
					msgChain.Text($"[{userInfo.Data.Name}]:{errorMsg.Value}\n");
					return;
				});
			}

			//开启子ffmpeg程序
			if (_childProcess == null || _childProcess.HasExited)
			{
				//若程序不存在或者已经退出（？错误退出）
				_ = Task.Run(async () =>
				{
					int num = streamConfig.Index;
					bool isStream = true;
					while (isStream)
					{
						try
						{
							CancellationTokenSource videoCts = new CancellationTokenSource();//要是中途出错了直接打断这个等待的Task并退出循环

							//设置播放的次序
							if (num >= _streamVideoPaths.Count) num = 0;
							_logger.LogInformation("播放第{i}个视频，路径为：{path}", num, _streamVideoPaths[num]);

							var streamConfig = _dataStorage.Load<StreamConfig>("stream");
							streamConfig.Index = num;
							_dataStorage.Save("stream", streamConfig);

							//设置视频时长
							TimeSpan duration;
							try
							{
								duration = GetVideoDuration(_streamVideoPaths[num]);
							}
							catch (Exception ex)
							{
								_logger.LogWarning(ex, "视频读取失败,{path}", _streamVideoPaths[num]);
								num++;
								continue;
							}
							var child_args = $"-re -loglevel warning -fflags +genpts+igndts+discardcorrupt -i \"{_streamVideoPaths[num]}\" -t {duration.TotalSeconds}  -c copy -mpegts_flags +initial_discontinuity -muxpreload 0 -muxdelay 0  -f mpegts udp://239.0.0.1:11111";
							_logger.LogDebug("ffmpeg子程序参数{args}", child_args);
							_childProcess = new Process
							{

								StartInfo = new ProcessStartInfo()
								{
									FileName = "ffmpeg",
									Arguments = child_args,
									RedirectStandardOutput = true,
									RedirectStandardError = true,
									RedirectStandardInput = true,
									UseShellExecute = false,
									CreateNoWindow = true,
								},
								EnableRaisingEvents = true
							};
							_childProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_child] {info}", e.Data); };
							_childProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_child] {info}", e.Data); };

							_childProcess.Exited += async (s, e) =>
							{
								//0即是正常退出（播放完或者是输入q，这里只有播放完，让他强制退出就使用-1）
								if (_childProcess.ExitCode == 0)
								{
									num++;
									videoCts.Cancel();
									return;
								}
								//windows是-1，linux是137
								if (_childProcess.ExitCode == -1 || _childProcess.ExitCode == 137)
								{
									_logger.LogInformation("子ffmpeg强制退出，结束直播");
									isStream = false;
									videoCts.Cancel();
									return;
								}
								_logger.LogWarning("子程序错误码{exit_code}", _childProcess.ExitCode);
								_logger.LogWarning("子ffmpeg异常退出");

								sendMessage(MessageChainBuilder.Create().Text("子程序退出出现了异常...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
								isStream = false;
								videoCts.Cancel();
							};
							_childProcess.Start();
							_childProcess.BeginOutputReadLine();
							_childProcess.BeginErrorReadLine();

							//等待视频的时长
							try
							{
								await Task.Delay((int)duration.TotalMilliseconds, videoCts.Token);
							}
							catch (Exception)
							{
								_logger.LogDebug("子程序经过{time}秒退出", duration.TotalSeconds);
							}

							using var closeCts = new CancellationTokenSource(10000);
							try
							{
								await _childProcess.WaitForExitAsync(closeCts.Token);
								_logger.LogInformation("直播子程序已关闭");
								continue;
							}
							catch (OperationCanceledException)
							{
								_logger.LogWarning("直播子程序超时未退出");
								sendMessage(MessageChainBuilder.Create().Text("子程序异常超时退出...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
								break;
							}
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "子程序出现错误");
							sendMessage(MessageChainBuilder.Create().Text("子程序出现了错误...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
							break;
						}
					}
				});
			}

			//开启开发平台玩法：
			msgChain.Text("开发平台玩法：");
			if (_isGameStart)
			{
				msgChain.Text("已开启，无需重复开启");
			}
			else
			{
				var startLiveEventResult = await StartOpenLive();
				startLiveEventResult.Switch(none => { _isGameStart = true; msgChain.Text("已开启"); }, error => { msgChain.Text("开启失败！"); });
			}
			msgChain.Text("\n");

			//开启看播
			msgChain.Text("看播：");
			foreach (var user in accountConfig.Users)
			{

				var userInfo = await UserInteraction.GetUserInfo(user.UserCredential);
				foreach (var targetUid in user.ViewLiveUsers)
				{
					var targetUserInfo = await UserInteraction.GetUserInfo(accountConfig.Users.FirstOrDefault(q => q.Uid == targetUid).UserCredential);

					var targetSession = _viewsSessions.FirstOrDefault(q => q.UserCredential.DedeUserID == user.Uid && q.TargetRoomID == targetUserInfo.Data.LiveRoom.RoomId);
					if (targetSession is not null)
					{
						if (targetSession.IsView)
						{
							_logger.LogInformation("user[{user}]:room[{room}]正在观看，跳过", user.Uid, targetUserInfo.Data.LiveRoom.RoomId);
							continue;
						}
						else
						{
							_logger.LogWarning("user[{user}]:room[{room}]已关闭，重新启动", user.Uid, targetUserInfo.Data.LiveRoom.RoomId);
							_viewsSessions.Remove(targetSession);
						}
					}
					_logger.LogDebug("添加[{uid}]的看[{targetRoomID}]直播间", user.Uid, targetUserInfo.Data.LiveRoom.RoomId);
					var session = new ViewStreamSession(user.UserCredential, userInfo.Data.Name, targetUserInfo.Data.Name, targetUserInfo.Data.LiveRoom.RoomId);
					session.Start();
					_viewsSessions.Add(session);
					msgChain.Text($"[{userInfo.Data.Name}]观看[{targetUserInfo.Data.Name}]的直播间").Text("\n");
				}
			}

			sendMessage(msgChain.Build());
		}
		public static async Task StopLive(Action<List<MessageSegment>> sendMessage)
		{
			var msgChain = MessageChainBuilder.Create().Text("直播关闭中，关闭状态\n");

			_logger.LogInformation("关闭直播间");

			var _sessions_copy = _sessions.ToList();
			foreach (var session in _sessions_copy)
			{
				_logger.LogInformation("关闭{user}的直播中", session.UserCredential.DedeUserID);
				var userInfo = await UserInteraction.GetUserInfo(session.UserCredential);
				msgChain.Text($"[{userInfo.Data.Name}]:");

				bool isSessionStop = await session.StopAsync();
				if (!isSessionStop)
				{
					msgChain.Text("串流程序关闭失败！\n");
					continue;
				}

				var stopliveResult = await CloseLiveRoom(session.UserCredential);
				stopliveResult.Switch(
					successMsg =>
					{
						msgChain.Text($"关闭成功{(string.IsNullOrEmpty(successMsg.Value) ? "" : $"({successMsg.Value})")}");
						_sessions.Remove(session);
					}, errorMsg =>
					{
						msgChain.Text(errorMsg.Value);
					});
				msgChain.Text("\n");
			}

			_logger.LogInformation("关闭看播");
			foreach (var session in _viewsSessions.ToList())
			{
				session.Stop();
				_viewsSessions.Remove(session);
				msgChain.Text($"关闭[{session.UserName}]看[{session.TargetUserName}]直播间").Text("\n");
				msgChain.Text(string.Join("\n", session.HeartResult.GroupBy(g => g).Select(r => $"{r.Key.msg}x{r.Count()}"))).Text("\n");
			}

			_logger.LogInformation("关闭玩法中");
			var gameResultMsgChain = MessageChainBuilder.Create();
			msgChain.Text("开发平台玩法：");
			if (_isGameStart)
			{
				var stopGameResult = await StopOpenLive();
				_logger.LogInformation("Result:{result}", stopGameResult);
				msgChain.Text((stopGameResult ? "关闭成功" : "关闭失败"));

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
				msgChain.Text("关闭状态，未开启");
			}
			msgChain.Text("\n");

			if (_childProcess is { HasExited: false })
			{

				using var cts = new CancellationTokenSource(5000);
				_childProcess!.Kill();
				try
				{
					await _childProcess.WaitForExitAsync(cts.Token);
					msgChain.Text("串流程序关闭成功");
					_logger.LogInformation("串流程序关闭成功");
				}
				catch (OperationCanceledException)
				{
					_logger.LogWarning("串流程序关闭失败");
					msgChain.Text("串流程序关闭失败！");
				}
			}

			sendMessage(msgChain.Build());
			_ = Task.Run(async () =>
			{
				await Task.Delay(Random.Shared.Next(500, 1500));
				sendMessage(gameResultMsgChain.Build());
			});
		}

		public static async Task ViewLiveState(Action<List<MessageSegment>> sendMessage)
		{
			var msgChain = MessageChainBuilder.Create();
			msgChain.Text($"串流程序状态：{((_childProcess == null || _childProcess.HasExited) ? "已退出或不存在（请及时关闭推流）" : "Alive! >w<")}\n");
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

		public static async Task FinishGiftTask(Action<List<MessageSegment>> sendMessage)
		{
			sendMessage(MessageChainBuilder.Create().Text("请等待...预计需要一分钟左右时间...").Build());

			var account = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			List<object> datas = new();
#if DEBUG
			datas = new() {
				new{
					face = "http://i1.hdslb.com/bfs/face/b94d505e6be9b2504f6fa23c0030751b23f54e5f.jpg",
					name = "Name",
					info =  "发送弹幕：\r\n ♪ [戀祈]直播间\r\n ♫  点赞成功x3\r\n ♫  您发送弹幕的频率过快x3\r\n ? [心爱子]直播间\r\n ♫  点赞成功x4\r\n ?  您发送弹幕的频率过快x2\r\n发送牛蛙：\r\n ♪ [心爱子]直播间\r\n ♫  投喂牛蛙成功"
				},
				new{
					face = "http://i1.hdslb.com/bfs/face/b94d505e6be9b2504f6fa23c0030751b23f54e5f.jpg",
					name = "Name",
					info =  "发送弹幕：\r\n ♪ [戀祈]直播间\r\n ♫  投喂牛蛙成功"
				},
				new{
					face = "http://i1.hdslb.com/bfs/face/b94d505e6be9b2504f6fa23c0030751b23f54e5f.jpg",
					name = "Name",
					info =  "发送弹幕：\r\n发送牛蛙：\r\n ♪ [戀祈]直播间\r\n ♫  投喂牛蛙成功\r\n ♪ [心爱子]直播间\r\n ♫  投喂牛蛙成功"
				}
			};
#else
			//发送礼物
			foreach (var user in account.Users)
			{

				var userCredential = user.UserCredential;
				var userInfo = await UserInteraction.GetUserInfo(userCredential);

				string msg = string.Empty;
				//发送弹幕
				if (user.SendUserDanmuku.Count != 0)
					msg += "发送弹幕：\n";
				foreach (var uid in user.SendUserDanmuku)
				{
					var danmukuUserInfo = await UserInteraction.GetUserInfo(account.Users.FirstOrDefault(q => q.Uid == uid).UserCredential);
					var room = danmukuUserInfo.Data.LiveRoom.RoomId;
					List<(int code, string msg)> danmukuResult = new();
					//发送六次弹幕
					for (int time = 0; time < 6; time++)
					{
						var danmuku = _danmukus[Random.Shared.Next(0, _danmukus.Count)];
						_logger.LogDebug("{senduser}给{room}发送弹幕{@danmuku}", user.Uid, room, danmuku);
						await Task.Delay(Random.Shared.Next(4*1000, 7*1000));
						var match = await UserInteraction.SendDanmuka(userCredential, room.ToString(), danmuku.danmukuType, danmuku.msg);
						match.Switch(
							None =>
						{
							danmukuResult.Add(new(0, "发送弹幕成功"));
						}, Error =>
						{
							danmukuResult.Add(new(Error.Value.code, Error.Value.msg));
						});
					}
					//点赞结束，汇总
					msg += @$" ♪ [{danmukuUserInfo.Data.Name}]直播间
    ♫  {string.Join("\n ♬  ", danmukuResult.GroupBy(q => q).Select(s => $"{s.Key.msg}x{s.Count()}"))}
";
				}

				//发送牛蛙
				if (user.GiftUsers.Count != 0)
					msg += "发送牛蛙：\n";
				foreach (var uid in user.GiftUsers)
				{
					var targetUserInfo = await UserInteraction.GetUserInfo(account.Users.FirstOrDefault(q => q.Uid == uid).UserCredential);
					var room = targetUserInfo.Data.LiveRoom.RoomId;
					(int code, string msg) result = new();

					await Task.Delay(Random.Shared.Next(500, 1000))
						;
					var match = await UserInteraction.SendLiveGift(userCredential, uid, room.ToString(), "31039");
					match.Switch(
						None =>
						{
							result = (0, "投喂牛蛙成功");
						}, Error =>
						{
							result = (Error.Value.code, Error.Value.msg);
						});

					//牛蛙结束，汇总
					msg += @$" ♪ [{targetUserInfo.Data.Name}]直播间
    ♫  {result.msg}
";
				}

				datas.Add(new
				{
					face = userInfo.Data.Face,
					name = userInfo.Data.Name,
					info = msg
				});
			}
#endif
			//截图界面
			string uuid = Guid.NewGuid().ToString();
			HttpServer.SetNewContent(uuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(new
			{
				data = datas,
				background = ""
			}));

			var base64 = await Webshot.ScreenShot($"{Webshot.GetIPAddress()}/MultiInfoView?id={uuid}");

			sendMessage(MessageChainBuilder.Create().Image("base64://" + base64).Build());
		}

		/// <summary>
		/// 获得视频长度
		/// </summary>
		/// <param name="videoPath">视频路径</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		static TimeSpan GetVideoDuration(string videoPath)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "ffprobe",
					Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			if (double.TryParse(output, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
			{
				return TimeSpan.FromSeconds(seconds);
			}

			throw new Exception($"无法解析 ffprobe 输出: {output}");
		}

		/// <summary>
		/// 开启直播间
		/// <paramref name="userCredential"/>用户凭证（登录信息）
		/// <paramref name="areaV2"/>直播分区
		/// <paramref name="platform"/>直播平台
		/// </summary>
		/// <returns>直播会话，或者一个错误信息返回</returns>
		static async Task<OneOf<LiveStreamSession, Error<string>>> CreateLiveSession(UserCredential userCredential, int areaV2, string platform)
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
						{ "platform", $"{platform}" },
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
				LiveStreamSession liveStreamSession = new(userCredential, platform, rtmp);

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

				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStopLiveApi}?room_id={roomID}&csrf={userCredential.Bili_Jct}&platform={session.Platform}");
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
