using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Infra.PuppeteerSharp.Interface;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PuppeteerSharp.Webshot
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
		void StartNewChrome()
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
				//首次调用实例化浏览器
				_browser = Puppeteer.ConnectAsync(new ConnectOptions
				{
					BrowserURL = address,
				}).Result;
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
		public async Task<string> ScreenShot(string url, ViewPortOptions viewPortOptions = null, ScreenshotOptions screenshotOptions = null, string waitForFunc = @"() => {
  return new Promise(async resolve => {
    // ✅ 先等待 Vue / DOM 渲染完成（等价于 Vue.nextTick）
    await new Promise(requestAnimationFrame);

    // ✅ 再等待所有图片加载完成
    const imagePromises = Array.from(document.images).map(img => {
      if (img.complete) return Promise.resolve();
      return new Promise(res => {
        img.onload = img.onerror = res;
      });
    });

    // ✅ 等待所有字体加载完成（document.fonts.ready）
    const fontPromise = document.fonts ? document.fonts.ready : Promise.resolve();

    // ✅ 等全部加载完成
    await Promise.all([...imagePromises, fontPromise]);

    resolve();
  });
}", List<CookieParam> cookieParam = null)
		{
			if (_browser is not { IsConnected: true })
			{
				_logger.LogWarning("Chrome被意外关闭，重新启动");
				StartNewChrome();
			}
			await using var page = await _browser.NewPageAsync();
			try
			{
				await page.GoToAsync(url);
				_logger.LogDebug("正在截图网页：{url}", url);
				await page.WaitForFunctionAsync(waitForFunc);
				await page.SetViewportAsync(viewPortOptions ?? new ViewPortOptions
				{
					Width = 1920,
					Height = 1080
				});
				var base64 = await page.ScreenshotBase64Async(screenshotOptions ?? new());
#if DEBUG
				var path = _dataStorage.GetDirectory(MoBot.Core.Models.DirectoryType.Cache) + "/" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".png";
				await page.ScreenshotAsync(path, screenshotOptions ?? new());
				//var base64 = Convert.ToBase64String(File.ReadAllBytes(path));
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
