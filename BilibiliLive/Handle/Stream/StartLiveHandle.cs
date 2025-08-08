using BilibiliLive.Constant;
using BilibiliLive.Manager;
using BilibiliLive.Models.Config;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle.Stream
{
	public class StartLiveHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/开始推流 <游戏>";

		public string Description => "开始推流（仅GI,HSR）";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";


		private readonly IDataStorage _dataStorage;

		public StartLiveHandle(IDataStorage dataStorage)
		{
			_dataStorage = dataStorage;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.SplitMsg(" ")[0] == "/开始推流")
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			Action<List<MessageSegment>> sendMessage = async (chain) => { await MessageSender.SendGroupMsg(message.GroupId, chain); };
			var args = message.SplitMsg(" ");
			var liveArea = string.Empty;
			switch (args.Count)
			{
				case 1:
					sendMessage(MessageChainBuilder.Create().Text("需要提供开播分区").Build());
					return;
				case 2:
					liveArea = Tool.LiveAreaKeyWordMatch.Match(args[1]);
					if (string.IsNullOrEmpty(liveArea))
					{
						sendMessage(MessageChainBuilder.Create().Text("提供的分区信息无效").Build());
						return;
					}
					break;
				default:
					sendMessage(MessageChainBuilder.Create().Text("参数无效").Build());
					break;
			}

			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);

			var result = await LiveManager.StartLive(accountConfig.Users.Where(q => q.LiveDatas.Any(l => l.LiveArea == liveArea)).Select(s => s.Uid).ToList(), liveArea);
			result.Switch(success =>
			{
				MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text("直播开启中(＾ω＾)，直播状态").Image($"base64://{success.Value}").Build());
			}, error =>
			{
				MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text(error.Value).Build());
			});
		}
	}
}
