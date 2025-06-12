using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Message
{
	internal class GroupMsg
	{
		[JsonPropertyName("group_id")]
		public long GroupID { get; }

		public List<MessageSegment> MessageSegment = new List<MessageSegment>();
	}
}
