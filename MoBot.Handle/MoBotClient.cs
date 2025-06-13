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
		private readonly IBotSocketClient _socketClient;
		private readonly IEnumerable<IMessageHandle> _messageHandle;
		public MoBotClient(
			IBotSocketClient socketClient,
			IEnumerable<IMessageHandle> messageHandle
			)
		{
			_socketClient = socketClient;
			_messageHandle = messageHandle;
		}

		public void Initial()
		{
			_socketClient.Initial();
			_socketClient.ReceiveMsgAction += RouteAsync;
			MessageSender.SocketClient = _socketClient;
		}

		public async Task RouteAsync(string message)
		{
			try
			{
				JObject json = JObject.Parse(message);
				foreach (var handler in _messageHandle)
				{
					if (handler.CanHandleAsync(json).Result)
					{
						await handler.HandleAsync(json);
					}
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Warning($"消息序列化错误 {ex}");
			}
		}
	}
}