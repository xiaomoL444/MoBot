using BilibiliLive.Handle;
using BilibiliLive.Models;
using DailyChat;
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

var _dataStorage = new JsonDataStorage();
var streamConfig = _dataStorage.Load<StreamConfig>("stream");
var accountConfig = _dataStorage.Load<AccountConfig>("account");
var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{BilibiliLive.Constant.Constants.BilibiliStartLiveAPI}?room_id={streamConfig.RoomID}&area_v2={streamConfig.AreaV2}&platform={streamConfig.Platform}&csrf={accountConfig.Bili_Jct}");
httpRequestMessage.Headers.Add("cookie", $"SESSDATA={accountConfig.Sessdata};bili_jct={accountConfig.Bili_Jct}");
var response = await BilibiliLive.Tool.HttpClient.SendAsync(httpRequestMessage);
Log.Debug("开启直播的回复{@response}", (await response.Content.ReadAsStringAsync()));

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

			server.AddScoped<IMessageHandle<Group>, DailyChat.EchoHandle>();

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