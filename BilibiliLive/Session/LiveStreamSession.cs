using BilibiliLive.Models;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Session
{
	/// <summary>
	/// 直播会话
	/// </summary>
	sealed class LiveStreamSession
	{
		private readonly ILogger _logger = GlobalLogger.CreateLogger(typeof(LiveStreamSession));

		public UserCredential UserCredential { get; } = new();//这位B站用户的个人信息
		public string Platform { get; } = string.Empty;
		public bool IsLive = false;//是否还在直播
		public Action FailCallback = () => { };//异常退出时触发错误回调（通常是关闭直播链接）
		public (string code, string msg) ExitState = new();

		private Process _process = new();//直播的ffmpeg程序

		private string _rtmp = string.Empty;//直播的远程推流连接
		public LiveStreamSession(UserCredential userCredential, string platform, string rtmp)
		{
			Platform = platform;
			UserCredential = userCredential;
			_rtmp = rtmp;
			Start();
		}
		void Start()
		{
			IsLive = true;
			//主ffmpeg程序
			var args = $"-loglevel warning -fflags +genpts -err_detect ignore_err -ignore_unknown -flags low_delay -i udp://239.0.0.1:11111 -c copy -f flv \"{_rtmp}\"";
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
			_process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogDebug("[{uid}直播会话] {info}", UserCredential.DedeUserID, e.Data); };
			_process.ErrorDataReceived += (s, e) => { if (string.IsNullOrEmpty(e.Data)) return; _logger.LogDebug("[{uid}直播会话] {info}", UserCredential.DedeUserID, e.Data); };
			_process.Exited += (s, e) =>
			{
				IsLive = false;
				ExitState = new($"{_process.ExitCode}", $"退出码：{_process.ExitCode}");
				if (_process.ExitCode == 0)
				{
					_logger.LogInformation("[{uid}]直播会话手动退出", UserCredential.DedeUserID);
					return;
				}
				_logger.LogError("[{uid}]直播会话异常退出，CODE:{exit_code}", UserCredential.DedeUserID, _process.ExitCode);
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
}
