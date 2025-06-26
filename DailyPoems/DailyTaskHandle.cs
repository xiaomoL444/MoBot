using DailyTask.Models;
using DailyTask.Constant;
using DailyTask.Tool;
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
using HttpClient = DailyTask.Tool.HttpClient;

namespace DailyTask
{
	public class DailyTaskHandle : IMessageHandle<Group>
	{
		private ILogger<DailyTaskHandle> _logger;
		private IDataStorage _dataStorage;

		private IScheduler scheduler;

		private const string DefaultTriggerGrpup = "Default";
		private const string DefaultJobGrpup = "Default";

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
			await RescheduleOrCreateJob(scheduler, "DailyPoemsJob", DefaultJobGrpup, "DailyPoemsTrigger", DefaultTriggerGrpup, config.DailyPoemsCron);//DailyPoems
			await RescheduleOrCreateJob(scheduler, "DailyPraiseJob", DefaultJobGrpup, "DailyPraiseTrigger", DefaultTriggerGrpup, config.DailyPraiseCron);//每日夸夸

			return;
		}

		public async Task Initial()
		{
			var config = _dataStorage.Load<Config>("config");
			if (!CronExpression.IsValidExpression(config.DailyPoemsCron))
			{
				_logger.LogError("Cron表达式非法");
				return;
			}
			//添加定时调度器
			var schedulerFactory = new StdSchedulerFactory();
			scheduler = schedulerFactory.GetScheduler().Result;
			await scheduler.Start();
			_logger.LogDebug("定时调度器开启成功");

			var DailyPoemsJob = JobBuilder.Create<FetchPoems>()
				.SetJobData(new JobDataMap() {
					new KeyValuePair<string, object>("DataStorage", _dataStorage) ,
					new KeyValuePair<string, object>("Logger",_logger)
				})
				.Build();
			await RescheduleOrCreateJob(scheduler, "DailyPoemsJob", DefaultJobGrpup, "DailyPoemsTrigger", DefaultTriggerGrpup, config.DailyPoemsCron, DailyPoemsJob);//创建DailyPoems的定时任务

			var DailyPraise = JobBuilder.Create<DailyPraise>()
				.SetJobData(new JobDataMap() {
					new KeyValuePair<string, object>("Logger",_logger)
				})
				.Build();
			await RescheduleOrCreateJob(scheduler, "DailyPraiseJob", DefaultJobGrpup, "DailyPraiseTrigger", DefaultTriggerGrpup, config.DailyPraiseCron, DailyPraise);//创建每日夸夸的定时任务


			return;
		}
		public async Task RescheduleOrCreateJob(
			IScheduler scheduler,
			string jobName,
			string jobGroup,
			string triggerName,
			string triggerGroup,
			string cron,
			IJobDetail jobDetail = null)
		{
			var triggerKey = new TriggerKey(triggerName, triggerGroup);
			var jobKey = new JobKey(jobName, jobGroup);

			var existingTrigger = await scheduler.GetTrigger(triggerKey);

			ITrigger newTrigger;

			if (existingTrigger != null)
			{
				//已存在 trigger，重载时间
				newTrigger = TriggerBuilder.Create()
					   .WithIdentity(triggerKey)
					   .WithSchedule(CronScheduleBuilder.CronSchedule(cron))
					   .Build();

				await scheduler.RescheduleJob(triggerKey, newTrigger);
				_logger.LogInformation("已更新 Trigger {triggerName} 的定时时间：{cron}", triggerName, cron);
			}
			else
			{
				// ❌ 不存在 trigger，检查 Job 是否存在
				if (!await scheduler.CheckExists(jobKey))
				{
					// Job 不存在，必须传入 jobDetail 才能创建
					if (jobDetail == null)
						throw new InvalidOperationException("Job 不存在，必须提供 jobDetail 用于注册");

					newTrigger = TriggerBuilder.Create()
						.WithIdentity(triggerKey)
						.WithSchedule(CronScheduleBuilder.CronSchedule(cron))
						.Build();
					await scheduler.ScheduleJob(jobDetail, newTrigger);

					_logger.LogInformation("创建新的 Job 和 Trigger，定时为：{cron}", cron);
				}
				else
				{
					// Job 存在，添加一个新的 trigger
					newTrigger = TriggerBuilder.Create()
						.WithIdentity(triggerKey)
						.ForJob(jobKey)
						.WithSchedule(CronScheduleBuilder.CronSchedule(cron))
						.Build();
					_logger.LogInformation("Job 已存在，添加新 Trigger，定时为{cron}", cron);
				}
			}
			_logger.LogInformation("更新{jobName} {jobGrpup} {triggerName} {triggerGroup}，设置的时间为{time}，下次触发时间: {next_time}", jobName, jobGroup, triggerName, triggerGroup, CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron, new CronExpressionDescriptor.Options
			{
				Locale = "zh-CN",
				Use24HourTimeFormat = true
			}), newTrigger.GetNextFireTimeUtc()?.ToLocalTime());
		}
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
			Logger.LogInformation("触发古文事件");
			var config = DataStorage!.Load<Config>("config");
			if (string.IsNullOrEmpty(config.Token))
			{
				Logger!.LogError("每日古文Token不存在");
				await MessageSender.SendGroupMsg(Constants.OPGroupID, MessageChainBuilder.Create().Text("古诗文token为空，请检查配置文件").Build());
				return;
			}
			HttpRequestMessage requestMessage = new(HttpMethod.Get, $"https://app24.guwendao.net/router/mingju/mingjuXiaobujian.aspx?source=gwd&token={config.Token}&zujianType=0");
			var result = await (await HttpClient.SendAsync(requestMessage)).Content.ReadAsStringAsync();
			Logger.LogDebug(result);
			if (string.Equals(result, "非法请求"))
			{
				Logger.LogError("偶遇非法请求，拼尽全力无法战胜");
				await MessageSender.SendGroupMsg(Constants.OPGroupID, MessageChainBuilder.Create().Text("偶遇古诗文非法请求，拼尽全力无法战胜").Build());
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
			await MessageSender.SendGroupMsg(Constants.OPGroupID, MessageChainBuilder.Create().Text(msg).Build());
		});
	}
}

class DailyPraise : IJob
{

	public ILogger? Logger { get; set; }

	private List<string> _imageUrl = new() { "http://i0.hdslb.com/bfs/new_dyn/31b533ee46fc3ab53d7348079c330440609872107.jpg",
	"http://i0.hdslb.com/bfs/new_dyn/b24e8c4041836ec90c6a5706f52a8091609872107.jpg",
	"http://i0.hdslb.com/bfs/new_dyn/6f985f0cb051634d33471c6e09b2384a609872107.jpg",
	"http://i0.hdslb.com/bfs/new_dyn/7c4bd657f7c30a37c5847bbca12f63cb609872107.jpg" };//bilibili动态 4

	public async Task Execute(IJobExecutionContext context)
	{
		Logger.LogInformation("触发每日夸夸事件");
		var msg = "今天的沫沫也很可爱哦，今天也要继续加油哦~☆";
		_ = Task.Run(async () =>
		{ //发送消息
			await MessageSender.SendGroupMsg(Constants.OPGroupID, MessageChainBuilder.Create().Text(msg).Build());
			await Task.Delay(Random.Shared.Next(500, 1500));
			//发送图片
			await MessageSender.SendGroupMsg(Constants.OPGroupID, MessageChainBuilder.Create().Image(_imageUrl[Random.Shared.Next(0, _imageUrl.Count)]).Build());
		});

		return;
	}
}
