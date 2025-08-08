using BilibiliLive.Models.Config;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Message;
using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Session
{
	internal class SourceStreamSession
	{
		private readonly ILogger _logger = GlobalSetting.CreateLogger<SourceStreamSession>();
		private readonly IDataStorage _dataStorage = GlobalSetting.DataStorage;

		private List<string> _streamVideoPaths = new();
		private Process? _process;//子推流程序1
		public Action NextRouteStreamAction = () => { };
		public bool IsStream = false;

		public async Task<OneOf<Success<string>, Error<string>>> Initialize()
		{
			var streamConfig = _dataStorage.Load<StreamConfig>("stream");
			//校验参数
			if (!Directory.Exists(streamConfig.StreamVideoDirectory))
			{
				_logger.LogError("视频目录->{path}不存在，请检查配置文件", streamConfig.StreamVideoDirectory);
				return new Error<string>("视频目录不存在...߹ - ߹，勾修金sama请检查一下吧");
			}

			//读取视频文件
			_streamVideoPaths = Directory.GetFiles(streamConfig.StreamVideoDirectory).Where(q => q.EndsWith(".mp4")).OrderBy(q => Path.GetFileName(q)).ToList();
			_logger.LogDebug("找到的所有视频文件{paths}", _streamVideoPaths);

			if (_streamVideoPaths.Count <= 0)
			{
				_logger.LogError("视频目录->{path}下不存在视频源", streamConfig.StreamVideoDirectory);
				return new Error<string>("不存在视频源...߹ - ߹，勾修金sama请检查一下吧");
			}
			return new Success<string>("源推流开启成功");
		}
		public async Task<string> Open()
		{
			//开启子ffmpeg程序
			if (_process is { HasExited: false })
			{
				_logger.LogDebug("源推流程序存在，无需重复开启");
				return "源推流程序存在，无需重复开启";
			}

			_logger.LogInformation("尝试开启源推流程序");

			var streamConfig = _dataStorage.Load<StreamConfig>("stream");

			//开启推流
			_ = Task.Run(async () =>
			{
				int num = streamConfig.Index;
				IsStream = true;
				while (IsStream)
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
						_process = new Process
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
						_process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_child] {info}", e.Data); };
						_process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[ffmpeg_child] {info}", e.Data); };

						_process.Exited += async (s, e) =>
						{
							//0即是正常退出（播放完或者是输入q，这里只有播放完，让他强制退出就使用-1）
							if (_process.ExitCode == 0)
							{
								num++;
								videoCts.Cancel();
								return;
							}
							//windows是-1，linux是137
							if (_process.ExitCode == -1 || _process.ExitCode == 137)
							{
								_logger.LogInformation("子ffmpeg强制退出，结束直播");
								IsStream = false;
								videoCts.Cancel();
								return;
							}
							_logger.LogWarning("子程序错误码{exit_code}", _process.ExitCode);
							_logger.LogWarning("子ffmpeg异常退出");

							//sendMessage(MessageChainBuilder.Create().Text("子程序退出出现了异常...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
							IsStream = false;
							videoCts.Cancel();
						};
						_process.Start();
						_process.BeginOutputReadLine();
						_process.BeginErrorReadLine();

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
							await _process.WaitForExitAsync(closeCts.Token);
							_logger.LogInformation("直播子程序已关闭");
							continue;
						}
						catch (OperationCanceledException)
						{
							_logger.LogWarning("直播子程序超时未退出");
							//sendMessage(MessageChainBuilder.Create().Text("子程序异常超时退出...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
							break;
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "子程序出现错误");
						//sendMessage(MessageChainBuilder.Create().Text("子程序出现了错误...(˃ ⌑ ˂ഃ )，下次末酱会处理好的").Build());
						break;
					}
				}
			});

			return "源推流程序开启成功";

		}
		public async Task<OneOf<Success<string>, Error<string>>> Close()
		{
			if (_process is { HasExited: false })
			{

				using var cts = new CancellationTokenSource(5000);
				_process!.Kill();
				try
				{
					await _process.WaitForExitAsync(cts.Token);
					_logger.LogInformation("串流程序关闭成功");
					return new Success<string>("串流程序关闭成功");
				}
				catch (OperationCanceledException)
				{
					_logger.LogWarning("串流程序关闭失败");
					return new Error<string>("串流程序关闭失败！");
				}
			}
			return new Success<string>("串流程序不存在");
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
	}
}
