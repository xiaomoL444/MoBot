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
		[JsonPropertyName("status")]
		public string Status { get; set; } = "";

		[JsonPropertyName("retcode")]
		public int Retcode { get; set; } = 0;

		[JsonPropertyName("data")]
		public object? Data { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; } = "";

		[JsonPropertyName("wording")]
		public string Wording { get; set; } = "";

		[JsonPropertyName("echo")]
		public string Echo { get; set; } = "";
	}
}
