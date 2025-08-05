using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PuppeteerSharp.Interface
{
	public interface IWebshot
	{
		string GetIPAddress();

		Task<string> ScreenShot(string url, ViewPortOptions viewPortOptions = null, ScreenshotOptions screenshotOptions = null, string waitForFunc = "() => window.appLoaded === true", List<CookieParam> cookieParam = null);
	}
}
