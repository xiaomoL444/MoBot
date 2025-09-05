using BilibiliLive.Handle;
using Destructurama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Handle;
using MoBot.Handle.DataStorage;
using MoBot.Handle.Net;
using MoBot.Infra.PlayWright.Webshot;
using MoBot.Infra.PlayWright.WebshotRequestStore;
using MoBot.Infra.Quartz.JobListener;
using Quartz;
using Quartz.Impl.Matchers;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net;

string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} : {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.Destructure.ToMaximumStringLength(500) // 限制字符串属性长度为100
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
			services.AddJobListener();//使用计时器的Log输出
			services.AddWebshot();//添加截图程序
			services.AddWebshotRequestStore();//添加截图程序索要数据的伺服器

			//Bot客户端
			services.AddScoped<IMoBotClient, MoBotClient>();
			services.AddScoped<IBotSocketClient, ConsoleClient>();

			//添加事件

			//B站
			services.AddScoped<IInitializer, BilibiliLive.Handle.InitialHandle>();//B站初始化

			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.EchoHandle>();//复活吧我的爱人

			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Account.SignHandle>();//B站登录
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Account.ListHandle>();//B站列出登录了的用户
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Account.DeleteUserHandle>();//B站删除用户
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Account.QueryBattery>();//查询电池数量


			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Stream.StartLiveHandle>();//B站开始直播
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Stream.StopLiveHandle>();//B站关闭直播
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Stream.ViewLiveStateHandle>();//B站查看直播状态
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Stream.FinishGiftTaskHandle>();//B站完成投喂任务

			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Era.QueryTasksHandle>();//B站查询激励计划任务
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Era.ReceiveDailyEraAwardHandle>();//B站领取每日任务
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Era.RefreshEraDataHandle>();//B站在版更时刷新任务
			services.AddScoped<IMessageHandle<Group>, BilibiliLive.Handle.Job.ChangeAutoLiveHandle>();//开启自动直播

			//关键词回复
			//services.AddScoped<IMessageHandle<Group>, DailyChat.EchoHandle>();//自定义回复

			//每日定时任务
			services.AddScoped<IInitializer, DailyTask.InitialHandle>();//每日定时任务初始化
			services.AddScoped<IMessageHandle<Group>, DailyTask.DailyTaskHandle>();//每日定时任务（古文和夸夸）

			//帮助列表
			services.AddScoped<IInitializer, ModelManager.Handle.InitialHandle>();//Bot模块管理器初始化
			services.AddScoped<IMessageHandle<Group>, ModelManager.Handle.GetHelpHandle>();//帮助列表
			services.AddScoped<IMessageHandle<Group>, ModelManager.Handle.GetSystemStatusHandle>();//获得系统运行状态

			services.AddQuartz();
			services.AddQuartzHostedService(option => { option.WaitForJobsToComplete = true; });

			server.Add(services); // 拷贝或保存原 services

		})
		.Build();

	await (await host.Services.GetRequiredService<ISchedulerFactory>().GetScheduler()).Start();
	_ = Task.Run(async () => { await Task.Delay(10 * 1000); await ShowAllJobsAsync(await host.Services.GetRequiredService<ISchedulerFactory>().GetScheduler()); });

	var MoBotClient = host.Services.GetRequiredService<IMoBotClient>();

	MoBotClient.Initial();

	while (true) ;
}
catch (Exception ex)
{
	Log.Logger.Error(ex, "错误");
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
