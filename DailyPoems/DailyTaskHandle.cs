using DailyTask.Models;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTask
{
	public class DailyTaskHandle : IMessageHandle<Group>
	{
		private ILogger<DailyTaskHandle> _logger;
		private IDataStorage _dataStorage;

		private IScheduler scheduler;

		private const string DefaultGroup = "Default";

		public DailyTaskHandle(
			ILogger<DailyTaskHandle> logger,
			IDataStorage dataStorage)
		{
			_logger = logger;
			_dataStorage = dataStorage;

		}
		public Task<bool> CanHandleAsync(Group group)
		{
			if (group.IsGroupID(Constant.Constants.OPGroupID) && group.IsMsg("/重载时间"))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group group)
		{
			var config = _dataStorage.Load<Config>("config");
			await RescheduleJob(scheduler, "DailyPoemsTrigger", DefaultGroup, config.DailyPoemsCron);
			return;
		}

		public Task Initial()
		{
			var config = _dataStorage.Load<Config>("config");
			if (!CronExpression.IsValidExpression(config.DailyPoemsCron))
			{
				_logger.LogError("Cron表达式非法");
				return Task.CompletedTask;
			}
			//添加定时调度器
			var schedulerFactory = new StdSchedulerFactory();
			scheduler = schedulerFactory.GetScheduler().Result;
			scheduler.Start();
			_logger.LogDebug("定时调度器开启成功");

			var DailyPoemsJob = JobBuilder.Create<FetchPoems>()
				.SetJobData(new JobDataMap() {
					new KeyValuePair<string, object>("DataStorage", _dataStorage) ,
					new KeyValuePair<string, object>("Logger",_logger)
				})
				.Build();
			var DailyPoemsTrigger = TriggerBuilder.Create()
				.WithIdentity("DailyPoemsTrigger", DefaultGroup)
				.WithCronSchedule(config.DailyPoemsCron)
				.Build();

			scheduler.ScheduleJob(DailyPoemsJob, DailyPoemsTrigger);
			_logger.LogInformation("每日诗文定时开启，设置的corn表达式为{time}，下次触发时间: {next_time}", config.DailyPoemsCron, DailyPoemsTrigger.GetNextFireTimeUtc()?.ToLocalTime());



			return Task.CompletedTask;
		}
		public async Task RescheduleJob(IScheduler scheduler, string triggerName, string triggerGtoup, string newCron)
		{
			var triggerKey = new TriggerKey(triggerName, triggerGtoup);

			// 先获取旧的 Trigger
			var oldTrigger = await scheduler.GetTrigger(triggerKey);
			if (oldTrigger == null)
			{
				_logger.LogError("Trigger 不存在，无法更新");
				return;
			}

			// 创建新 Trigger（带新 Cron）
			var newTrigger = TriggerBuilder.Create()
				.WithIdentity(triggerKey)
				.WithSchedule(CronScheduleBuilder.CronSchedule(newCron))
				.Build();

			// 使用 RescheduleJob 重新绑定
			await scheduler.RescheduleJob(triggerKey, newTrigger);
			_logger.LogInformation($"已更新 {triggerName} 的 Cron 表达式为 {newCron}");
		}

	}

	class FetchPoems : IJob
	{
		public ILogger? Logger { get; set; }
		public IDataStorage? DataStorage { get; set; }
		public Task Execute(IJobExecutionContext context)
		{
			return Task.Factory.StartNew(async () =>
			{
				var config = DataStorage!.Load<Config>("config");
				if (string.IsNullOrEmpty(config.Token))
				{
					Logger!.LogError("每日古文Token不存在");
					await MessageSender.SendGroupMsg(Constant.Constants.OPGroupID, MessageChainBuilder.Create().Text("古诗文token为空，请检查配置文件").Build());
					return;
				}
				HttpRequestMessage requestMessage = new(HttpMethod.Get, $"https://app24.guwendao.net/router/mingju/mingjuXiaobujian.aspx?source=gwd&token={config.Token}&zujianType=0");
				var result = await (await Tool.HttpClient.SendAsync(requestMessage)).Content.ReadAsStringAsync();
				Logger.LogDebug(result);
				if (string.Equals(result, "非法请求"))
				{
					Logger.LogError("偶遇非法请求，拼尽全力无法战胜");
					await MessageSender.SendGroupMsg(Constant.Constants.OPGroupID, MessageChainBuilder.Create().Text("偶遇古诗文非法请求，拼尽全力无法战胜").Build());
					return;
				}
				var json = JsonConvert.DeserializeObject<MingjuListResponse>(result);
				if (json.Code != 200)
				{
					Logger.LogWarning("遇见意外错误，错误码{code}，错误信息{msg}", json.Code, json.Message);
					return;
				}

				var msgChain = MessageChainBuilder.Create();
				var mingjuList = json.Result.MingjuList;

				int randomIndex = Random.Shared.Next(0, mingjuList.Count);
				var msg = $@"{mingjuList[randomIndex].NameStr}

—{mingjuList[randomIndex].Author}《{mingjuList[randomIndex].Source}》
link:https://www.gushiwen.cn/{mingjuList[randomIndex].Guishu switch
				{
					1 => "shiwenv",
					2 => "bookv",
					4 => "juv",
					_ => "归属其他，不明，请前往控制台查看"
				}}_{mingjuList[randomIndex].SourceIdStr}.aspx";

				Logger.LogInformation("古诗文选中的古诗{msg}", msg);
				await MessageSender.SendGroupMsg(Constant.Constants.OPGroupID, MessageChainBuilder.Create().Text(msg).Build());
			});
		}
	}
}
