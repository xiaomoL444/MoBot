using BilibiliLive.Constant;
using BilibiliLive.Manager;
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
	/// <summary>
	/// 更新激励计划
	/// </summary>
	public class RefreshEraDataHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/更新激励计划";

		public string Description => "更新激励计划";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.IsMsg("/更新激励计划"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			Action<List<MessageSegment>> sendMessage = async (chain) => { await MessageSender.SendGroupMsg(message.GroupId, chain); };
			await EraManager.RefreshEraData(sendMessage);
		}
	}
}
