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
		/// Corn表达式的定时，默认是每天13点触发一次
		/// </summary>
		[JsonProperty("daily_poems_cron")]
		public string DailyPoemsCron { get; set; } = "0 0 13 1/1 * ? *";
	}
}
