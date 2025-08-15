using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using MoBot.Core.Interfaces;
using MoBot.Infra.PlayWright.Constant;
using MoBot.Infra.PlayWright.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PlayWright.Webshot
{
	public class Webshot : IWebshot
	{
		private readonly IDataStorage _dataStorage;
		private readonly ILogger _logger;
		private IBrowser _browser = null;

		public Webshot(
			IDataStorage dataStorage,
			ILogger<Webshot> logger)
		{
			_dataStorage = dataStorage;
			_logger = logger;

			StartNewChrome();
		}

		public string GetIPAddress()
		{
			return "http://webshot.lan:8080";
		}
		async Task StartNewChrome()
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
			try
			{
				var playwright = await Playwright.CreateAsync();
				//首次调用实例化浏览器
				var browser = await playwright.Chromium.ConnectOverCDPAsync(address);
				_browser = browser;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "连接失败");
				return;
			}
			_logger.LogInformation("连接成功");

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
		public async Task<string> ScreenShot(string url, BrowserNewContextOptions browserNewContextOptions = null, PageScreenshotOptions pageScreenshotOptions = null, List<Microsoft.Playwright.Cookie> cookieParam = null)
		{
			if (_browser is not { IsConnected: true })
			{
				_logger.LogWarning("Chrome被意外关闭，重新启动");
				await StartNewChrome();
			}
			var context = await _browser.NewContextAsync(browserNewContextOptions ?? new BrowserNewContextOptions
			{
				ViewportSize = new ViewportSize
				{
					Width = 2560,
					Height = 1440
				}
			});
			await context.AddCookiesAsync(cookieParam ?? new());

			var page = await context.NewPageAsync();

			try
			{
				await page.GotoAsync(url);

				_logger.LogDebug("正在截图网页：{url}", url);
				//await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
				await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

				var pagescOption = pageScreenshotOptions ?? new PageScreenshotOptions()
				{
					FullPage = true,
					Type = ScreenshotType.Png,
				};
#if DEBUG
				var path = _dataStorage.GetDirectory(Core.Models.DirectoryType.Cache) + "/" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".png";
				pagescOption.Path = path;//Debug下保存为路劲
#endif
				var bytes = await page.ScreenshotAsync(pagescOption);
				_logger.LogDebug("截图完成");
				await page.CloseAsync();
				return Convert.ToBase64String(bytes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "截图失败");
				return Constants.WhiteTransParentBase64;//返回空图片
			}

		}
	}
}
