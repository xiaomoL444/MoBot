using BilibiliLive.Constant;
using BilibiliLive.Manager.Era;
using BilibiliLive.Manager.Era.Factory;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
			var messageChain = MessageChainBuilder.Create();

			//获取原神的更新结果
			var genshinResult = await EraLogicFactory.GetLogic("genshin").RefreshEraData();

			genshinResult.Switch(success =>
			{
				messageChain.Text(success.Value);
			}, error =>
			{
				messageChain.Text(error.Value);
			});
			messageChain.Text("\n");

			//获取星铁的更新结果
			var starrailResult = await EraLogicFactory.GetLogic("starrail").RefreshEraData();

			starrailResult.Switch(success =>
			{
				messageChain.Text(success.Value);
			}, error =>
			{
				messageChain.Text(error.Value);
			});
			await MessageSender.SendGroupMsg(message.GroupId, messageChain.Build());
		}
	}
}
