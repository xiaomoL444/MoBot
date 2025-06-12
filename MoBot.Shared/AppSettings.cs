using Newtonsoft.Json;
using MoBot.Core.Models;
using Newtonsoft.Json.Serialization;

namespace MoBot.Shared
{
	public class AppSettings
	{
		private static readonly Lazy<AppSettings> lazy = new Lazy<AppSettings>(() => new());
		public static AppSettings Instance { get { return lazy.Value; } }

		public static ApplicationSetting config = new();

		private AppSettings()
		{
			//配置文件
			var appsetting_file = "appsettings.json";

			if (!File.Exists(appsetting_file))
			{
				Serilog.Log.Warning($"{System.Environment.ProcessPath}/{appsetting_file}配置文件不存在");
				var defaultConfig = new ApplicationSetting();

				var json = JsonConvert.SerializeObject(defaultConfig, new JsonSerializerSettings
				{
					DefaultValueHandling = DefaultValueHandling.Include,
				});

				File.WriteAllText(appsetting_file, json);
			}
			try
			{
				config = JsonConvert.DeserializeObject<ApplicationSetting>(appsetting_file);
				if (config != null) throw new Exception("配置文件读取失败");
			}
			catch (Exception ex)
			{
				Serilog.Log.Warning($"{ex}配置文件读取失败，请删除{System.Environment.ProcessPath}/{appsetting_file}重新打开程序");
				throw;
			}
		}
	}
}
