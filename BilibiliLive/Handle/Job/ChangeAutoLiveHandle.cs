using BilibiliLive.Constant;
using BilibiliLive.Handle.Stream;
using BilibiliLive.Interaction;
using BilibiliLive.Models.Config;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Core.Models.Net;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle.Job
{
	public class ChangeAutoLiveHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/开启(关闭)自动开播 [游戏]";

		public string Description => "开关自动开播";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";

		private readonly ILogger<FinishGiftTaskHandle> _logger;
		private readonly IDataStorage _dataStorage;

		public ChangeAutoLiveHandle(
	ILogger<FinishGiftTaskHandle> logger,
	IDataStorage dataStorage)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && (message.SplitMsg(" ")[0] == "/开启自动开播" || message.SplitMsg(" ")[0] == "/关闭自动开播"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			Func<List<MessageSegment>, Task<ActionPacketRsp>> sendMessage = async (chain) => { return await MessageSender.SendGroupMsg(message.GroupId, chain); };
			var args = message.SplitMsg(" ");
			var liveArea = string.Empty;
			var uidList = new List<string>();

			//校验参数
			if (args.Count <= 1)
			{
				await sendMessage(MessageChainBuilder.Create().Text("需要提供开播分区").Build());
				return;
			}
			if (args.Count >= 4)
			{
				await sendMessage(MessageChainBuilder.Create().Text("参数数量错误").Build());
				return;
			}

			//校验第一个参数，应为开播分区
			liveArea = Tool.LiveAreaKeyWordMatch.Match(args[1]);
			if (string.IsNullOrEmpty(liveArea))
			{
				await sendMessage(MessageChainBuilder.Create().Text("提供的分区信息无效").Build());
				return;
			}

			//先获取所有的用户
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			uidList = accountConfig.Users.Where(q => ((q.Owner == message.UserId) || (message.UserId == Constants.OPAdmin)) && q.LiveDatas.Any(l => l.LiveArea == liveArea)).Select(s => s.Uid).ToList();

			//第二个参数是可选参数，代表指定的用户序号，若存在则覆盖前面的
			if (args.Count == 3)
			{
				if (int.TryParse(args[2], out int num) && accountConfig.Users.Count >= num + 1)
				{
					uidList = new() { accountConfig.Users[num].Uid };
				}
				else
				{
					await sendMessage(MessageChainBuilder.Create().Text("提供的用户序号无效").Build());
					return;
				}
			}

			var msg = string.Empty;
			foreach (var uid in uidList)
			{
				try
				{
					bool status = message.SplitMsg(" ")[0] == "/开启自动开播" ? true : false;
					var liveData = accountConfig.Users.FirstOrDefault(q => q.Uid == uid).LiveDatas.FirstOrDefault(q => q.LiveArea == liveArea);

					if (liveData == null)
					{
						msg += $"该用户未开启{liveArea}分区直播\n";
						continue;
					}

					liveData.AutoLive = status;
					msg += $"将[{(await UserInteraction.GetUserInfo(accountConfig.Users.FirstOrDefault(q => q.Uid == uid).UserCredential)).Data.Name}]的{liveArea}自动直播状态更改为{status}\n";
				}
				catch (Exception ex)
				{
					_logger.LogError("更改{user}的直播状态错误", uid);
					msg += $"更改[{uid}]用户直播状态失败";
				}
			}

			await sendMessage(MessageChainBuilder.Create().Text($"(●• ̀ω•́ )✧自动直播更改状态信息~\n{msg}").Build());

			_dataStorage.Save(Constants.AccountFile, accountConfig);

		}
	}
}
