using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class TaskInfoRsp
	{
		[JsonProperty("code")]
		public int Code { get; set; } = 0;

		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		[JsonProperty("ttl")]
		public int Ttl { get; set; } = 0;

		[JsonProperty("data")]
		public TaskInfoData Data { get; set; } = new();

		public class TaskInfoData
		{
			[JsonProperty("list")]
			public List<TaskElement> List = new();

			public class TaskElement
			{
				[JsonProperty("task_id")]
				public string TaskID { get; set; } = string.Empty;//任务id

				[JsonProperty("task_name")]

				public string TaskName { get; set; } = string.Empty;//任务名称

				[JsonProperty("task_status")]

				public TaskCompleteStatus TaskStatus { get; set; } = TaskCompleteStatus.未完成;//完成状态

				[JsonProperty("accumulative_count")]
				public string AccumulativeCount { get; set; } = string.Empty;//多任务的完成次数（比如直播任务完成天数）

				[JsonProperty("check_points")]
				public List<CheckPointElement> CheckPoints { get; set; } = new(); //详细任务（）

				[JsonProperty("accumulative_check_points")]
				public List<CheckPointElement> AccumulativeCheckPoints { get; set; } = new();//更多任务（）

				public class CheckPointElement
				{
					[JsonProperty("status")]
					public TaskCompleteStatus Status { get; set; } = TaskCompleteStatus.未完成;//任务完成状态

					[JsonProperty("alias")]
					public string Alias { get; set; } = string.Empty;//名称

					[JsonProperty("award_name")]
					public string AwardName { get; set; } = string.Empty;//奖励名称

					[JsonProperty("award_sid")]
					public string AwardSid { get; set; } = string.Empty;//不知道有什么用

					[JsonProperty("sid")]
					public string Sid { get; set; } = string.Empty;//任务ID，领取时用

				}
			}
			public enum TaskCompleteStatus
			{
				未完成 = 1,//未完成
				已完成但未领取 = 2,//完成但是未领取
				已领取 = 3//已领取
			}
		}
	}
}