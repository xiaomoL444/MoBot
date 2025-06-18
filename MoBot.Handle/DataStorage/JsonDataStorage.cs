using MoBot.Core.Interfaces;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoBot.Handle.DataStorage
{
	public class JsonDataStorage : IDataStorage
	{
		private object _lock = new();
		public T Load<T>(string fileName, string basePath = "configs", string pluginName = "") where T : new()
		{
			lock (_lock)
			{
				{
					pluginName = string.IsNullOrEmpty(pluginName) ? (Assembly.GetCallingAssembly().GetName().Name ?? "UnknownPlugin") : pluginName;
					string path = GetFilePath(basePath, pluginName, fileName);

					if (!File.Exists(path))
					{
						//不存在当即创建新的
						Save<T>(fileName, new(), basePath, pluginName);
						return new T();
					}
					string json = File.ReadAllText(path);
					return JsonConvert.DeserializeObject<T>(json) ?? new T();
				}
			}
		}

		public void Save<T>(string fileName, T data, string basePath = "configs", string pluginName = "")
		{
			lock (_lock)
			{
				pluginName = string.IsNullOrEmpty(pluginName) ? (Assembly.GetCallingAssembly().GetName().Name ?? "UnknownPlugin") : pluginName;
				string path = GetFilePath(basePath, pluginName, fileName);
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);

				string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { Formatting = Formatting.Indented });
				File.WriteAllText(path, json);
			}
		}

		private string GetFilePath(string basePath, string pluginName, string fileName)
		{
			return Path.Combine(basePath, pluginName, $"{fileName}.json");
		}
	}
}
