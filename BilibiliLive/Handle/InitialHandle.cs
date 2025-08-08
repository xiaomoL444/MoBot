using BilibiliLive.Constant;
using BilibiliLive.Job;
using BilibiliLive.Tool;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Infra.PuppeteerSharp.Interface;
using MoBot.Infra.PuppeteerSharp.Interfaces;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle
{
	public class InitialHandle : IInitializer
	{
		private readonly ILogger<InitialHandle> _logger;
		private readonly ISchedulerFactory _schedulerFactory;
		private readonly IJobListener _jobListener;

		public IRootModel RootModel => new BilibiliRootModel();

		public InitialHandle(
			ILogger<InitialHandle> logger,
			ILoggerFactory loggerFactory,
			IDataStorage dataStorage,
			ISchedulerFactory schedulerFactory,
			IJobListener jobListener,
			IWebshot webshot,
			IWebshotRequestStore webshotRequestStore)
		{
			_logger = logger;
			GlobalSetting.LoggerFactory = loggerFactory;
			GlobalSetting.DataStorage = dataStorage;
			GlobalSetting.Webshot = webshot;
			GlobalSetting.WebshotRequestStore = webshotRequestStore;
			_schedulerFactory = schedulerFactory;
			_jobListener = jobListener;
		}
		public async Task Initialize()
		{
			//探测GenerateS服务是否开启
			try
			{
				var result = await Tool.HttpClient.SendAsync(new(HttpMethod.Get, Constants.LiveHeartGenerateSUrl));

				_logger.LogInformation("探测到GenerateS服务");
			}
			catch (Exception ex)
			{
				_logger.LogError("直播GenerateS服务可能未开启，请注意开启");
			}

			var scheduler = await _schedulerFactory.GetScheduler();
			//注册领取事件
			var receiveLiveAwardJob = JobBuilder.Create<ReceiveFinallyAwardJob>()
				.WithIdentity(new JobKey("receiveAward", Constants.JobGroup))
				.Build();
			var receiveLiveAwardTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("receiveLiveAwardTrigger", Constants.TriggerGroup))
				.UsingJobData(new JobDataMap() { new KeyValuePair<string, object>("GameName", "genshin"), new KeyValuePair<string, object>("AwardName", "live") })
				.WithCronSchedule("58 59 0 * * ?")
				.Build();//领取原神直播奖励
			var receiveViewAwardTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("receiveViewAwardTrigger", Constants.TriggerGroup))
				.UsingJobData(new JobDataMap() { new KeyValuePair<string, object>("GameName", "genshin"), new KeyValuePair<string, object>("AwardName", "view") })
				.WithCronSchedule("58 29 0 * * ?")
				.Build();//领取原神看播奖励
			var receiveStarRailLiveTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("receiveStarRailLiveTrigger", Constants.TriggerGroup))
				.UsingJobData(new JobDataMap() { new KeyValuePair<string, object>("GameName", "starrail"), new KeyValuePair<string, object>("AwardName", "live") })
				.WithCronSchedule("58 29 0 * * ?")
				.Build();//领取星铁直播奖励

			await scheduler.ScheduleJob(receiveLiveAwardJob, new[] { receiveLiveAwardTrigger, receiveViewAwardTrigger, receiveStarRailLiveTrigger }, replace: true);

			//自动直播
			var autoLiveJob = JobBuilder.Create<AutoLiveJob>()
				.WithIdentity(new JobKey("autoLive", Constants.JobGroup))
				.Build();
			var autoGenshinLiveTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("autoGenshinLiveTrigger", Constants.TriggerGroup))
				.UsingJobData(new JobDataMap() { new KeyValuePair<string, object>("GameName", "genshin") })
				.WithCronSchedule("0 30 6 * * ?")
				.Build();//原神触发器
			var autoStarRailLiveTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("autoStarRailLiveTrigger", Constants.TriggerGroup))
				.UsingJobData(new JobDataMap() { new KeyValuePair<string, object>("GameName", "starrail") })
				.WithCronSchedule("0 0 8 * * ?")
				.Build();//星铁触发器
			await scheduler.ScheduleJob(autoLiveJob, new[] { autoGenshinLiveTrigger, autoStarRailLiveTrigger
	}, replace: true);

			scheduler.ListenerManager.AddJobListener(_jobListener, GroupMatcher<JobKey>.GroupEquals(Constants.JobGroup));

			return;
		}
	}
}
