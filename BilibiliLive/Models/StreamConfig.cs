using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class StreamConfig
	{
		/// <summary>
		/// 用于推流的视频的文件夹目录
		/// </summary>
		[JsonProperty("stream_video_directory")]
		public string StreamVideoDirectory { get; set; } = "";

		/// <summary>
		/// 那个大伟的overlay视频位置，不能放在StreamVideoDirectory里面，不然会被读取成主视频
		/// </summary>
		[JsonProperty("overlay_stream_video")]
		[Obsolete]
		public string OverlayStreamVideo { get; set; } = "";

		/// <summary>
		/// 推流到了第几个
		/// </summary>
		[JsonProperty("index")]
		public int Index { get; set; } = 0;

		/// <summary>
		/// 房间ID
		/// </summary>
		[JsonProperty("room_id")]
		public long RoomID { get; set; } = 0;

		/// <summary>
		/// 直播分区 ID（子分区 ID），必要字段，详见直播分区列表
		/// </summary>
		[JsonProperty("area_v2")]
		public int AreaV2 { get; set; } = 321;

		/// <summary>
		/// 直播平台，必要字段：
		///
		/// - 直播姬（PC）：pc_link  
		/// - 网页端在线直播：web_link  
		/// - BiliLink（如安卓）：android_link
		/// </summary>
		[JsonProperty("platform")]
		public string Platform { get; set; } = "web";
	}
}
