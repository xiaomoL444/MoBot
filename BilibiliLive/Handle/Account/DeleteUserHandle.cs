using BilibiliLive.Constant;
using BilibiliLive.Manager;
using BilibiliLive.Models.Config;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle.Account
{
	public class DeleteUserHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/删除用户 <用户序号>";

		public string Description => "删除用户";

		public string Icon => "./Assets/BilibiliLive/icon/bilibiliTelevision.png";

		private readonly IDataStorage _dataStorage;

		public DeleteUserHandle(IDataStorage dataStorage)
		{
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.SplitMsg(" ")[0] == "/删除用户")
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var args = message.SplitMsg(" ");
			var messageChain = MessageChainBuilder.Create().Reply(message);
			switch (args.Count)
			{
				case 1:
					messageChain.Text("需要提供删除的用户序号哦");
					break;
				case 2:
					if (int.TryParse(args[1], out int index))
					{
						if (index <= accountConfig.Users.Count - 1)
						{
							var result = await AccountManager.DeleteUser(accountConfig.Users[index].Uid);
							result.Switch(success =>
							{
								messageChain.Text(success.Value);
							}, error =>
							{
								messageChain.Text(error.Value);
							});
							break;
						}
						messageChain.Text("序号不正确哦，请重新来一次吧").Build();
						break;
					}
					messageChain.Text("输入的不是数字哦，请重新来一次吧");
					break;
				default:
					messageChain.Text("参数数量有误呢");
					break;
			}
			await MessageSender.SendGroupMsg(message.GroupId, messageChain.Build());
		}
	}
}
