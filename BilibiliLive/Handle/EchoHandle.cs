
using MoBot.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace BilibiliLive.Handle
{
	public class EchoHandle : IMessageHandle
	{
		public Task<bool> CanHandleAsync(JObject message)
		{
			return Task.FromResult(true);	
		}

		public Task HandleAsync(JObject message)
		{

		}
	}
}
