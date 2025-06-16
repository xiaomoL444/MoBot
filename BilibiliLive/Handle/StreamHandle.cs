using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle;
using MoBot.Handle.Extensions;
using System.Diagnostics;

namespace BilibiliLive.Handle
{
	/// <summary>
	/// 控制串流的小家伙
	/// </summary>
	public class StreamHandle : IMessageHandle<Group>
	{
		private readonly long _opGroupID = Constant.Constant.OPGroupID;
		private readonly long _opAdmin = Constant.Constant.OPAdmin;

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;

		private Process? _mainProcess;//主推流程序
		private Process? _childProcess;//子推流程序1

		private List<string> _streamVideoPaths = new();

		private bool isStreaming = false;//是否正在直播


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
			if (!File.Exists(streamConfig.OverlayStreamVideo))
			{
				_logger.LogWarning("大伟视频->{path}不存在，请检查配置文件", streamConfig.OverlayStreamVideo);
				await failAction();
				return;
			}

			_streamVideoPaths = Directory.GetFiles(streamConfig.StreamVideoDirectory).Where(q => q.EndsWith(".mp4")).ToList();

			if (_streamVideoPaths.Count <= 0)
			{
				_logger.LogWarning("视频目录->{path}下不存在视频源", streamConfig.StreamVideoDirectory);
				await failAction();
				return;
			}

			isStreaming = true;

			#region MainProcess
			var args = $"-fflags +genpts+igndts+discardcorrupt -f lavfi -i color=c=black:s=1280x720 -i udp://127.0.0.1:11111 -filter_complex \"[1:v]scale=1280:720:force_original_aspect_ratio=decrease[vid1];[0:v][vid1]overlay=(W-w)/2:(H-h)/2\" -c:v libx264 -preset veryfast -tune zerolatency -c:a aac -ar 44100 -b:a 128k -f mpegts \"{accountConfig.RtmpUrl}\"";
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

			#endregion

			#region ChildProcess
			Action<int> action = (int num) => { };
			action = (int num) =>
			{
				if (num >= _streamVideoPaths.Count) num = 0;
				_logger.LogInformation("播放第{i}个视频，路径为：{path}", num, _streamVideoPaths[num]);

				streamConfig.Index = num;
				_dataStorage.Save("stream", streamConfig);

				_childProcess = new Process
				{
					StartInfo = new ProcessStartInfo()
					{
						FileName = "ffmpeg",
						Arguments = $"-re -fflags +genpts+igndts+discardcorrupt -max_interleave_delta 0 -avoid_negative_ts 1 -i \"{_streamVideoPaths[num]}\" -stream_loop -1 -i \"{streamConfig.OverlayStreamVideo}\" -filter_complex \"[1:v]scale=570:405[little];[0:v][little]overlay=W-w:H-h\" -shortest -f mpegts udp://127.0.0.1:11111",
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
						action(num + 1);
						return;
					}
					if (_childProcess.ExitCode == -1)
					{
						_logger.LogInformation("子ffmpeg强制退出");
						return;
					}
					isStreaming = false;
					_logger.LogWarning("子ffmpeg异常退出");
					if (_mainProcess != null && _mainProcess.HasExited == false)
					{
						_mainProcess!.StandardInput.WriteLine("q");
						_mainProcess.StandardInput.Flush();
						_logger.LogError("主ffmpeg一起退出");
					}
					await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("主程序异常退出，请检查控制台输出").Build());
				};
				_childProcess.Start();
				_childProcess.BeginOutputReadLine();
				_childProcess.BeginErrorReadLine();
			};
			action(streamConfig.Index);

			#endregion
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
			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("推流已关闭").Build());
		}
	}
}
