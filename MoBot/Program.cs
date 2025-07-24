using BilibiliLive.Handle;
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
using MoBot.Handle.DestructuringPolicy;
using MoBot.Handle.Net;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime;
using System.Text.Json;


string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {CallerFilePath} {CallerMemberName} {CallerLineNumber} : {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.Destructure.ToMaximumStringLength(100) // 限制字符串属性长度为100
	.Destructure.With<TryParseJsonDestructuringPolicy>()
	.Destructure.JsonNetTypes()
	.Enrich.FromLogContext()
	.WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Literate, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
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
			services.AddScoped<IBotSocketClient, WebSocketClient>();

			//加载插件
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.EchoHandle>();//复活吧我的爱人
			services.AddScoped<IMessageHandle<Group>, StreamHandle>();//直播
			services.AddScoped<IMessageHandle<Group>, SignHandle>();//登录B站
			services.AddScoped<IMessageHandle<Group>, AccountListHandle>();//登录B站
			services.AddScoped<IMessageHandle<Group>, EraHandle>();//激励计划

			services.AddScoped<IMessageHandle<Group>, DailyChat.EchoHandle>();//自定义回复

			services.AddScoped<IMessageHandle<Group>, DailyTask.DailyTaskHandle>();//每日定时任务（古文和夸夸）

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
	Log.Error(ex, ex.ToString());
}