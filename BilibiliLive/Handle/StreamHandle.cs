using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Handle.Extensions;

namespace BilibiliLive.Handle
{
	/// <summary>
	/// 控制串流的小家伙
	/// </summary>
	public class StreamHandle : IMessageHandle<Group>
	{
		private readonly long _opGroupID = Constant.Constant.OPGroupID;
		private readonly long _opAdmin = Constant.Constant.OPAdmin;

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;

		private string streamArgs = "";

		public StreamHandle(
			ILogger<StreamHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && message.IsMsg("/开始推流")) return Task.FromResult(true);
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && message.IsMsg("/关闭推流")) return Task.FromResult(true);

			return Task.FromResult(false);
		}

		public Task HandleAsync(Group message)
		{
			var commonds = message.SplitMsg();
			switch (commonds[0])
			{
				case "/开始推流":
					StartStream();
					break;
				case "/关闭推流":
					StopStream();
					break;
			}
			return Task.CompletedTask;
		}

		void StartStream()
		{
			var args = "";


		}
		void StopStream()
		{
		}
	}
}
