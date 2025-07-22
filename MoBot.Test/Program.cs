using BilibiliLive.Handle;
using BilibiliLive.Models;
using DailyChat;
using DailyTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Handle;
using MoBot.Handle.DataStorage;
using MoBot.Handle.Net;
using Newtonsoft.Json;
using Serilog;
using Serilog.Enrichers.WithCaller;
using Serilog.Sinks.SystemConsole.Themes;
using SkiaSharp;
using System.Collections.Specialized;
using System.Web;


string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} : {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.Destructure.ToMaximumStringLength(50) // 限制字符串属性长度为100
	.Enrich.FromLogContext()
	.WriteTo.Console(outputTemplate: outputTemplate, theme: AnsiConsoleTheme.Literate, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
	.WriteTo.File("./logs/log-.txt", outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
	.CreateLogger();

try
{
	var host = Host.CreateDefaultBuilder()
		.UseSerilog()
		.ConfigureServices((builder, server) =>
		{
			//添加必要的插件
			server.AddScoped<IDataStorage, JsonDataStorage>();

			//Bot客户端
			server.AddScoped<IMoBotClient, MoBotClient>();
			server.AddScoped<IBotSocketClient, ConsoleClient>();

			//添加事件
			server.AddScoped<IMessageHandle<Group>, SignHandle>();
			server.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.StreamHandle>();
			server.AddScoped<IMessageHandle<Group>, AccountListHandle>();
			server.AddScoped<IMessageHandle<Group>, EraHandle>();
			//server.AddScoped<IMessageHandle<Group>, DailyChat.EchoHandle>();
			//server.AddScoped<IMessageHandle<Group>, DailyTaskHandle>();
		})
		.Build();

	var MoBotClient = host.Services.GetRequiredService<IMoBotClient>();
	BilibiliLive.Tool.GlobalLogger.LoggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
	MoBotClient.Initial();

	while (true) ;
}
catch (Exception ex)
{
	Log.Error(ex, "错误");
}