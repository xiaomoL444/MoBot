using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Webshot
{
	internal class TaskStatus
	{
		[JsonProperty("face")]
		public string Face { get; set; }

		[JsonProperty("background")]
		public string Background{ get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}
}
