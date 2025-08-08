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

namespace BilibiliLive.Handle.Account
{
	public class ListHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/账号列表";

		public string Description => "列出登录了的账户";

		public string Icon => "./Assets/BilibiliLive/icon/UI_Icon_Paimon.png";

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.IsMsg("/账号列表"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			var result = await AccountManager.ShowUserList();
			result.Switch(async success =>
			{
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text("(●• ̀ω•́ )✧末酱为勾修金sama找到了的用户").Image($"base64://{success.Value}").Build());
			}, async error =>
			{
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text(error.Value).Build());
			});
		}
	}
}
