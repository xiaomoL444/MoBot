using DailyChat.Constant;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyChat
{
	public class EchoHandle : IMessageHandle<Group>
	{
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public Task HandleAsync(Group message)
		{
			var MsgChain = MessageChainBuilder.Create();



			return Task.CompletedTask;
		}
	}
}
