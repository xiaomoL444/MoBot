using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	internal class LiveRoomInfoRsp
	{
		/// <summary>
		/// 返回码：
		/// 0：成功，-400：请求错误
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = -400;

		/// <summary>
		/// 错误信息，默认为空字符串
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// 固定值 1
		/// </summary>
		[JsonProperty("ttl")]
		public int Ttl { get; set; } = 1;

		/// <summary>
		/// 信息本体
		/// </summary>
		[JsonProperty("data")]
		public LiveRoomInfo Data { get; set; } = new LiveRoomInfo();

		public class LiveRoomInfo
		{
			/// <summary>
			/// 直播间状态：
			/// 0：无房间，1：有房间
			/// </summary>
			[JsonProperty("roomStatus")]
			public int RoomStatus { get; set; } = 0;

			/// <summary>
			/// 轮播状态：
			/// 0：未轮播，1：轮播中
			/// </summary>
			[JsonProperty("roundStatus")]
			public int RoundStatus { get; set; } = 0;

			/// <summary>
			/// 直播状态：
			/// 0：未开播，1：直播中
			/// </summary>
			[JsonProperty("live_status")]
			public int LiveStatus { get; set; } = 0;

			/// <summary>
			/// 直播间网页 URL
			/// </summary>
			[JsonProperty("url")]
			public string Url { get; set; } = string.Empty;

			/// <summary>
			/// 直播间标题
			/// </summary>
			[JsonProperty("title")]
			public string Title { get; set; } = string.Empty;

			/// <summary>
			/// 直播间封面 URL
			/// </summary>
			[JsonProperty("cover")]
			public string Cover { get; set; } = string.Empty;

			/// <summary>
			/// 直播间人气值（为上次直播时刷新）
			/// </summary>
			[JsonProperty("online")]
			public int Online { get; set; } = 0;

			/// <summary>
			/// 直播间 ID（短号）
			/// </summary>
			[JsonProperty("roomid")]
			public int RoomId { get; set; } = 0;

			/// <summary>
			/// 广播类型（通常为 0）
			/// </summary>
			[JsonProperty("broadcast_type")]
			public int BroadcastType { get; set; } = 0;

			/// <summary>
			/// 是否隐藏在线人数（0 = 显示）
			/// </summary>
			[JsonProperty("online_hidden")]
			public int OnlineHidden { get; set; } = 0;
		}
	}
}
