using MoBot.Core.Interfaces;
using MoBot.Core.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoBot.Handle.DataStorage
{
	public class JsonDataStorage : IDataStorage
	{
		private object _lock = new();
		public T Load<T>(string fileName, DirectoryType directoryType = DirectoryType.Config, string pluginName = "") where T : new()
		{
			lock (_lock)
			{
				{
					pluginName = string.IsNullOrEmpty(pluginName) ? (Assembly.GetCallingAssembly().GetName().Name ?? "Unknown") : pluginName;
					string path = GetFilePath(GetDirectoryName(directoryType), pluginName, fileName);
					CreateDirectory(path);
					if (!File.Exists(path))
					{
						//不存在当即创建新的
						Save<T>(fileName, new(), directoryType, pluginName);
						return new T();
					}
					string json = File.ReadAllText(path);
					return JsonConvert.DeserializeObject<T>(json) ?? new T();
				}
			}
		}

		public void Save<T>(string fileName, T data, DirectoryType directoryType = DirectoryType.Config, string pluginName = "")
		{
			lock (_lock)
			{
				pluginName = string.IsNullOrEmpty(pluginName) ? (Assembly.GetCallingAssembly().GetName().Name ?? "UnknownPlugin") : pluginName;
				string path = GetFilePath(GetDirectoryName(directoryType), pluginName, fileName);
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);

				string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { Formatting = Formatting.Indented });
				File.WriteAllText(path, json);
			}
		}

		public string GetPath(DirectoryType directoryType, string pluginName = "")
		{
			pluginName = string.IsNullOrEmpty(pluginName) ? (Assembly.GetCallingAssembly().GetName().Name ?? "UnknownPlugin") : pluginName;
			var path = "./" + Path.Combine(GetDirectoryName(directoryType), pluginName);
			CreateDirectory(path);
			return path;
		}
		private string GetDirectoryName(DirectoryType directoryType)
		{
			return directoryType switch
			{
				DirectoryType.Config => "config",
				DirectoryType.Data => "data",
				DirectoryType.Cache => "cache"
			};
		}

		private string GetFilePath(string basePath, string pluginName, string fileName)
		{
			return Path.Combine(basePath, pluginName, $"{fileName}.json");
		}
		private void CreateDirectory(string path)
		{
			var directory = Path.GetDirectoryName(path);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
		}
	}
}
