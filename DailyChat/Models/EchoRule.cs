using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyChat.Models
{
	public class EchoRule
	{
		[JsonProperty("reply_items")]
		public ReplyItem ReplyItems { get; set; } = new();
	}

	public class ReplyItem
	{
		/// <summary>
		/// 触发的指令，比如“/贴贴”或者“@末酱 贴贴” 之类的
		/// </summary>
		[JsonProperty("trigger")]
		public List<string> Trigger { get; set; } = [];

		/// <summary>
		/// 白名单，就是特别的回复们
		/// </summary>
		[JsonProperty("white_list")]
		public Response WhiteList = new();

		/// <summary>
		/// 普通回复
		/// </summary>
		[JsonProperty("normal")]
		public Response Normal = new();

		//目前没有打算搞黑名单的打算，因为只在我自己的群里玩回复消息
	}

	public class Response
	{
		/// <summary>
		/// 用户ID，
		/// </summary>
		[JsonProperty("user_id")]
		public long UserID { get; set; } = 0;

		/// <summary>
		/// 回复的问题，多个list代表随机选一条回复
		/// </summary>
		[JsonProperty("message")]
		public List<string> message { get; set; } = new();
	}
	public class MessageContent
	{
		/// <summary>
		/// 消息链，一个item表示只发一次，两个item表示分开发
		/// </summary>
		[JsonProperty("message_chains")]
		public List<MessageChain> MessageChains { get; set; } = new();
	}
	public class MessageChain
	{
		/// <summary>
		///真正的消息，是和llonebot一样，分为type和data 
		/// </summary>
		[JsonProperty("message_items")]
		public List<MessageItem> MessageItems { get; set; } = new();
	}
	public class MessageItem
	{
		/// <summary>
		/// 消息类型
		/// </summary>
		[JsonProperty("message_item")]
		public MessageItemType MessageItemType { get; set; }

		public string content { get; set; } = string.Empty;
	}

	public enum MessageItemType
	{
		text = 0,//文字
		image = 1,//图片
	}
}
