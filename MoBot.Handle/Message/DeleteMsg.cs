using MoBot.Core.Models.Event.Message;
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
		/// 获取Bot的登录信息
		/// </summary>
		/// <returns>发送后返回的值</returns>
		public static async Task<ActionPacketRsp> DeleteMsg(string msgID)
		{
			return await SocketClient!.SendMessage("delete_msg", ActionType.Post, new { message_id = msgID });
		}
		/// <summary>
		/// 获取Bot的登录信息
		/// </summary>
		/// <returns>发送后返回的值</returns>
		public static async Task<ActionPacketRsp> DeleteMsg(long msgID)
		{
			return await DeleteMsg(msgID.ToString());
		}
		/// <summary>
		/// 获取Bot的登录信息
		/// </summary>
		/// <returns>发送后返回的值</returns>
		public static async Task<ActionPacketRsp> DeleteMsg(MessageBase message)
		{
			return await DeleteMsg(message.MessageId);
		}
	}
}
