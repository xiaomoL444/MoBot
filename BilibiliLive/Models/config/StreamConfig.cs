using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Config
{
	public class StreamConfig
	{
		/// <summary>
		/// 用于推流的视频的文件夹目录
		/// </summary>
		[JsonProperty("stream_video_directory")]
		public string StreamVideoDirectory { get; set; } = "";

		/// <summary>
		/// 推流到了第几个
		/// </summary>
		[JsonProperty("index")]
		public int Index { get; set; } = 0;

		[JsonProperty("live_areas")]
		public List<LiveAreaData> LiveAreas { get; set; } = new();

		public LiveOpenPlatForm LiveOpenPlatForm = new();
	}

	public class LiveAreaData
	{
		[JsonProperty("area_name")]
		public string AreaName { get; set; } = string.Empty;

		/// <summary>
		/// 大分区
		/// </summary>
		[JsonProperty("live_part")]
		public int LivePart { get; set; } = 0;

		/// <summary>
		/// 小分区
		/// </summary>
		[JsonProperty("area")]
		public int Area { get; set; } = 0;
	}

	public class LiveOpenPlatForm
	{
		//直播用
		[JsonProperty("access_key_id")]
		public string AccessKeyId = "";//填入你的accessKeyId，可以在直播创作者服务中心【个人资料】页面获取(https://open-live.bilibili.com/open-manage)
		[JsonProperty("access_key_secret")]
		public string AccessKeySecret = "";//填入你的accessKeySecret，可以在直播创作者服务中心【个人资料】页面获取(https://open-live.bilibili.com/open-manage)
		[JsonProperty("app_id")]
		public string AppId = "";//填入你的appId，可以在直播创作者服务中心【我的项目】页面创建应用后获取(https://open-live.bilibili.com/open-manage)
		[JsonProperty("code")]
		public string Code = "";//填入你的主播身份码Code，可以在互动玩法首页，右下角【身份码】处获取(互玩首页：https://play-live.bilibili.com/)
	}
}
