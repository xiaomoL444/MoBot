using DailyChat.Constant;
using DailyChat.Models;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<EchoHandle> _logger;
		private readonly IDataStorage _dataStorage;

		public EchoHandle(
			ILogger<EchoHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

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

			var EchoRules = _dataStorage.Load<EchoRule>("EchoRules");


			return Task.CompletedTask;
		}
	}
}
