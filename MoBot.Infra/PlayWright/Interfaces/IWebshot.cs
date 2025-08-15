using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PlayWright.Interfaces
{
	public interface IWebshot
	{
		string GetIPAddress();

		Task<string> ScreenShot(string url, BrowserNewContextOptions browserNewContextOptions = null, PageScreenshotOptions pageScreenshotOptions = null, List<Microsoft.Playwright.Cookie> cookieParam = null);
	}
}
