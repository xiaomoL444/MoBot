using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle
{
	public class MoBotClient
	{
		private readonly ILogger<MoBotClient> _logger;
		private readonly IBotSocketClient _socketClient;
		private readonly IEnumerable<IMessageHandle> _messageHandle;
		public MoBotClient(
			ILogger<MoBotClient> logger,
			IBotSocketClient socketClient,
			IEnumerable<IMessageHandle> messageHandle
			)
		{
			_logger = logger;
			_socketClient = socketClient;
			_messageHandle = messageHandle;
		}

		public void Initial()
		{
			_socketClient.Initial();
			_socketClient.ReceiveMsgAction = RouteAsync;
		}

		public async Task RouteAsync(string message)
		{
			try
			{
				JObject json = JObject.Parse(message);
				foreach (var handler in _messageHandle)
				{
					await handler.HandleAsync(json);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"消息序列化错误 {ex}");
			}
		}
	}
}
