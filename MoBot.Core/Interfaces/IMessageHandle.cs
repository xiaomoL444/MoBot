using MoBot.Core.Models.Net;

namespace MoBot.Core.Interfaces
{
	public interface IMessageHandle
	{
		Task<bool> CanHandleAsync(EventPacket message);
		Task HandleAsync(EventPacket message);
	}
}
