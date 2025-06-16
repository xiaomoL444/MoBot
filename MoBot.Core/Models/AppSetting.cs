using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models
{
	public class AppSetting
	{
		[JsonProperty("server")]
		public ServerConfig Server = new();
		public class ServerConfig
		{
			/// <summary>
			/// Websocket的远程连接
			/// </summary>
			[JsonProperty("ws_url")]
			public string Ws_Url = "ws://127.0.0.1:8489";
		}
	}
}
