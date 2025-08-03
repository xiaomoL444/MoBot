using BilibiliLive.Constant;
using BilibiliLive.Job;
using BilibiliLive.Tool;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
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
			IJobListener jobListener)
		{
			_logger = logger;
			GlobalLogger.LoggerFactory = loggerFactory;
			GlobalDataStorage.DataStorage = dataStorage;
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

			//开启http伺服器给webshot传数据
			HttpServer.Start();

			var scheduler = await _schedulerFactory.GetScheduler();
			//注册领取事件
			//看播奖励（00:30前触发）
			var receiveViewAwardJobKey = new JobKey("receiveViewAward", Constants.JobGroup);
			var receiveViewAwardJob = JobBuilder.Create<ReceiveViewAwardJob>()
				//setData
				.WithIdentity(receiveViewAwardJobKey)
				.Build();
			var receiveViewAwardTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("receiveViewAwardTrigger", Constants.TriggerGroup))
				//.WithCronSchedule("50 29 0 * * ?")
				.WithCronSchedule("58 29 0 * * ?")
				.Build();
			await scheduler.ScheduleJob(receiveViewAwardJob, receiveViewAwardTrigger);

			var receiveLiveAwardJobKey = new JobKey("receiveLiveAward", Constants.JobGroup);
			var receiveLiveAwardJob = JobBuilder.Create<ReceiveLiveAwardJob>()
				.WithIdentity(receiveLiveAwardJobKey)
				.Build();
			var receiveLiveAwardTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("receiveLiveAwardTrigger", Constants.TriggerGroup))
				.WithCronSchedule("58 59 0 * * ?")
				.Build();
			await scheduler.ScheduleJob(receiveLiveAwardJob, receiveLiveAwardTrigger);

			scheduler.ListenerManager.AddJobListener(_jobListener, GroupMatcher<JobKey>.GroupEquals(Constants.JobGroup));

			return;
		}
	}
}
