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
		private readonly IServiceProvider _provider;
		private readonly ILogger _logger;
		public MoBotClient(
			IServiceProvider provider,
			ILogger<MoBotClient> logger
			)
		{
			_provider = provider;
			_logger = logger;
		}

		public void Initial()
		{
			var client = _provider.GetService<IBotSocketClient>();
			client.Initial();
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
				_logger.LogWarning($"消息序列化错误 {ex}");
			}
		}

		public async Task RouteAsync(EventPacketBase eventPacket)
		{
			await RouteAsyncPri(eventPacket);
		}
	}
}