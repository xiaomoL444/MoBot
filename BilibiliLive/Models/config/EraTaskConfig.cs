using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models.Config
{
	public class EraTaskConfig
	{
		[JsonProperty("era_task_datas")]
		public List<EraTaskData> EraTaskDatas { get; set; } = new();
		public class EraTaskData
		{
			[JsonProperty("game_name")]
			public string GameName { get; set; } = string.Empty;//Era的名字，比如genshin和starrail

			[JsonProperty("task_title")]
			public string TaskTitle { get; set; } = string.Empty;//任务标题“如原神5.8激励计划”

			[JsonProperty("task_ids")]
			public Dictionary<string, List<string>> TaskIDs { get; set; } = new();//所有任务名与ids（如每日奖励(dailyAwared:1,2,3,4)）

			[JsonProperty("activity_id")]
			public string ActivityID { get; set; } = string.Empty;//活动id
		}
		[JsonProperty("exclude_sid")]
		public List<string> ExcludeSid { get; set; } = new();//排除的奖励sid	
	}
}
