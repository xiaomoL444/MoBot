using MoBot.Infra.PlayWright.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Infra.PlayWright.Interfaces
{
	public interface IWebshotRequestStore
	{
		void SetNewContent(string uuid, HttpServerContentType contentType, object content);
		string GetIPAddress();
	}
}
