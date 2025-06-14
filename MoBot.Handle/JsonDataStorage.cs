using MoBot.Core.Interfaces;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoBot.Handle
{
	public class JsonDataStorage : IDataStorage
	{
		private readonly string _basePath = "Plugins";
		public T Load<T>(string fileName) where T : new()
		{
			string pluginName = GetCallerPluginName();
			string path = GetFilePath(pluginName, fileName);

			if (!File.Exists(path))
				return new T();

			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<T>(json) ?? new T();
		}

		public void Save<T>(string fileName, T data)
		{
			string pluginName = GetCallerPluginName();
			string path = GetFilePath(pluginName, fileName);
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};

			string json = JsonSerializer.Serialize(data, options);
			File.WriteAllText(path, json);
		}

		private string GetCallerPluginName()
		{
			// 获取调用者（非当前类）的程序集名
			var callingAssembly = Assembly.GetCallingAssembly();
			return callingAssembly.GetName().Name ?? "UnknownPlugin";
		}

		private string GetFilePath(string pluginName, string fileName)
		{
			return Path.Combine(_basePath, pluginName, $"{fileName}.json");
		}
	}
}
