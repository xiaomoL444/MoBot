using BilibiliLive.Constant;
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
using OneOf;
using OneOf.Types;
using OpenBLive.Client;
using OpenBLive.Client.Data;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using HttpClient = BilibiliLive.Tool.HttpClient;

namespace BilibiliLive.Handle
{
	sealed class LiveStreamSession
	{
		private readonly ILogger<LiveStreamSession> _logger;

		public UserCredential UserCredential { get; } = new();//这位B站用户的个人信息
		public string Platform { get; } = string.Empty;
		public bool IsLive = false;//是否还在直播
		public Action FailCallback = () => { };//异常退出时触发错误回调（通常是关闭直播链接）
		public (string code, string msg) ExitState = new();

		private Process _process = new();//直播的ffmpeg程序

		private string _rtmp = string.Empty;//直播的远程推流连接
		public LiveStreamSession(UserCredential userCredential, string platform, string rtmp, ILogger<LiveStreamSession> logger)
		{
			_logger = logger;
			Platform = platform;
			UserCredential = userCredential;
			_rtmp = rtmp;
			Start();
		}
		void Start()
		{
			IsLive = true;
			//主ffmpeg程序
			var args = $"-fflags +genpts -err_detect ignore_err -ignore_unknown -flags low_delay -i udp://239.0.0.1:11111 -c copy -f flv \"{_rtmp}\"";
			_logger.LogDebug("{uid}直播会话程序参数{args}", UserCredential.DedeUserID, args);
			_process = new Process
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "ffmpeg",
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				},
				EnableRaisingEvents = true
			};
			_process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_main] {info}", e.Data); };
			_process.ErrorDataReceived += (s, e) => { if (string.IsNullOrEmpty(e.Data)) return; _logger.LogDebug("[ffmpeg_main] {info}", e.Data); };
			_process.Exited += (s, e) =>
			{
				IsLive = false;
				ExitState = new($"{_process.ExitCode}", $"退出码：{_process.ExitCode}");
				if (_process.ExitCode == 0)
				{
					_logger.LogInformation("{uid}直播会话手动退出", UserCredential.DedeUserID);
					return;
				}
				_logger.LogError("{uid}直播会话异常退出，CODE:{exit_code}", UserCredential.DedeUserID, _process.ExitCode);
				FailCallback();

				//await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("主程序退出出现了一些异常...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
			};
			_process.Start();
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();
		}

		/// <summary>
		/// 关闭程序
		/// </summary>
		/// <returns>返回是否关闭成功</returns>
		public async Task<bool> StopAsync()
		{
			IsLive = false;
			if (_process is not { HasExited: true })
			{
				_process!.StandardInput.WriteLine("q");
				_process.StandardInput.Flush();

				using var cts = new CancellationTokenSource(5000);
				try
				{
					await _process.WaitForExitAsync(cts.Token);
					_logger.LogInformation("{uid}的直播会话已关闭", UserCredential.DedeUserID);
					return true;
				}
				catch (OperationCanceledException)
				{
					_logger.LogWarning("{uid}的直播会话未退出，超时", UserCredential.DedeUserID);
					return false;
				}
			}
			_logger.LogWarning("该直播会话不存在亦或是已退出，无法重新退出");
			return true;
		}
	}
	/// <summary>
	/// 控制串流的小家伙
	/// </summary>
	public class StreamHandle : IMessageHandle<Group>
	{
		private readonly long _opGroupID = Constant.Constants.OPGroupID;
		private readonly long _opAdmin = Constant.Constants.OPAdmin;

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;
		private readonly ILoggerFactory _loggerFactory;

		private Process? _childProcess;//子推流程序1

		private (string version, string build) _livehimeVersion = new("7.19.0.9432", "9432");
		private List<string> _streamVideoPaths = new();
		private bool _isGameStart = false;//B站的开发平台的玩法，我用来检测弹幕了（）
		private List<LiveStreamSession> _sessions = new();
#if DEBUG
		private bool isDebug = true; //主程序的推流-f flv 要记得改 和 mpegts
#endif
#if !DEBUG
		private bool isDebug = false; //主程序的推流-f flv 要记得改 和 mpegts
#endif

		private string streamOpenTime = "";//开启直播的时间，用来记录Data

		public StreamHandle(
			ILogger<StreamHandle> logger,
			IDataStorage dataStorage,
			ILoggerFactory loggerFactory
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
			_loggerFactory = loggerFactory;
		}

		public Task Initial()
		{
			return Task.CompletedTask;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && (message.IsMsg("/开始推流") || message.IsMsg("/关闭推流") || message.IsMsg("/推流状态") || message.IsMsg("/投喂任务"))) return Task.FromResult(true);

			return Task.FromResult(false);
		}

		public Task HandleAsync(Group message)
		{
			var commonds = message.SplitMsg();
			switch (commonds[0])
			{
				case "/开始推流":
					StartStream(message);
					break;
				case "/关闭推流":
					StopStream(message);
					break;
				case "/推流状态":
					ViewStreamState(message);
					break;
				case "/投喂任务":
					FinishGiftTask(message);
					break;
			}
			return Task.CompletedTask;
		}
		async void StartStream(Group group)
		{

			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");

			//校验参数
			if (!Directory.Exists(streamConfig.StreamVideoDirectory))
			{
				_logger.LogError("视频目录->{path}不存在，请检查配置文件", streamConfig.StreamVideoDirectory);
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("视频目录不存在...߹ - ߹，勾修金sama请检查一下吧").Build());
				return;
			}

			//读取视频文件
			_streamVideoPaths = Directory.GetFiles(streamConfig.StreamVideoDirectory).Where(q => q.EndsWith(".mp4")).OrderBy(q => Path.GetFileName(q)).ToList();
			_logger.LogDebug("找到的所有视频文件{paths}", _streamVideoPaths);

			if (_streamVideoPaths.Count <= 0)
			{
				_logger.LogError("视频目录->{path}下不存在视频源", streamConfig.StreamVideoDirectory);
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("不存在视频源...߹ - ߹，勾修金sama请检查一下吧").Build());
				return;
			}

			//开启直播
			var msgChain = MessageChainBuilder.Create().Text("直播开启中(＾ω＾)，直播状态\n");

			//获取直播姬版本
			try
			{
				var versionData = JObject.Parse(await (await HttpClient.SendAsync(new(HttpMethod.Get, $"{Constants.GetLivehimeVersion}?system_version=2"))).Content.ReadAsStringAsync());
				_livehimeVersion = new(((string?)versionData["data"]["curr_version"]), ((string?)versionData["data"]["build"]));
			}
			catch (Exception ex)
			{
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("请求版本失败，可能是没有网络连接(｡•́︿•̀｡) ").Build());
				return;
			}

			//开启直播
			foreach (var account in accountConfig.Users)
			{
				var userCredential = account.UserCredential;
				var userInfo = await BilibiliApiTool.GetUserInfo(userCredential);
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
				var result = await StartLive(userCredential, streamConfig.AreaV2, streamConfig.Platform);
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
							var child_args = $"-re -fflags +genpts+igndts+discardcorrupt -i \"{_streamVideoPaths[num]}\" -t {duration.TotalSeconds}  -c copy -mpegts_flags +initial_discontinuity -muxpreload 0 -muxdelay 0  -f mpegts udp://239.0.0.1:11111";
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

								await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序退出出现了异常...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
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
								await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序异常超时退出...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
								break;
							}
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "子程序出现错误");
							await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序出现了错误...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
							break;
						}
					}
				});
			}


			msgChain.Text("开发平台玩法：");
			if (_isGameStart)
			{
				msgChain.Text("已开启，无需重复开启");
			}
			else
			{
				var startLiveEventResult = await StartLiveEvent();
				startLiveEventResult.Switch(none => { _isGameStart = true; msgChain.Text("已开启"); }, error => { msgChain.Text("开启失败！"); });
			}
			//msgChain.Text("\n");


			await MessageSender.SendGroupMsg(group.GroupId, msgChain.Build());
		}
		async void StopStream(Group group)
		{
			var msgChain = MessageChainBuilder.Create().Text("直播关闭中，关闭状态\n");

			_logger.LogInformation("关闭直播间");

			var _sessions_copy = _sessions.ToList();
			foreach (var session in _sessions_copy)
			{
				_logger.LogInformation("关闭{user}的直播中", session.UserCredential.DedeUserID);
				var userInfo = await BilibiliApiTool.GetUserInfo(session.UserCredential);
				msgChain.Text($"[{userInfo.Data.Name}]:");

				bool isSessionStop = await session.StopAsync();
				if (!isSessionStop)
				{
					msgChain.Text("串流程序关闭失败！\n");
					continue;
				}

				var stopliveResult = await StopLive(session.UserCredential);
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

			_logger.LogInformation("关闭玩法中");
			var gameResultMsgChain = MessageChainBuilder.Create();
			msgChain.Text("开发平台玩法：");
			if (_isGameStart)
			{
				var stopGameResult = await StopLiveEvent();
				_logger.LogInformation("Result:{result}", stopGameResult);
				msgChain.Text((stopGameResult ? "关闭成功" : "关闭失败"));

				if (!_isGameStart)
				{
					//若活动玩法成功关闭就发送
					var liveLogs = _dataStorage.Load<LiveEventLog>($"BilibiliLive_{streamOpenTime}", "data").logs;
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

			await MessageSender.SendGroupMsg(group.GroupId, msgChain.Build());
			_ = Task.Run(async () =>
			{
				await Task.Delay(Random.Shared.Next(500, 1500));
				await MessageSender.SendGroupMsg(group.GroupId, gameResultMsgChain.Build());
			});
		}

		async void ViewStreamState(Group group)
		{
			var msgChain = MessageChainBuilder.Create();
			msgChain.Text($"串流程序状态：{((_childProcess == null || _childProcess.HasExited) ? "已退出或不存在（请及时关闭推流）" : "Alive! >w<")}\n");
			foreach (var session in _sessions)
			{
				var userInfo = await BilibiliApiTool.GetUserInfo(session.UserCredential);
				var userLiveRoomInfo = await BilibiliApiTool.GetUserRoomInfo(session.UserCredential.DedeUserID);
				msgChain.Text($"[{userInfo.Data.Name}]：会话：{(session.IsLive ? "Alive! >w<" : "已退出")}，直播间(未做)：{(userLiveRoomInfo.Data.LiveStatus == 0 ? "未开播" : "已开播")}");
				msgChain.Text("\n");
			}
			msgChain.Text($"玩法：{(_isGameStart ? "Alive >w<" : "已关闭")}").Text("\n");

			await MessageSender.SendGroupMsg(group.GroupId, msgChain.Build());
			return;
		}

		async void FinishGiftTask(Group group)
		{

		}

		/// <summary>
		/// 获得视频长度
		/// </summary>
		/// <param name="videoPath">视频路径</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		TimeSpan GetVideoDuration(string videoPath)
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
		async Task<OneOf<LiveStreamSession, Error<string>>> StartLive(UserCredential userCredential, int areaV2, string platform)
		{
			try
			{
				//挨个获取roomID和rtmp_url

				var getLiveRoomData = await BilibiliApiTool.GetUserRoomInfo(userCredential.DedeUserID);

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
				param = BilibiliApiTool.AppSign(param, app_key, app_sec);
				startliveRequest.Content = new FormUrlEncodedContent(param);
				var startliveResponse = await HttpClient.SendAsync(startliveRequest);
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
				LiveStreamSession liveStreamSession = new(userCredential, platform, rtmp, _loggerFactory.CreateLogger<LiveStreamSession>());

				return liveStreamSession;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "开启{uid}直播间失败", userCredential.DedeUserID);
				return new Error<string>("开启直播间遇到未知错误");
			}
		}

		/// <summary>
		/// 关闭直播间
		/// </summary>
		/// <returns></returns>
		async Task<OneOf<Success<string>, Error<string>>> StopLive(UserCredential userCredential)
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
				var userData = await BilibiliApiTool.GetUserInfo(userCredential);
				var roomID = userData.Data.LiveRoom.RoomId;

				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStopLiveApi}?room_id={roomID}&csrf={userCredential.Bili_Jct}&platform={session.Platform}");
				httpRequestMessage.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
				var response = await HttpClient.SendAsync(httpRequestMessage);

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


		IBApiClient bApiClient = new BApiClient();
		WebSocketBLiveClient m_WebSocketBLiveClient;
		InteractivePlayHeartBeat m_PlayHeartBeat;
		string appId = "";
		string gameId = "";
		/// <summary>
		/// 开启监控直播间事件
		/// </summary>
		/// <returns></returns>
		async Task<OneOf<None, Error<string>>> StartLiveEvent()
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
					await StopLiveEvent();
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
						var logList = _dataStorage.Load<LiveEventLog>($"BilibiliLive_{streamOpenTime}", "data");
						_logger.LogInformation(content);
						logList.logs.Add(new($"[{DateTime.Now.ToString("O")}]", $"{content}"));
						_dataStorage.Save($"BilibiliLive_{streamOpenTime}", logList, "data");
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
																																								//m_WebSocketBLiveClient.Connect();//正常连接
					m_WebSocketBLiveClient.Connect();
					return new None();
				}
				else
				{
					_logger.LogError("开启玩法错误: {@info}", startInfo);
					await StopLiveEvent();
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
		/// 关闭监控直播间事件
		/// </summary>
		/// <returns></returns>
		async Task<bool> StopLiveEvent()
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
