using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	internal class EraTaskConfig
	{
		[JsonProperty("task_title")]
		public string TaskTitle { get; set; } = string.Empty;//任务标题“如原神5.8激励计划”

		[JsonProperty("live_task_ids")]
		public List<string> LiveTaskIDs { get; set; } = new();//所有直播任务的id

		[JsonProperty("view_task_ids")]
		public List<string> ViewTaskIDs { get; set; } = new();//所有看播任务的id

		[JsonProperty("activity_id")]
		public string ActivityID { get; set; } = string.Empty;//活动id
	}
}
