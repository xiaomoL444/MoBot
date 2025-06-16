using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class WebQRcodeGenerate
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
		/// TTL，恒为 1
		/// </summary>
		[JsonProperty("ttl")]
		public int Ttl { get; set; } = 1;

		/// <summary>
		/// 信息本体
		/// </summary>
		[JsonProperty("data")]
		public WebQRcodeGenerateData Data { get; set; } = new WebQRcodeGenerateData();
	}
	public class WebQRcodeGenerateData
	{
		/// <summary>
		/// 二维码内容（登录页面 URL）
		/// </summary>
		[JsonProperty("url")]
		public string Url { get; set; } = string.Empty;

		/// <summary>
		/// 扫码登录秘钥，恒为 32 字符
		/// </summary>
		[JsonProperty("qrcode_key")]
		public string QrcodeKey { get; set; } = string.Empty;
	}
}
