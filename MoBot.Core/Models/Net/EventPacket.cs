using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Net
{
	public class EventPacket
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


	}
}
