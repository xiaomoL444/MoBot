using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle.DestructuringPolicy
{
	public class TryParseJsonDestructuringPolicy : IDestructuringPolicy
	{
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
		{
			result = null;

			if (value is string s)
			{
				try
				{
					// 尝试把 string 解析成 JSON 对象
					var parsed = JsonConvert.DeserializeObject(s);
					if (parsed is JObject jObj)
					{
						result = propertyValueFactory.CreatePropertyValue(jObj, true);
						return true;
					}
					if (parsed is JArray jArr)
					{
						result = propertyValueFactory.CreatePropertyValue(jArr, true);
						return true;
					}
				}
				catch
				{
					// 非 JSON 字符串，忽略
				}
			}

			return false;
		}
	}
}
