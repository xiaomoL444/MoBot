using MoBot.Core.Models.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Event.Message
{
	public class Group : MessageBase
	{
		/// <summary>
		/// 收到事件的机器人 QQ 号
		/// </summary>
		[JsonProperty("self_id")]
		public long SelfId { get; set; }

		/// <summary>
		/// 群号（如果是群消息）
		/// </summary>
		[JsonProperty("group_id")]
		public long GroupId { get; set; }

		/// <summary>
		/// 匿名信息，如果不是匿名消息则为 null（我还不知道里面是什么）
		/// </summary>
		[JsonProperty("anonymous")]
		public object Anonymous { get; set; } = 0;
	}
}
