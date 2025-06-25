using BilibiliLive.Handle;
using BilibiliLive.Models;
using DailyChat;
using DailyPoems;
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
using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Specialized;
using System.Web;


Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug() // ✅ 设置为显示 Debug 及以上
	.Enrich.FromLogContext()
	.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Literate)
	.WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day)
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
			//server.AddScoped<IMessageHandle<Group>, SignHandle>();
			//server.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.StreamHandle>();

			//server.AddScoped<IMessageHandle<Group>, DailyChat.EchoHandle>();
			server.AddScoped<IMessageHandle<Group>, DailyPoemsHandle>();
		})
		.Build();

	var MoBotClient = host.Services.GetRequiredService<IMoBotClient>();
	MoBotClient.Initial();

	while (true) ;
}
catch (Exception ex)
{
	Log.Error(ex, "错误");
}