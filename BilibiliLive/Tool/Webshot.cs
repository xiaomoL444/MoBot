using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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
			return "http://localhost:8080";
		}

		static Webshot()
		{
			_logger.LogDebug("实例化一次");
			var chromePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ChromePathWindow : ChromePathLinux;
			if (!File.Exists(chromePath))
			{
				_logger.LogError("不存在Chrome地址！");
				throw new("不存在Chrome地址！");
			}
			//首次调用实例化浏览器
			_browser = Puppeteer.LaunchAsync(new LaunchOptions
			{
				ExecutablePath = chromePath,
				Headless = true,
				Args = [ "--no-sandbox",
	"--disable-gpu",
	"--disable-dev-shm-usage",
	"--disable-software-rasterizer",
	"--disable-extensions",
	"--disable-background-networking",
	"--disable-sync",
	"--disable-default-apps",
	"--mute-audio",
	"--disable-setuid-sandbox",
	"--disable-features=TranslateUI",
	"--no-xshm",
	"--disable-dbus"],
				DumpIO = true// 打开调试输出
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
		/// <returns>图片地址</returns>
		public static async Task<string> ScreenShot(string url, ViewPortOptions viewPortOptions = null, ScreenshotOptions screenshotOptions = null, string waitForFunc = "() => window.appLoaded === true", List<CookieParam> cookieParam = null)
		{
			await using var page = await _browser.NewPageAsync();
			await page.GoToAsync(url);
			string uuid = Guid.NewGuid().ToString();
			var path = $"{_dataStorage.GetPath(MoBot.Core.Models.DirectoryType.Cache)}/{uuid}.png";
			await page.SetViewportAsync(viewPortOptions ?? new ViewPortOptions
			{
				Width = 2560,
				Height = 1440
			});
			_logger.LogDebug("正在截图网页：{url}，保存地址：{path}", url, path);
			await page.WaitForFunctionAsync(waitForFunc);
			await page.ScreenshotAsync(path, screenshotOptions ?? new());
			return path;
		}
	}
}
