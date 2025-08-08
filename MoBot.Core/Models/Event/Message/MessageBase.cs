using MoBot.Core.Models.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Event.Message
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum MessageType
	{
		unknow = 0,
		@private = 1,
		group = 2
	}
	[JsonConverter(typeof(StringEnumConverter))]
	public enum SubType
	{
		unknow = 0,
		group = 1,
		@public = 2,
	}
	public class MessageBase : EventPacketBase
	{
		/// <summary>
		/// 消息类型
		/// </summary>
		[JsonProperty("message_type")]
		public MessageType MessageType { get; set; } = MessageType.unknow;

		/// <summary>
		/// 表示消息的子类型，如 group, public
		/// </summary>
		[JsonProperty("sub_type")]
		public string SubType { get; set; } = "";

		/// <summary>
		/// 消息 ID
		/// </summary>
		[JsonProperty("message_id")]
		public long MessageId { get; set; } = 0;

		/// <summary>
		/// 发送者 QQ 号
		/// </summary>
		[JsonProperty("user_id")]
		public long UserId { get; set; } = 0;

		/// <summary>
		/// 消息链，封装具体消息内容
		/// </summary>
		[JsonProperty("message")]
		public List<MessageSegment> Message { get; set; } = new();

		/// <summary>
		/// 原始消息字符串（CQ码格式）
		/// </summary>
		[JsonProperty("raw_message")]
		public string RawMessage { get; set; } = "";

		/// <summary>
		/// 字体，通常为 0
		/// </summary>
		[JsonProperty("font")]
		public int Font { get; set; } = 0;

		/// <summary>
		/// 发送者信息
		/// </summary>
		[JsonProperty("sender")]
		public Sender Sender { get; set; } = new();
	}
}
