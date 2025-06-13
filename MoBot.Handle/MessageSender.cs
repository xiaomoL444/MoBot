using MoBot.Core.Interfaces;
using MoBot.Core.Models.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle
{
	public static class MessageSender
	{
		public static IBotSocketClient? SocketClient;

		/// <summary>
		/// 发送群组消息
		/// </summary>
		/// <param name="group_id">群ID</param>
		/// <param name="message_chain">消息链</param>
		/// <returns>发送后返回的值</returns>
		public static async Task<JObject> SendGroupMsg(long group_id, List<MessageSegment> message_chain)
		{
			return new JObject();
		}
	}
}
