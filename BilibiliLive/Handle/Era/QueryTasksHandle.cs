using BilibiliLive.Constant;
using BilibiliLive.Manager.Era;
using BilibiliLive.Manager.Era.Factory;
using BilibiliLive.Models.Config;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle.Era
{
	public class QueryTasksHandle : IMessageHandle<Group>
	{
		private readonly IDataStorage _dataStorage;
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/查询任务 [游戏] <序号>";

		public string Description => "查询任务";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";

		public QueryTasksHandle(IDataStorage dataStorage)
		{
			_dataStorage = dataStorage;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.SplitMsg(" ")[0] == "/查询任务")
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			Action<List<MessageSegment>> sendMessage = async (chain) => { await MessageSender.SendGroupMsg(message.GroupId, chain); };

			var args = message.SplitMsg(" ");
			var liveArea = "";
			var uidList = new List<string>();

			//校验参数
			if (args.Count <= 1)
			{
				sendMessage(MessageChainBuilder.Create().Text("需要提供开播分区").Build());
				return;
			}
			if (args.Count >= 4)
			{
				sendMessage(MessageChainBuilder.Create().Text("参数数量错误").Build());
				return;
			}

			//校验第一个参数，应为开播分区
			liveArea = Tool.LiveAreaKeyWordMatch.Match(args[1]);
			if (string.IsNullOrEmpty(liveArea))
			{
				sendMessage(MessageChainBuilder.Create().Text("提供的分区信息无效").Build());
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
					sendMessage(MessageChainBuilder.Create().Text("提供的用户序号无效").Build());
					return;
				}
			}

			int resultNum = 0;
			var messageChain = MessageChainBuilder.Create().Text("查询的任务");
			foreach (var uid in uidList)
			{
				_ = Task.Run(async () =>
				{
					var result = await EraLogicFactory.GetLogic(liveArea).QueryTasks(uid);
					result.Switch(success =>
					{
						messageChain.Image($"base64://{success.Value}");
					}, error =>
					{
						messageChain.Text(error.Value);
					});
					Interlocked.Increment(ref resultNum);
				});
			}
			//等待任务查询完毕
			_ = Task.Run(async () =>
			{
				while (true)
				{
					if (resultNum == uidList.Count)
					{
						sendMessage(messageChain.Build());
						return;
					}
					await Task.Delay(1000);
				}
			});
		}
	}
}
