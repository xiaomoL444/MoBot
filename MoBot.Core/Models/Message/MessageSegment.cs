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
		[JsonPropertyName("type")]
		public string Type { get; set; } = "type";

		[JsonPropertyName("data")]
		public object? Data { get; set; } = null;
	}
}
