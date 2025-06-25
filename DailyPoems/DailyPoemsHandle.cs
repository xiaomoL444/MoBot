using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPoems
{
	public class DailyPoemsHandle : IMessageHandle<Group>
	{
		private ILogger<DailyPoemsHandle> _logger;
		private IDataStorage _storage;

		public DailyPoemsHandle(
			ILogger<DailyPoemsHandle> logger,
			IDataStorage dataStorage)
		{
			_logger = logger;
			_storage = dataStorage;

			//添加定时调度器
			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.Result.Start();
			_logger.LogDebug("定时调度器开启成功");

			var jobDetail = JobBuilder.Create<FetchPoems>().Build();
			var trigger = TriggerBuilder.Create().WithCronSchedule("0 0 13 1/1 * ? *").Build();

			scheduler.Result.ScheduleJob(jobDetail, trigger);
			_logger.LogDebug("调度开启成功");
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			return Task.FromResult(false);
		}

		public Task HandleAsync(Group message)
		{
			return Task.CompletedTask;
		}
	}

	class FetchPoems : IJob
	{
		public Task Execute(IJobExecutionContext context)
		{
			return Task.Factory.StartNew(() =>
			{

			});
		}
	}
}
