using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelManager.Models.WebShot
{
	public class HelpList
	{
		[JsonProperty("background")]
		public string BackGround { get; set; } = string.Empty;

		[JsonProperty("image_count")]
		public int ImageCount { get; set; } = 0;

		[JsonProperty("model_infos")]
		public List<ModelInfo> ModelInfos { get; set; } = new();

		public class ModelInfo
		{
			[JsonProperty("name")]
			public string Name { get; set; } = string.Empty;

			[JsonProperty("icon")]
			public string Icon { get; set; } = string.Empty;

			[JsonProperty("description")]
			public string Description { get; set; } = string.Empty;

			[JsonProperty("plugin_infos")]
			public List<PluginInfo> PluginInfos { get; set; } = new();

			public class PluginInfo
			{
				[JsonProperty("name")]
				public string Name { get; set; } = string.Empty;

				[JsonProperty("icon")]
				public string Icon { get; set; } = string.Empty;

				[JsonProperty("description")]
				public string Description { get; set; } = string.Empty;
			}
		}
	}
}
