using BilibiliLive.Constant;
using BilibiliLive.Manager;
using BilibiliLive.Manager.Era.Factory;
using BilibiliLive.Models.Config;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Message;
using MoBot.Handle.Message;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Job
{
	internal class AutoLiveJob : IJob
	{
		public string GameName{ private get; set; }
		private readonly ILogger _logger = GlobalSetting.CreateLogger<AutoLiveJob>();
		private readonly IDataStorage _dataStorage = GlobalSetting.DataStorage;
		public async Task Execute(IJobExecutionContext context)
		{
			_logger.LogInformation("正在执行{game}的直播任务", GameName);
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);

			var uidList = accountConfig.Users.Where(q => q.LiveDatas.Any(l => l.LiveArea == GameName)).Select(s => s.Uid).ToList();

			var liveResult = await LiveManager.StartLive(uidList, GameName);
			var messageChain = MessageChainBuilder.Create().Text($"{GameName}直播自动任务：\n");
			messageChain.Text("开播：");

			await liveResult.Match(async success =>
			{
				//若成功，准备图片，并且执行投喂任务，开启计时，一个小时后关闭推流
				messageChain.Text("成功：").Image($"base64://{success.Value}");

				await Task.Delay(10 * 1000);

				var giftResult = await LiveManager.FinishGiftTask();
				messageChain.Text("投喂：");
				giftResult.Switch(giftSuccess =>
				{
					messageChain.Text("成功：").Image($"base64://{giftSuccess.Value}");
				}, giftError =>
				{
					messageChain.Text("失败：").Text(giftError.Value);
				});
				messageChain.Text("等待75min后关闭推流...");

				//创建Task等待75min关闭推流
				_ = Task.Run(async () =>
				{
					await Task.Delay(75 * 60 * 1000);
					var overMessageChain = MessageChainBuilder.Create();
					if (LiveManager.IsLive)
					{
						var msg = await LiveManager.StopLive();
						overMessageChain.Text(msg);
					}
					//尝试领取奖励，不管会话是否存活
					overMessageChain.Text("尝试领取每日奖励\n");
					var eraDailyAwardResult = await EraLogicFactory.GetLogic(GameName).ReceiveDailyEraAward(uidList);
					eraDailyAwardResult.Switch(awardSuccess =>
					{
						overMessageChain.Image($"base64://{awardSuccess.Value}");
					}, awardError =>
					{
						overMessageChain.Text(awardError.Value);
					});
					await MessageSender.SendGroupMsg(Constants.OPGroupID, overMessageChain.Build());
				});

			}, async error =>
			{
				messageChain.Text("失败：").Text(error.Value);
			});
			await MessageSender.SendGroupMsg(Constants.OPGroupID, messageChain.Build());
		}
	}
}
