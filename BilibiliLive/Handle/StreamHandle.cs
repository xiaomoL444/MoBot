using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle;
using MoBot.Handle.Extensions;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
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

		private string rtmp_url = "";
#if DEBUG
		private bool isDebug = true; //主程序的推流-f flv 要记得改 和 mpegts
#endif
#if !DEBUG
		private bool isDebug = false; //主程序的推流-f flv 要记得改 和 mpegts
#endif

		public StreamHandle(
			ILogger<StreamHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
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
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("已经在推流中，无法重复推流").Build());
				_logger.LogWarning("已经在推流中，无法重复推流");
				return;
			}

			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");

			//校验参数

			var failAction = async () => { await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("推流失败，请检查控制台输出").Build()); };
			if (String.IsNullOrEmpty(accountConfig.RtmpUrl))
			{
				_logger.LogWarning("远程推流链接不存在，请重新检查配置文件");
				await failAction();
				return;
			}
			if (!Directory.Exists(streamConfig.StreamVideoDirectory))
			{
				_logger.LogWarning("视频目录->{path}不存在，请检查配置文件", streamConfig.StreamVideoDirectory);
				await failAction();
				return;
			}

			//读取视频文件
			_streamVideoPaths = Directory.GetFiles(streamConfig.StreamVideoDirectory).Where(q => q.EndsWith(".mp4")).OrderBy(q => Path.GetFileName(q)).ToList();
			_logger.LogDebug("找到的所有视频文件{paths}", _streamVideoPaths);

			if (_streamVideoPaths.Count <= 0)
			{
				_logger.LogWarning("视频目录->{path}下不存在视频源", streamConfig.StreamVideoDirectory);
				await failAction();
				return;
			}

			//开启直播
			isStreaming = true;
			rtmp_url = accountConfig.RtmpUrl;
			if (!isDebug)
			{
				if (!await StartLive())
				{
					_logger.LogError("开启直播间失败");
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("打开直播间失败，请检查控制台输出").Build());
					return;
				}
			}

			//主ffmpeg程序
			var args = $"-fflags +genpts -err_detect ignore_err -ignore_unknown -flags low_delay -i udp://127.0.0.1:11111 -c copy -f {(isDebug ? "mpegts" : "flv")} \"{rtmp_url}\"";
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
			_mainProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_main] {info}", e.Data); };
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
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("主程序异常退出，请检查控制台输出").Build());
			};
			_mainProcess.Start();
			_mainProcess.BeginOutputReadLine();
			_mainProcess.BeginErrorReadLine();



			//子ffmpeg程序

			_ = Task.Run(async () =>
			{
				int num = streamConfig.Index;
				while (true)
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
						TimeSpan duration = GetVideoDuration(_streamVideoPaths[num]);
						_childProcess = new Process
						{
							StartInfo = new ProcessStartInfo()
							{
								FileName = "ffmpeg",
								Arguments = $" -fflags +genpts+igndts+discardcorrupt -i \"{_streamVideoPaths[num]}\" -t {duration.TotalSeconds}  -c copy -mpegts_flags +initial_discontinuity -muxpreload 0 -muxdelay 0  -f mpegts udp://127.0.0.1:11111",
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
								return;
							}
							_logger.LogWarning("子程序错误码{exit_code}", _childProcess.ExitCode);
							isStreaming = false;
							_logger.LogWarning("子ffmpeg异常退出");
							if (_mainProcess != null && _mainProcess.HasExited == false)
							{
								_mainProcess!.StandardInput.WriteLine("q");
								_mainProcess.StandardInput.Flush();
								_logger.LogError("主ffmpeg一起退出");
							}
							await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序异常退出，请检查控制台输出").Build());
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
						await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("子程序异常退出，请检查控制台输出").Build());
						break;
					}
				}
			});

			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("直播已开始").Build());
		}
		async void StopStream(Group group)
		{
			if (!isStreaming)
			{
				_logger.LogWarning("没有正在进行的推流");
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("没有正在进行的推流").Build());
				return;
			}
			isStreaming = false;
			_logger.LogInformation("关闭推流");
			_mainProcess!.StandardInput.WriteLine("q");
			_mainProcess.WaitForExit();
			_childProcess!.Kill();
			_childProcess.WaitForExit();
			if (!isDebug)
			{
				if (!await StopLive())
				{
					_logger.LogError("关闭直播失败");
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("关闭直播间失败，请检查控制台输出").Build());
					return;
				}
			}
			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("推流已关闭").Build());
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
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStartLiveAPI}?room_id={streamConfig.RoomID}&area_v2={streamConfig.AreaV2}&platform={streamConfig.Platform}&csrf={accountConfig.Bili_Jct}");
			httpRequestMessage.Headers.Add("cookie", $"SESSDATA={accountConfig.Sessdata};bili_jct={accountConfig.Bili_Jct}");
			var response = await HttpClient.SendAsync(httpRequestMessage);
			_logger.LogDebug("开启直播的回复{@response}", (await response.Content.ReadAsStringAsync()));
			try
			{
				var responseJson = JsonConvert.DeserializeObject<StartLiveRsp>(await response.Content.ReadAsStringAsync());
				rtmp_url = responseJson?.Data.Rtmp.Addr + JsonConvert.DeserializeObject<string>($"\"{responseJson?.Data.Rtmp.Code}\"");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "开启直播失败");
				return false;
			}
			return true;
		}

		async Task<bool> StopLive()
		{
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");
			var accountConfig = _dataStorage.Load<AccountConfig>("account");
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.BilibiliStopLiveApi}?room_id={streamConfig.RoomID}&csrf={accountConfig.Bili_Jct}");
			httpRequestMessage.Headers.Add("cookie", $"SESSDATA={accountConfig.Sessdata};bili_jct={accountConfig.Bili_Jct}");
			var response = await HttpClient.SendAsync(httpRequestMessage);
			_logger.LogDebug("关闭直播的回复{@response}", (await response.Content.ReadAsStringAsync()));
			try
			{
				var responseJson = JsonConvert.DeserializeObject<StopLiveRsp>(await response.Content.ReadAsStringAsync());
				if (responseJson?.Data.Status == "PREPARING")
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "关闭直播失败");
				return false;
			}
			return true;
		}
	}
}
