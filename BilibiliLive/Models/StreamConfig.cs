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
		public string OverlayStreamVideo { get; set; } = "";

		/// <summary>
		/// 推流到了第几个
		/// </summary>
		[JsonProperty("index")]
		public int Index { get; set; } = 0;
	}
}
