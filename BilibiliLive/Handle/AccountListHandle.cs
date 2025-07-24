using BilibiliLive.Constant;
using BilibiliLive.Interaction;
using BilibiliLive.Models;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BilibiliLive.Models.AccountConfig;
using HttpClient = BilibiliLive.Tool.HttpClient;

namespace BilibiliLive.Handle
{
	public class AccountListHandle : IMessageHandle<Group>
	{
		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;

		public AccountListHandle(ILogger<StreamHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.GroupId == Constants.OPGroupID && message.Sender.UserId == Constants.OPAdmin)
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			List<string> commnad = message.SplitMsg(" ");

			switch (commnad[0])
			{
				case "/账号列表":
					await ShowTaskList(message);
					break;
				case "/删除用户":
					await DeleteUser(message);
					break;
				default:
					break;
			}
			return;
		}

		public Task Initial()
		{
			return Task.CompletedTask;
		}
		async Task DeleteUser(Group group)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			try
			{
				var args = group.SplitMsg();
				int index = int.Parse(args[1]);
				accountConfig.Users.RemoveAt(index);
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text($"成功帮勾修金sama移除第[{index}]个用户(骄傲)").Build());
				_dataStorage.Save(Constants.AccountFile, accountConfig);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "删除用户失败");
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text($"移除用户失败惹(｡•́︿•̀｡) ").Build());
			}


		}

		async Task ShowTaskList(Group group)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);

			var msgChain = MessageChainBuilder.Create().Text("末酱为勾修金sama找到了的用户\n");
			for (int i = 0; i < accountConfig.Users.Count; i++)
			{
				var userCredential = accountConfig.Users[i].UserCredential;
				var userInfo = await UserInteraction.GetUserInfo(userCredential);

				try
				{
					msgChain.Text($"[{i}][{userInfo.Data.Name}]:{(userInfo.Data.LiveRoom.RoomStatus == 0 ? "未开通直播间" : "已开通直播间")}");
					msgChain.Text("\n");
				}
				catch (Exception ex)
				{
					msgChain.Text("获取失败");
				}

			}
			await MessageSender.SendGroupMsg(group.GroupId, msgChain.Build());
		}
	}
}
