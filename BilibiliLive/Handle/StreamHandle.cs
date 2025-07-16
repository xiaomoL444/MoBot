using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using OpenBLive.Client;
using OpenBLive.Client.Data;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using System.Diagnostics;
using System.Globalization;
using static QRCoder.PayloadGenerator;
using HttpClient = BilibiliLive.Tool.HttpClient;
using Timer = System.Timers.Timer;

namespace BilibiliLive.Handle
{
	/// <summary>
	/// 控制串流的小家伙
	/// </summary>
	public class StreamHandle : IMessageHandle<Group>
	{
		private readonly long _opGroupID = Constant.Constants.OPGroupID;
		private readonly long _opAdmin = Constant.Constants.OPAdmin;

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;

		private Process? _mainProcess;//主推流程序
		private Process? _childProcess;//子推流程序1

		private List<string> _streamVideoPaths = new();

		private bool isStreaming = false;//是否正在直播

		//private string rtmp_url = "";
		private List<string> rtmp_urls = new();
#if DEBUG
		private bool isDebug = true; //主程序的推流-f flv 要记得改 和 mpegts
#endif
#if !DEBUG
		private bool isDebug = false; //主程序的推流-f flv 要记得改 和 mpegts
#endif

		private string streamOpenTime = "";

		public StreamHandle(
			ILogger<StreamHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task Initial()
		{
			return Task.CompletedTask;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && (message.IsMsg("/开始推流") || message.IsMsg("/关闭推流"))) return Task.FromResult(true);

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
			}
			return Task.CompletedTask;
		}

		async void StartStream(Group group)
		{
			if (isStreaming)
			{
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("已经在推流中哦，不能再推啦~").Build());
				_logger.LogWarning("已经在推流中，无法重复推流");
				return;
			}

			//在开播前先把可能存在的进程关闭
			if (_mainProcess != null && !_mainProcess.HasExited)
			{
				_logger.LogWarning("主ffmpeg未关闭!");
				_mainProcess.StandardInput.WriteLine("q");
				_mainProcess.WaitForExit();
			}
			if (_childProcess != null && !_childProcess.HasExited)
			{
				_logger.LogWarning("子ffmpeg未关闭!");
				_childProcess.StandardInput.WriteLine("q");
				_childProcess.WaitForExit();
			}

			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");

			//校验参数

