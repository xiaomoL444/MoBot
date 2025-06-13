using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Net
{
	public abstract class ActionPacketReq
	{
		[JsonProperty("action")]
		public abstract string Action { get; }

		[JsonProperty("params")]
		public abstract object Params { get; }

		[JsonProperty("echo")]
		public abstract string Echo { get; }
	}
}
