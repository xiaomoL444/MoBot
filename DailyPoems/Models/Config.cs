using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTask.Models
{
	internal class Config
	{
		/// <summary>
		/// 古文岛的token，作用不明，总之先分开来吧
		/// </summary>
		[JsonProperty("token")]
		public string Token { get; set; } = string.Empty;

		/// <summary>
		/// 每日诗文的Cron触发时间
		/// </summary>
		[JsonProperty("daily_poems_cron")]
		public string DailyPoemsCron { get; set; } = "0 0 13 1/1 * ? *";

		/// <summary>
		/// 每日夸夸的触发时间
		/// </summary>
		[JsonProperty("daily_praise_cron")]
		public string DailyPraiseCron { get; set; } = "0 0 8 1/1 * ? *";
	}
}
