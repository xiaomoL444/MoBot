using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	internal class WebQRcodePoll
	{
		/// <summary>
		/// 返回值，0 表示成功
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = 0;

		/// <summary>
		/// 错误信息
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// 信息本体
		/// </summary>
		[JsonProperty("data")]
		public WebQRcodePollData Data { get; set; } = new WebQRcodePollData();
	}

	public class WebQRcodePollData
	{
		/// <summary>
		/// 游戏分站跨域登录 URL，未登录时为空
		/// </summary>
		[JsonProperty("url")]
		public string Url { get; set; } = string.Empty;

		/// <summary>
		/// 用于刷新登录状态的 Refresh Token，未登录时为空
		/// </summary>
		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; } = string.Empty;

		/// <summary>
		/// 登录时间戳（毫秒），未登录时为 0
		/// </summary>
		[JsonProperty("timestamp")]
		public long Timestamp { get; set; } = 0;

		/// <summary>
		/// 登录状态码：
		/// 0：扫码登录成功；
		/// 86038：二维码已失效；
		/// 86090：二维码已扫码未确认；
		/// 86101：未扫码
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = 86101;

		/// <summary>
		/// 扫码状态信息
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;
	}
}
