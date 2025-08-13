using BilibiliLive.Constant;
using BilibiliLive.Manager;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using OneOf.Types;
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

		public string Description => "列出登录了的账户(所有人可用)";

		public string Icon => "./Assets/BilibiliLive/icon/UI_Icon_Paimon.png";

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsMsg("/账号列表"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			_ = Task.Run(async () =>
			{
				//等待是否需要重新生成图片
				try
				{
					await Task.Delay(2 * 1000, cancellationTokenSource.Token);
					await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Reply(message).Text("不存在缓存或有信息更新...请勾修金sama稍等...OvO").Build());
				}
				catch (OperationCanceledException)
				{
				}
			});
			var messageChain = MessageChainBuilder.Create().Reply(message);
			var result = await AccountManager.ShowUserList(message.UserId);
			result.Switch(success =>
			{
				cancellationTokenSource.Cancel();
				messageChain.Text("(●• ̀ω•́ )✧末酱为勾修金sama找到了的用户").Image($"base64://{success.Value}");
			}, error =>
			{
				cancellationTokenSource.Cancel();
				messageChain.Text(error.Value);
			});
			await MessageSender.SendGroupMsg(message.GroupId, messageChain.Build());
		}
	}
}
