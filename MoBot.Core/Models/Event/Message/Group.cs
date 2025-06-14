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
		/// 群号（如果是群消息）
		/// </summary>
		[JsonProperty("group_id")]
		public long GroupId { get; set; }

		/// <summary>
		/// 匿名信息，如果不是匿名消息则为 null（我还不知道里面是什么）
		/// </summary>
		[JsonProperty("anonymous")]
		public Anonymous Anonymous { get; set; } = 0;
	}
	public class Anonymous
	{
		/// <summary>
		/// 匿名用户 ID
		/// </summary>
		[JsonProperty("id")]
		public long ID { get; set; }

		/// <summary>
		/// 匿名用户名称
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// 匿名用户 flag，在调用禁言 API 时需要传入
		/// </summary>
		[JsonProperty("flag")]
		public string Flag { get; set; }
	}
}
