using Newtonsoft.Json;
using OpenBLive.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Config
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

			[JsonProperty("live_datas")]
			public List<LiveData> LiveDatas { get; set; } = new();

			public class LiveData
			{
				[JsonProperty("live_area")]
				public string LiveArea { get; set; } = string.Empty;

				/// <summary>
				/// 哪个用户给这个直播间发送礼物,uid与次数
				/// </summary>
				[JsonProperty("gift_users")]
				public Dictionary<string, int> GiftUsers { get; set; } = new();

				/// <summary>
				/// 哪个用户看这个直播间
				/// </summary>
				[JsonProperty("view_live_users")]
				public List<string> ViewLiveUsers { get; set; } = new();

				/// <summary>
				/// 哪些用户给这个直播间发送消息,uid与次数
				/// </summary>
				[JsonProperty("send_user_danmuku")]
				public Dictionary<string, int> SendUserDanmuku { get; set; } = new();
			}
		}
	}
}
