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
using MoBot.Handle.Net;
using MoBot.Infra.Quartz.JobListener;
using Quartz;
using Quartz.Impl.Matchers;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime;
using System.Text.Json;


string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {CallerFilePath} {CallerMemberName} {CallerLineNumber} : {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.Destructure.ToMaximumStringLength(100) // 限制字符串属性长度为100
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
			services.AddJobListener();//使用计时器的Log输出

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

			services.AddQuartz();
			services.AddQuartzHostedService(option => { option.WaitForJobsToComplete = true; });

			server.Add(services); // 拷贝或保存原 services
		})
		.Build();

	await (await host.Services.GetRequiredService<ISchedulerFactory>().GetScheduler()).Start();
	_ = Task.Run(async () => { await Task.Delay(2 * 1000); 
		await ShowAllJobsAsync(await host.Services.GetRequiredService<ISchedulerFactory>().GetScheduler()); });

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

async Task ShowAllJobsAsync(IScheduler scheduler)
{
	// 1. 获取所有 Job 分组
	var jobGroups = await scheduler.GetJobGroupNames();

	foreach (var group in jobGroups)
	{
		// 2. 获取该分组下的所有 JobKey
		var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));

		foreach (var jobKey in jobKeys)
		{
			// 3. 获取 Job 的详细信息
			var jobDetail = await scheduler.GetJobDetail(jobKey);

			// 4. 获取该 Job 对应的 Trigger（可能有多个）
			var triggers = await scheduler.GetTriggersOfJob(jobKey);

			foreach (var trigger in triggers)
			{
				var triggerState = await scheduler.GetTriggerState(trigger.Key);

				// 打印信息
				Log.Logger.Information("Job: {JobKey} | Trigger: {TriggerKey} | State: {State} | NextFireTime: {NextFireTimeUtc}",
					jobKey,
					trigger.Key,
					triggerState,
					trigger.GetNextFireTimeUtc()?.ToLocalTime() ?? DateTime.MinValue);
			}
		}
	}
}