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
	public class ApplicationSetting
	{
		[JsonProperty("Server")]
		public Server server;
		public class Server
		{
			public string ws_url = "ws://192.168.5.2:8489";
		}
	}
}
