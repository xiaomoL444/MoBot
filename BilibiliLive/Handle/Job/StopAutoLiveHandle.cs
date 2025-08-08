using BilibiliLive.Constant;
using BilibiliLive.Handle.Stream;
using BilibiliLive.Models.Config;
using Microsoft.Extensions.Logging;
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

namespace BilibiliLive.Handle.Job
{
	public class StopAutoLiveHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/关闭自动开播 [游戏]";

		public string Description => "关闭自动开播";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";

		private readonly ILogger<FinishGiftTaskHandle> _logger;
		private readonly IDataStorage _dataStorage;

		public StopAutoLiveHandle(
	ILogger<FinishGiftTaskHandle> logger,
	IDataStorage dataStorage)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.SplitMsg(" ")[0] == "/关闭自动开播")
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

			var streamConfig = _dataStorage.Load<StreamConfig>(Constants.StreamFile);

			streamConfig.LiveAreas.FirstOrDefault(q => q.AreaName == liveArea).AutoLive = false;

			sendMessage(MessageChainBuilder.Create().Text("自动直播关闭成功啦~₍˄·͈༝·͈˄*₎◞ ̑̑").Build());

			_dataStorage.Save(Constants.StreamFile, streamConfig);

		}
	}
}
