using Newtonsoft.Json;
using OpenBLive.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class AccountConfig
	{
		[JsonProperty("user")]
		public List<User> Users { get; set; } = new();
		public class User
		{
			/// <summary>
			/// 用户的id
			/// </summary>
			[JsonProperty("uid")]
			public string Uid { get; set; } = string.Empty;

			/// <summary>
			/// 用户的凭证
			/// </summary>
			[JsonProperty("userCredential")]
			public UserCredential UserCredential { get; set; } = new();

			/// <summary>
			/// 是否是要开播的账号
			/// </summary>
			[JsonProperty("is_start_live")]
			public bool IsStartLive { get; set; } = false;

			/// <summary>
			/// 发送给哪个用户的直播礼物
			/// </summary>
			[JsonProperty("")]
			public List<string> GiftUsers { get; set; } = new();

			/// <summary>
			/// 看哪个用户的直播间
			/// </summary>
			public List<string> ViewLiveUsers { get; set; } = new();
		}
	}
}
