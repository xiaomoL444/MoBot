using BilibiliLive.Constant;
using BilibiliLive.Interaction;
using BilibiliLive.Manager;
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

namespace BilibiliLive.Handle.Account
{
	public class QueryBattery : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/查询电池";

		public string Description => "查询电池数量";

		public string Icon => "./Assets/BilibiliLive/icon/bilibiliTelevision.png";

		private readonly IDataStorage _dataStorage;

		public QueryBattery(IDataStorage dataStorage)
		{
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.IsMsg("/查询电池"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var messageChain = MessageChainBuilder.Create().Reply(message);
			foreach (var user in accountConfig.Users)
			{
				var userInfo = await UserInteraction.GetUserInfo(user.UserCredential);
				var glod = await UserInteraction.GetGlodWallet(user.UserCredential);
				messageChain.Text($"用户 {userInfo.Data.Name} 的硬币数量为 {glod}\n");
			}
			await MessageSender.SendGroupMsg(message.GroupId, messageChain.Build());
		}
	}
}
