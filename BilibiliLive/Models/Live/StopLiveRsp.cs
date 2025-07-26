using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Live
{
	internal class StopLiveRsp
	{
		/// <summary>
		/// 返回值：
		/// 0：成功，65530：token错误（登录错误），-400：没有权限（其他错误码待补充）
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = -400;

		/// <summary>
		/// 错误信息（默认为空）
		/// </summary>
		[JsonProperty("msg")]
		public string Msg { get; set; } = string.Empty;

		/// <summary>
		/// 错误信息（默认为空）
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// 信息本体
		/// </summary>
		[JsonProperty("data")]
		public StopLiveData Data { get; set; } = new StopLiveData();
	}

	public class StopLiveData
	{
		/// <summary>
		/// 是否改变状态：0 表示未改变，1 表示改变
		/// </summary>
		[JsonProperty("change")]
		public int Change { get; set; } = 0;

		/// <summary>
		/// 直播间状态，可能的值：
		/// - PREPARING：准备中
		/// - ROUND：轮播中
		/// </summary>
		[JsonProperty("status")]
		public string Status { get; set; } = string.Empty;
	}
}
