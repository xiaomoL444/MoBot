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

		}
	}
}
