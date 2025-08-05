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
	public class QueryTasksHandle:IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/查询任务";

		public string Description => "查询任务";

		public string Icon => "./Asserts/BilibiliLive/icon/live.png";

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.IsMsg("/查询任务"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			Action<List<MessageSegment>> sendMessage = async (chain) => { await MessageSender.SendGroupMsg(message.GroupId, chain); };
			await EraManager.QueryTasks(sendMessage);
		}
	}
}
