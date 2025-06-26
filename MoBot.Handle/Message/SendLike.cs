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
		/// 点赞好友
		/// </summary>
		/// <param name="userID">用户ID</param>
		/// <param name="time">点赞的次数</param>
		public static async Task<ActionPacketRsp> SendLike(long userID, int times)
		{
			return await SocketClient!.SendMessage("send_like", ActionType.Post, new
			{
				user_id = userID,
				times
			});
		}
	}
}
