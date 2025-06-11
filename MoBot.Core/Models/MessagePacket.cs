using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models
{
	public abstract class MessagePacket
	{
		[JsonPropertyName("action")]
		public abstract string Action { get; }

		[JsonPropertyName("params")]
		public abstract object Params { get; }

		[JsonPropertyName("echo")]
		public abstract string Echo { get; }
	}
}
