using MoBot.Infra.PuppeteerSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PuppeteerSharp.Interfaces
{
	public interface IWebshotRequestStore
	{
		void SetNewContent(string uuid, HttpServerContentType contentType, object content);
		string GetIPAddress();
	}
}
