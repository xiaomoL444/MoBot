using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event;
using MoBot.Core.Models.Event.Message;
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
				var messageType = message.GetType();
				var handlerType = typeof(IMessageHandle<>).MakeGenericType(messageType);

				var handlers = (IEnumerable<object>)_provider.GetServices(handlerType);

				foreach (var handler in handlers)
				{
					// 调用 CanHandleAsync
					var canHandleMethod = handlerType.GetMethod("CanHandleAsync");
					var canHandleTask = (Task<bool>)canHandleMethod.Invoke(handler, new[] { message });
					bool canHandle = await canHandleTask;
					if (!canHandle) continue;

					// 调用 HandleAsync
					var handleMethod = handlerType.GetMethod("HandleAsync");
					var handleTask = (Task)handleMethod.Invoke(handler, new[] { message });
					await handleTask;
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