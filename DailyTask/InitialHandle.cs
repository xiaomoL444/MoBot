using DailyTask.Constant;
using DailyTask.Models;
using DailyTask.Tool;
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

namespace DailyTask
{
	public class InitialHandle : IInitializer
	{
		private readonly ILogger<InitialHandle> _logger;
		private readonly IDataStorage _dataStorage;

		private readonly ISchedulerFactory _schedulerFactory;
		private readonly IJobListener _jobListener;
		public IRootModel RootModel => new DailyTaskRootModel();

		public InitialHandle(
			ILogger<InitialHandle> logger,
			IDataStorage dataStorage,
			ILoggerFactory loggerFactory,
			ISchedulerFactory schedulerFactory,
			IJobListener jobListener)
		{
			_logger = logger;
			_dataStorage = dataStorage;
			GlobalDataStorage.DataStorage = dataStorage;
			GlobalLogger.LoggerFactory = loggerFactory;
			_schedulerFactory = schedulerFactory;
			_jobListener = jobListener;
		}

		public async Task Initialize()
		{
			var config = _dataStorage.Load<Config>("config");
			if (!CronExpression.IsValidExpression(config.DailyPoemsCron))
			{
				_logger.LogError("Cron表达式非法");
				return;
			}
			//添加定时调度器
			var scheduler = await _schedulerFactory.GetScheduler();

			var DailyPoemsJob = JobBuilder.Create<FetchPoems>()
				.WithIdentity(new JobKey("DailyPoems", Constants.JobGroup))
				.SetJobData(new JobDataMap() {
					new KeyValuePair<string, object>("DataStorage", _dataStorage) ,
					new KeyValuePair<string, object>("Logger",_logger)
				})
				.Build();
			var DailyPoemsTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("DailyPoemsTrigger", Constants.TriggerGroup))
				.WithCronSchedule(config.DailyPoemsCron)
				.Build();
			await scheduler.ScheduleJob(DailyPoemsJob, DailyPoemsTrigger);

			var DailyPraiseJob = JobBuilder.Create<DailyPraise>()
				.WithIdentity(new JobKey("DailyPraise", Constants.JobGroup))
				.SetJobData(new JobDataMap() {
					new KeyValuePair<string, object>("DataStorage", _dataStorage) ,
					new KeyValuePair<string, object>("Logger",_logger)
				})
				.Build();
			var DailyPraiseTrigger = TriggerBuilder.Create()
				.WithIdentity(new TriggerKey("DailyPraiseTrigger", Constants.TriggerGroup))
				.WithCronSchedule(config.DailyPraiseCron)
				.Build();
			await scheduler.ScheduleJob(DailyPraiseJob, DailyPraiseTrigger);

			scheduler.ListenerManager.AddJobListener(new JobFinishedListener(_logger), GroupMatcher<JobKey>.GroupContains(Constants.JobGroup));

			return;
		}
	}
}
