using BilibiliLive.Handle;
using BilibiliLive.Models;
using DailyChat;
using DailyTask;
using Destructurama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models;
using MoBot.Core.Models.Event.Message;
using MoBot.Handle;
using MoBot.Handle.DataStorage;
using MoBot.Handle.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Specialized;
using System.Web;

string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} : {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.Destructure.ToMaximumStringLength(100) // 限制字符串属性长度为100
	.Destructure.JsonNetTypes()
	.Enrich.FromLogContext()
	.WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Literate, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
	.WriteTo.File("./logs/log-.txt", outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
	.CreateLogger();

IServiceCollection services = new ServiceCollection();

try
{
	var host = Host.CreateDefaultBuilder()
		.UseSerilog()
		.ConfigureServices((builder, server) =>
		{
			//添加必要的插件
			services.AddScoped<IDataStorage, JsonDataStorage>();

			//Bot客户端
			services.AddScoped<IMoBotClient, MoBotClient>();
			services.AddScoped<IBotSocketClient, ConsoleClient>();

			//添加事件
			services.AddScoped<IMessageHandle<Group>, SignHandle>();
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.StreamHandle>();
			services.AddScoped<IMessageHandle<Group>, AccountListHandle>();
			services.AddScoped<IMessageHandle<Group>, EraHandle>();
			//server.AddScoped<IMessageHandle<Group>, DailyChat.EchoHandle>();
			//server.AddScoped<IMessageHandle<Group>, DailyTaskHandle>();

			server.Add(services); // 拷贝或保存原 services

		})
		.Build();

	//创建文件夹
	List<string> directoryList = Enum.GetValues(typeof(DirectoryType)).Cast<DirectoryType>().ToList().Select(q => q.ToString().ToLower()).ToList();
	foreach (var directory in directoryList)
	{
		if (!Directory.Exists(directory))
		{
			Log.Warning("{d}不存在，创建中", directory);
			Directory.CreateDirectory("./" + directory);
		}
		foreach (var serive in services)
		{
			var path = $"./{directory}/{(serive.ImplementationType.Assembly.GetName().Name ?? "Unknown")}/";
			if (!Directory.Exists(path))
			{
				Log.Warning("{p}不存在，创建中", path);
				Directory.CreateDirectory(path);
			}
		}
	}


	var MoBotClient = host.Services.GetRequiredService<IMoBotClient>();
	BilibiliLive.Tool.GlobalLogger.LoggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
	BilibiliLive.Tool.GlobalDataStorage.DataStorage = host.Services.GetRequiredService<IDataStorage>();
	MoBotClient.Initial();

	while (true) ;
}
catch (Exception ex)
{
	Log.Error(ex, "错误");
}