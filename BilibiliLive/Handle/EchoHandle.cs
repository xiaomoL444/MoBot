
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Net;
using Newtonsoft.Json.Linq;
using MoBot.Core.Models.Event.Message;

namespace BilibiliLive.Handle
{
	public class EchoHandle : IMessageHandle<Group>
	{
		public Task<bool> CanHandleAsync(Group message)
		{
			throw new NotImplementedException();
		}

		public Task HandleAsync(Group message)
		{
			throw new NotImplementedException();
		}
	}
}