			var failAction = async () => { await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("推流的配置好像不完整...߹ - ߹，勾修金sama请检查一下吧").Build()); };

			if (!Directory.Exists(streamConfig.StreamVideoDirectory))
			{
				_logger.LogError("视频目录->{path}不存在，请检查配置文件", streamConfig.StreamVideoDirectory);
				await failAction();
				return;
			}

			//读取视频文件
			_streamVideoPaths = Directory.GetFiles(streamConfig.StreamVideoDirectory).Where(q => q.EndsWith(".mp4")).OrderBy(q => Path.GetFileName(q)).ToList();
			_logger.LogDebug("找到的所有视频文件{paths}", _streamVideoPaths);

			if (_streamVideoPaths.Count <= 0)
			{
				_logger.LogError("视频目录->{path}下不存在视频源", streamConfig.StreamVideoDirectory);
				await failAction();
				return;
			}

			//开启直播
			isStreaming = true;
			if (!isDebug)
			{
				if (!await StartLiveEvent() || !await StartLive())
				{
					_logger.LogError("开启直播间失败");
					await StopLive();
					await StopLiveEvent();
					isStreaming = false;
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("打开直播间失败了>﹏<，请检查一下控制台输出吧").Build());
					return;
				}
			}

			string flags = "[f=flv]";
			string teeOutput = string.Join(" | ", rtmp_urls.Select(url => $"{flags}{url}"));

			//主ffmpeg程序
			var args = $"-fflags +genpts -err_detect ignore_err -ignore_unknown -flags low_delay -i udp://127.0.0.1:11111 -map 0 -vtag 7 -atag 10 -c copy -f {(isDebug ? "mpegts" : "tee")} \"{teeOutput}\"";
			_logger.LogDebug("ffmpeg主程序参数{args}", args);
			_mainProcess = new Process
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
			_mainProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_main] {info}", e.Data); };
			_mainProcess.ErrorDataReceived += async (s, e) =>
			{
				if (string.IsNullOrEmpty(e.Data)) return;

				_logger.LogDebug("[ffmpeg_main] {info}", e.Data);
				if (e.Data.Contains("av_interleaved_write_frame") ||
				e.Data.Contains("Connection refused") ||
				e.Data.Contains("Broken pipe") ||
				e.Data.Contains("error") ||
				e.Data.Contains("No such file or directory") ||
				e.Data.Contains("Connection reset"))
				{
					// 🔔 一路推流出错，发出警告或处理
					_logger.LogError("其中一条远程推流出现错误");
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("其中一条推流出现错误，请查看控制台").Build());
					// 你可以选择重启 FFmpeg 或做其他处理
				}
			};
			_mainProcess.Exited += async (s, e) =>
			{
				if (_mainProcess.ExitCode == 0)
				{
					_logger.LogInformation("主ffmpeg强制退出");
					return;
				}
				_logger.LogWarning("主程序错误码{exit_code}", _mainProcess.ExitCode);
				isStreaming = false;
				_logger.LogError("主ffmpeg异常退出");
				if (_childProcess != null && _childProcess.HasExited == false)
				{
					_childProcess.Kill();
					_logger.LogWarning("子ffmpeg一起退出");
				}
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("主程序退出出现了一些异常...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
			};
			_mainProcess.Start();
			_mainProcess.BeginOutputReadLine();
			_mainProcess.BeginErrorReadLine();



			//子ffmpeg程序

			_ = Task.Run(async () =>
			{
				int num = streamConfig.Index;
				bool isStream = true;
				while (isStream)
				{
					try
					{
						CancellationTokenSource cts = new CancellationTokenSource();//要是中途出错了直接打断这个等待的Task并退出循环

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
							continue;
						}
						var child_args = $"-re -fflags +genpts+igndts+discardcorrupt -i \"{_streamVideoPaths[num]}\" -t {duration.TotalSeconds}  -c copy -mpegts_flags +initial_discontinuity -muxpreload 0 -muxdelay 0  -f mpegts udp://127.0.0.1:11111";
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
								cts.Cancel();
								return;
							}
							//windows是-1，linux是137
							if (_childProcess.ExitCode == -1 || _childProcess.ExitCode == 137)
							{
								_logger.LogInformation("子ffmpeg强制退出");
								cts.Cancel();
								isStream = false;
								return;
							}
							cts.Cancel();
							_logger.LogWarning("子程序错误码{exit_code}", _childProcess.ExitCode);
							isStreaming = false;
							_logger.LogWarning("子ffmpeg异常退出");
							if (_mainProcess != null && _mainProcess.HasExited == false)
							{
								_mainProcess!.StandardInput.WriteLine("q");
								_mainProcess.StandardInput.Flush();
								_logger.LogError("主ffmpeg一起退出");
							}
							await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序退出出现了异常1...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
							isStream = false;
						};
						_childProcess.Start();
						_childProcess.BeginOutputReadLine();
						_childProcess.BeginErrorReadLine();

						//等待视频的时长
						try
						{
							await Task.Delay((int)duration.TotalMilliseconds, cts.Token);
						}
						catch (Exception)
						{
							_logger.LogDebug("子程序经过{time}秒退出", duration.TotalSeconds);
						}
						if (!_childProcess.HasExited)
						{
							//如果没有退出，等待1s（因为可能ffmpeg自己有点慢之类的，容忍等待）
							try
							{
								await Task.Delay(1000, cts.Token);
								//要是还没退出发送关闭指令
								_logger.LogWarning("播放第{i}的视频{path}的时候退出慢了", num, _streamVideoPaths[num]);
								_childProcess.StandardInput.WriteLine("q");
								_childProcess.StandardInput.Flush();
							}
							catch (Exception)
							{
								_logger.LogDebug("子程序经过{time} +1秒发送退出请求后退出", duration.TotalSeconds);
							}
						}
						if (!_childProcess.HasExited)
						{
							try
							{
								await Task.Delay(1000, cts.Token);
								//要是还没退出强制关闭
								_logger.LogWarning("播放第{i}的视频{path}的时候需要强制关闭！", num, _streamVideoPaths[num]);
								_childProcess.Kill();
								_childProcess.WaitForExit();
							}
							catch (Exception)
							{
								_logger.LogDebug("子程序经过{time} +2秒发送退出请求后退出", duration.TotalSeconds);
							}
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "子程序出现错误");
						await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序退出出现了异常2...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
						break;
					}
				}
			});

			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("直播已开始啦(＾ω＾)").Build());
		}
		async void StopStream(Group group)
		{
			if (!isStreaming)
			{
				_logger.LogWarning("没有正在进行的推流，进行强制关播");
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("现在末酱不在在推流哦~(进行强制关播)").Build());
			}
			isStreaming = false;
			_logger.LogInformation("关闭推流");
			if (_mainProcess != null && !_mainProcess.HasExited)
			{
				_mainProcess!.StandardInput.WriteLine("q");
				_mainProcess.WaitForExit();
			}
			if (_childProcess != null && !_childProcess.HasExited)
			{
				_childProcess!.Kill();
				_childProcess.WaitForExit();
			}
			if (!isDebug)
			{
				if (!await StopLive() || !await StopLiveEvent())
				{
					_logger.LogError("关闭直播失败");
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("哇...!关闭...失败了......可能要请勾修金sama邦邦末酱了(｡•́︿•̀｡) ").Build());
					return;
				}
			}
			var liveLogs = _dataStorage.Load<LiveEventLog>($"BilibiliLive_{streamOpenTime}", "data").logs;
			var showLogNum = 4;
			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text(@$"这次的推流很成功哦~
(●• ̀ω•́ )✧末酱在直播间看到了一些有趣的消息
-------------
{String.Join("\n", liveLogs.Take(showLogNum).ToDictionary().Values)}{(liveLogs.Count > showLogNum ? $"\n以及其他{liveLogs.Count - showLogNum}条信息......" : "")}
-------------
可以来这里查看哦*⸜( •ᴗ• )⸝*:BilibiliLive_{streamOpenTime}").Build());
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
		/// </summary>
		/// <returns>是否开启成功（也就是有没有报错）</returns>
		async Task<bool> StartLive()
		{
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");
			var accountConfig = _dataStorage.Load<AccountConfig>("account");

			try
			{
				//挨个获取roomID和rtmp_url
				for (int i = 0; i < accountConfig.Accounts.Count; i++)
				{
					var account = accountConfig.Accounts[i];
					var roomIDReqMsg = new HttpRequestMessage(HttpMethod.Get, $"{Constants.BilibiliGetRoomInfoOld}?mid={account.DedeUserID}");
					var roomIDReqResponse = await HttpClient.SendAsync(roomIDReqMsg);
					_logger.LogDebug("获取{user}直播信息的回复{@response}", account.DedeUserID, (await roomIDReqResponse.Content.ReadAsStringAsync()));

					var roomID = JsonConvert.DeserializeObject<LiveRoomInfoRsp>((await roomIDReqResponse.Content.ReadAsStringAsync())).Data.RoomId;


					var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStartLiveAPI}?room_id={roomID}&area_v2={streamConfig.AreaV2}&platform={streamConfig.Platform}&csrf={account.Bili_Jct}");
					httpRequestMessage.Headers.Add("cookie", $"SESSDATA={account.Sessdata};bili_jct={account.Bili_Jct}");
					var response = await HttpClient.SendAsync(httpRequestMessage);
					_logger.LogDebug("开启{uid}直播间的回复{@response}", account.DedeUserID, (await response.Content.ReadAsStringAsync()));

					var responseJson = JsonConvert.DeserializeObject<StartLiveRsp>(await response.Content.ReadAsStringAsync());
					rtmp_urls.Add(responseJson?.Data.Rtmp.Addr + JsonConvert.DeserializeObject<string>($"\"{responseJson?.Data.Rtmp.Code}\""));
					_logger.LogInformation("直播间{uid}开启成功", account.DedeUserID);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "开启直播间失败");
				return false;
			}
			return true;
		}

		/// <summary>
		/// 关闭直播间
		/// </summary>
		/// <returns></returns>
		async Task<bool> StopLive()
		{
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");
			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			try
			{
				//挨个关闭直播间
				for (int i = 0; i < accountConfig.Accounts.Count; i++)
				{
					var account = accountConfig.Accounts[i];
					var roomIDReqMsg = new HttpRequestMessage(HttpMethod.Get, $"{Constants.BilibiliGetRoomInfoOld}?mid={account.DedeUserID}");
					var roomIDReqResponse = await HttpClient.SendAsync(roomIDReqMsg);
					_logger.LogDebug("获取{user}直播信息的回复{@response}", account.DedeUserID, (await roomIDReqResponse.Content.ReadAsStringAsync()));

					var roomID = JsonConvert.DeserializeObject<LiveRoomInfoRsp>((await roomIDReqResponse.Content.ReadAsStringAsync())).Data.RoomId;


					var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStopLiveApi}?room_id={roomID}&csrf={account.Bili_Jct}&platform={streamConfig.Platform}");
					httpRequestMessage.Headers.Add("cookie", $"SESSDATA={account.Sessdata};bili_jct={account.Bili_Jct}");
					var response = await HttpClient.SendAsync(httpRequestMessage);
					_logger.LogDebug("关闭{user}直播间的回复{@response}", account.DedeUserID, (await response.Content.ReadAsStringAsync()));

					var responseJson = JsonConvert.DeserializeObject<StopLiveRsp>(await response.Content.ReadAsStringAsync());

					if (responseJson?.Data.Status == "PREPARING")
					{
						continue;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "关闭直播间失败");
				return false;
			}
			return true;
		}


		IBApiClient bApiClient = new BApiClient();
		WebSocketBLiveClient m_WebSocketBLiveClient;
		string appId = "";
		string gameId = "";
		/// <summary>
		/// 开启监控直播间事件
		/// </summary>
		/// <returns></returns>
		async Task<bool> StartLiveEvent()
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
					return false;
				}

				gameId = startInfo?.Data?.GameInfo?.GameId;
				if (gameId != null)
				{
					game_id = gameId;
					_logger.LogInformation("B站直播开放平台成功开启，开始心跳，场次ID: ", gameId);

					//心跳API（用于保持在线）
					InteractivePlayHeartBeat m_PlayHeartBeat = new InteractivePlayHeartBeat(gameId);
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
					return true;
				}
				else
				{
					_logger.LogError("开启玩法错误: {@info}", startInfo);
				}
			}
			else
			{
				_logger.LogError("AppID不存在: {appId}", appId);
			}
			return false;
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
				_logger.LogInformation("关闭玩法:{@ret} ", ret);
				_logger.LogInformation("断开心跳");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "关闭玩法失败");
				return false;
			}

			return true;
		}
	}
}
