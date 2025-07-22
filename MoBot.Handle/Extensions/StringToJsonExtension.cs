using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle.Extensions
{
	public static class StringToJsonExtension
	{
		/// <summary>
		/// 尝试序列化成json，通常是log才用
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static object TryPraseToJson(this string value)
		{
			try
			{
				return JObject.Parse(value);
			}
			catch (Exception ex)
			{
				return value;
			}
		}
	}
}
