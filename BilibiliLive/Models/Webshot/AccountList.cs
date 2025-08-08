using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Webshot
{
	public class AccountList
	{
		[JsonProperty("background")]
		public string BackGround { get; set; } = string.Empty;

		[JsonProperty("image_count")]
		public int ImageCount { get; set; } = 0;

		[JsonProperty("account_infos")]
		public List<AccountInfo> AccountInfos { get; set; } = new();

		public class AccountInfo
		{
			[JsonProperty("name")]
			public string Name { get; set; } = string.Empty;

			[JsonProperty("icon")]
			public string Icon { get; set; } = string.Empty;

			[JsonProperty("info")]
			public string Info { get; set; } = string.Empty;
		}
	}
}
