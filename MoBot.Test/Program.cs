using BilibiliLive.Handle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Handle;
using MoBot.Handle.Net;
using MoBot.Shared;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime;
using System.Text.Json;


Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug() // ✅ 设置为显示 Debug 及以上
	.Enrich.FromLogContext()
	.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Literate)
	.WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();



try
{
	var host = Host.CreateDefaultBuilder()
		.ConfigureServices((builder, server) =>
		{
			//Bot客户端
			server.AddScoped<MoBotClient>();
			server.AddScoped<IBotSocketClient, ConsoleClient>();

			//添加事件
			server.AddScoped<IMessageHandle<Group>, EchoHandle>();

		})
		.Build();

	var MoBotClient = host.Services.GetRequiredService<MoBotClient>();
	MoBotClient.Initial();

	while (true) ;
}
catch (Exception ex)
{
	Log.Error(ex,"错误");
}