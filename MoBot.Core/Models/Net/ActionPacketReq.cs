using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Net
{
	public class ActionPacketReq
	{
		[JsonProperty("action")]
		public string Action { get; set; } = "";

		[JsonProperty("params")]
		public object Params { get; set; } = "";

		[JsonProperty("echo")]
		public string Echo { get; set; } = "";
	}
}
