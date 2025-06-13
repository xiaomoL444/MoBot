using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event;
using MoBot.Core.Models.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle
{
	public class MoBotClient : IMoBotClient
	{
		private readonly IBotSocketClient _socketClient;
		private readonly IServiceProvider _provider;
		public MoBotClient(
			IBotSocketClient socketClient,
			IServiceProvider provider
			)
		{
			_socketClient = socketClient;
			_provider = provider;
		}

		public void Initial()
		{
			_socketClient.MoBotClient = this;
			_socketClient.Initial();
			MessageSender.SocketClient = _socketClient;
		}

		private async Task RouteAsyncPri<T>(T message) where T : EventPacketBase
		{
			try
			{
				foreach (var handler in _provider.GetServices<IMessageHandle<T>>())
				{
					if (handler.CanHandleAsync(message).Result)
					{
						await handler.HandleAsync(message);
					}
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Warning($"消息序列化错误 {ex}");
			}
		}

		public async Task RouteAsync(EventPacketBase eventPacket)
		{
			await RouteAsyncPri(eventPacket);
		}
	}
}