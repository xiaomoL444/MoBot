using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class AccountConfig
	{
		/// <summary>
		/// B站的cookie
		/// </summary>
		[JsonProperty("cookie")]
		public string Cookie { get; set; } = "";

		/// <summary>
		/// 测试用（）远程推流的链接
		/// </summary>
		[JsonProperty("rtmp_url")]
		[Obsolete]
		public string RtmpUrl { get; set; } = "";

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
