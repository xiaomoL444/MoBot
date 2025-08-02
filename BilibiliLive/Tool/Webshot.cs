using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	public static class Webshot
	{
		private static IDataStorage _dataStorage = GlobalDataStorage.DataStorage;
		private static ILogger _logger = GlobalLogger.CreateLogger(typeof(Webshot));

		private const string ChromePathWindow = "C:\\Code\\Chrome\\chrome.exe";//window的Chrome地址
		private const string ChromePathLinux = "./Chrome/chrome";//linux的Chrome地址

		private static IBrowser _browser = null;
		public static string GetIPAddress()
		{
			return "http://webshot.lan:8080";
		}

		static Webshot()
		{
			StartNewChrome();
		}

		static void StartNewChrome()
		{
			_logger.LogDebug("创建Chorme实例");
			string address = string.Empty;
			try
			{
				address = $"http://{Dns.GetHostEntry("chrome.lan").AddressList[0].ToString()}:9222";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取Chrome address错误");
				return;
			}
			_logger.LogDebug("尝试连接{address}", address);
			//首次调用实例化浏览器
			_browser = Puppeteer.ConnectAsync(new ConnectOptions
			{
				BrowserURL = address,
			}).Result;
		}

		/// <summary>
		/// 网页截图
		/// </summary>
		/// <param name="url">网页链接</param>
		/// <param name="viewPortOptions">网页缩放</param>
		/// <param name="screenshotOptions">截图的额外功能</param>
		/// <param name="waitForFunc">等待指令</param>
		/// <param name="cookieParam">cookie(我自己一般用不到（）)</param>
		/// <returns>图片base64</returns>
		public static async Task<string> ScreenShot(string url, ViewPortOptions viewPortOptions = null, ScreenshotOptions screenshotOptions = null, string waitForFunc = "() => window.appLoaded === true", List<CookieParam> cookieParam = null)
		{
			if (!_browser.IsConnected)
			{
				_logger.LogWarning("Chrome被意外关闭，重新启动");
				StartNewChrome();
			}
			await using var page = await _browser.NewPageAsync();
			try
			{
				await page.GoToAsync(url);
				await page.SetViewportAsync(viewPortOptions ?? new ViewPortOptions
				{
					Width = 1920,
					Height = 1080
				});
				_logger.LogDebug("正在截图网页：{url}", url);
				await page.WaitForFunctionAsync(waitForFunc);
				var base64 = await page.ScreenshotBase64Async(screenshotOptions ?? new());
#if DEBUG
				await page.ScreenshotAsync(_dataStorage.GetDirectory(MoBot.Core.Models.DirectoryType.Cache) + "/" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".png");
#endif
				_logger.LogDebug("截图完成");
				return base64;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "截图失败");
				return string.Empty;//其实可以返回一张空图片地址
			}

		}
	}
}
