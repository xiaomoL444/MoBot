using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Net
{
	public class PacketRsp
	{
		[JsonProperty("status")]
		public string Status { get; set; } = "";

		[JsonProperty("retcode")]
		public int Retcode { get; set; } = 0;

		[JsonProperty("data")]
		public object? Data { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; } = "";

		[JsonProperty("wording")]
		public string Wording { get; set; } = "";

		[JsonProperty("echo")]
		public string Echo { get; set; } = "";
	}
}
