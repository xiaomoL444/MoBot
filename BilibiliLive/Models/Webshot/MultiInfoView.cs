using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Webshot
{
	internal class MultiInfoView
	{
		[JsonProperty("background")]
		public string Background { get; set; } = string.Empty;

		[JsonProperty("data")]
		public List<MultiInfoData> Data { get; set; } = new();

		public class MultiInfoData
		{
			[JsonProperty("name")]
			public string Name { get; set; }= string.Empty;

			[JsonProperty("info")]
			public string Info { get; set; }=string.Empty;

			[JsonProperty("face")]
			public string Face {  get; set; } = string.Empty;
		}
		[JsonProperty("extra_infos")]
		public List<string> ExtraInfos = new();
	}
}
