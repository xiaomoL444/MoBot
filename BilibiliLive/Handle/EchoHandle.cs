
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Net;
using Newtonsoft.Json.Linq;

namespace BilibiliLive.Handle
{
	public class EchoHandle : IMessageHandle
	{
		public Task<bool> CanHandleAsync(EventPacket message)
		{
			return Task.FromResult(true);
		}

		public Task HandleAsync(EventPacket message)
		{
			return Task.CompletedTask;
		}
	}
}
