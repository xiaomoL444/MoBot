using MoBot.Core.Models.Message;
using MoBot.Core.Models.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Handle.Message
{
	public static partial class MessageSender
	{
		/// <summary>
		/// 发送群组消息
		/// </summary>
		/// <param name="group_id">群ID</param>
		/// <param name="message_chain">消息链</param>
		/// <returns>发送后返回的值</returns>
		public static async Task<ActionPacketRsp> SendGroupMsg(long group_id, List<MessageSegment> message_chain)
		{
			return await SocketClient!.SendMessage("send_group_msg", ActionType.Post, new
			{
				group_id,
				message = message_chain
			});

		}
	}
}
