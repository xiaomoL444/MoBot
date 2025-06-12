using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Message
{
	public class MessageSegment
	{
		[JsonProperty("type")]
		public string Type { get; set; } = "type";

		[JsonProperty("data")]
		public object? Data { get; set; } = null;
	}
}
