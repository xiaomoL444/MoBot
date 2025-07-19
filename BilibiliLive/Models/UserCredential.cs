using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class UserCredential
	{
		/// <summary>
		/// 用户ID吧
		/// </summary>
		[JsonProperty("dede_user_id")]
		public string DedeUserID { get; set; } = "";

		/// <summary>
		/// cookie的md5（还是不知道是什么）
		/// </summary>
		[JsonProperty("dede_user_id_ckMd5")]
		public string DedeUserID__ckMd5 { get; set; } = "";

		/// <summary>
		/// 不知道是什么
		/// </summary>
		[JsonProperty("expires")]
		public string Expires { get; set; } = "";

		/// <summary>
		/// cookie
		/// </summary>
		[JsonProperty("sessdata")]
		public string Sessdata { get; set; } = "";

		/// <summary>
		/// cookie
		/// </summary>
		[JsonProperty("bili_jct")]
		public string Bili_Jct { get; set; } = "";
	}
}
