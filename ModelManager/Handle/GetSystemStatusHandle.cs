using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using ModelManager.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelManager.Handle
{
	public class GetSystemStatusHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new ModelManagerRootModel();

		public string Name => "/运行状态";

		public string Description => "运行状态(所有人可用)";

		public string Icon => "./Assets/ModelManager/icon/UI_QuestMain_Finish.png";

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsMsg("/运行状态"))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			var messageChain = MessageChainBuilder.Create().Reply(message);
			messageChain.Text($"上一次开机时间：{DateTimeOffset.FromUnixTimeSeconds(Core.LastInitialTime).ToLocalTime().ToString("yyyy年MM月dd日 HH时mm分ss秒")}\n");
			TimeSpan diff = DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds(Core.LastInitialTime);
			messageChain.Text($"运行时长：{diff.Days}日,{diff.Hours}时,{diff.Minutes}分,{diff.Seconds}秒");
			await MessageSender.SendGroupMsg(message.GroupId, messageChain.Build());
		}
	}
}
