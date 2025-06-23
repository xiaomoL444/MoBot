using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Action
{
	public class GetLoginInfo : ActionBase
	{
		/// <summary>
		/// 用户 ID（建议使用 long 类型避免溢出）
		/// </summary>
		[JsonProperty("user_id")]
		public long UserId { get; set; } = 0;

		/// <summary>
		/// 用户昵称
		/// </summary>
		[JsonProperty("nickname")]
		public string Nickname { get; set; } = string.Empty;
	}
}
