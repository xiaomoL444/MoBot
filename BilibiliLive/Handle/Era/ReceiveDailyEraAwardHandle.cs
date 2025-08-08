using BilibiliLive.Constant;
using BilibiliLive.Manager.Era;
using BilibiliLive.Manager.Era.Factory;
using BilibiliLive.Models.Config;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Core.Models.Net;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle.Era
{
	public class ReceiveDailyEraAwardHandle : IMessageHandle<Group>
	{
		private readonly IDataStorage _dataStorage;
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/领取当日奖励 [游戏] <序号>";

		public string Description => "领取当日奖励";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";

		public ReceiveDailyEraAwardHandle(IDataStorage dataStorage)
		{
			_dataStorage = dataStorage;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.SplitMsg(" ")[0] == "/领取当日奖励")
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			Func<List<MessageSegment>, Task<ActionPacketRsp>> sendMessage = async (chain) => { return await MessageSender.SendGroupMsg(message.GroupId, chain); };

			var args = message.SplitMsg(" ");
			var liveArea = "";
			var uidList = new List<string>();

			var messageChain = MessageChainBuilder.Create().Reply(message);
			//校验参数
			if (args.Count <= 1)
			{
				await sendMessage(messageChain.Text("需要提供开播分区").Build());
				return;
			}
			if (args.Count >= 4)
			{
				await sendMessage(messageChain.Text("参数数量错误").Build());
				return;
			}

			//校验第一个参数，应为开播分区
			liveArea = Tool.LiveAreaKeyWordMatch.Match(args[1]);
			if (string.IsNullOrEmpty(liveArea))
			{
				await sendMessage(messageChain.Text("提供的分区信息无效").Build());
				return;
			}

			//先获取所有的用户
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			uidList = accountConfig.Users.Where(q => q.LiveDatas.Any(l => l.LiveArea == liveArea)).Select(s => s.Uid).ToList();

			//第二个参数是可选参数，代表指定的用户序号，若存在则覆盖前面的
			if (args.Count == 3)
			{
				if (int.TryParse(args[2], out int num) && accountConfig.Users.Count >= num + 1)
				{
					uidList = new() { accountConfig.Users[num].Uid };
				}
				else
				{
					await sendMessage(messageChain.Text("提供的用户序号无效").Build());
					return;
				}
			}

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			_ = Task.Run(async () =>
			{
				//等待是否需要生成图片
				try
				{
					await Task.Delay(1 * 1000, cancellationTokenSource.Token);
					await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Reply(message).Text("勾修金sama请稍等...(预计等待一分钟...)").Build());
				}
				catch (OperationCanceledException)
				{
				}
			});

			var result = await EraLogicFactory.GetLogic(liveArea).ReceiveDailyEraAward(uidList);
			await result.Match(async success =>
						{
							await MessageSender.SendGroupMsg(message.GroupId, messageChain.Image($"base64://{success.Value}").Build());
						}, async error =>
						{
							cancellationTokenSource.Cancel();
							await MessageSender.SendGroupMsg(message.GroupId, messageChain.Reply(message).Text(error.Value).Build());
						});
		}
	}
}
