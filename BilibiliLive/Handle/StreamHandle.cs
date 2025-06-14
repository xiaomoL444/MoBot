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

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && message.IsMsg("/开始推流")) return Task.FromResult(true);
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && message.IsMsg("/关闭推流")) return Task.FromResult(true);

			return Task.FromResult(false);
		}

		public Task HandleAsync(Group message)
		{

		}
	}
}
