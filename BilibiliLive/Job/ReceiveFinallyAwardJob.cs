using BilibiliLive.Constant;
using BilibiliLive.Manager.Era;
using BilibiliLive.Manager.Era.Factory;
using BilibiliLive.Models;
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
	sealed class ReceiveFinallyAwardJob : IJob
	{
		public string GameName { private get; set; }
		public string AwardName { private get; set; }
		private readonly ILogger _logger = GlobalSetting.CreateLogger<AutoLiveJob>();
		private IDataStorage _dataStorage = GlobalSetting.DataStorage;
		public async Task Execute(IJobExecutionContext context)
		{
			_logger.LogInformation("正在执行{game}的领取{awardType}任务", GameName, AwardName);
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var uidList = accountConfig.Users.Where(q => q.LiveDatas.Any(l => l.LiveArea == GameName)).Select(s => s.Uid).ToList();
			var result = await EraLogicFactory.GetLogic(GameName).ReceiveFinallylEraAward(uidList, AwardName);
			var messageChain = MessageChainBuilder.Create().Text($"{GameName}的领取{AwardName}任务情况");
			bool isNeedSend = true;
			result.Switch(success =>
			{
				messageChain.Image($"base64://{success.Value}");
			}, error =>
			{
				messageChain.Text(error.Value);
			},
			none =>
			{
				isNeedSend = false;
			});
			if (isNeedSend)
			{
				await MessageSender.SendGroupMsg(Constants.OPGroupID, messageChain.Build());
			}
			return;
		}
	}
}
