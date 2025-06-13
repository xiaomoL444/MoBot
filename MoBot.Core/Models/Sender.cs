using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models
{
	public class Sender
	{
		/// <summary>
		/// 发送者的ID
		/// </summary>
		[JsonProperty("user_id")]
		public long UserId { get; set; } = 0;

		/// <summary>
		/// 用户昵称
		/// </summary>
		[JsonProperty("nicklname")]
		public string NickName { get; set; } = "";

		/// <summary>
		/// 我也不知道是什么
		/// </summary>
		[JsonProperty("card")]
		public string Card { get; set; } = "";

		/// <summary>
		/// 身份，管理员什么的吧
		/// </summary>
		[JsonProperty("role")]
		public string Role { get; set; } = "";
		/// <summary>
		/// 我还是不知道是什么，记得再改
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; } = "";
	}
}
