using MoBot.Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Interfaces
{
	public interface IMoBotClient
	{
		public Task RouteAsync(EventPacketBase eventPacket);
	}
}
