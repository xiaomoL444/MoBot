using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MoBot.Core.Models.Event
{
	/// <summary>
	/// 表示该上报的类型, 消息, 消息发送, 请求, 通知, 或元事件
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PostType
	{
		unknow,
		message,
		message_sent,
		request,
		notice,
		meta_event
	}
	public class EventPacketBase
	{
		/// <summary>
		/// 事件发生的unix时间戳
		/// </summary>
		[JsonProperty("time")]
		public long Time { get; set; } = 0;

		/// <summary>
		/// 收到事件的机器人的 QQ 号
		/// </summary>
		[JsonProperty("self_id")]
		public long SelfID { get; set; } = 0;

		/// <summary>
		/// 消息类型
		/// </summary>
		[JsonProperty("post_type")]

		public PostType PostType { get; set; } = PostType.unknow;
	}
}
