using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Handle.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle
{
	public class SignHandle : IMessageHandle<Group>
	{
		private readonly long _opGroupID = Constant.Constant.OPGroupID;
		private readonly long _opAdmin = Constant.Constant.OPAdmin;

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;


		public SignHandle(
			ILogger<StreamHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && (message.IsMsg("/登录"))) return Task.FromResult(true);

			return Task.FromResult(false);
		}

		public Task HandleAsync(Group message)
		{
			throw new NotImplementedException();
		}
	}
}
