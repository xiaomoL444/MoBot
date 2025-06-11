using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Message
{
	public class PrivateMsg
	{
		[JsonPropertyName("user_id")]
		public long UserID { get; }

		public List<MessageSegment> MessageSegment = new List<MessageSegment>();
	}
}
