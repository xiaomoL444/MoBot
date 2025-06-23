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
		/// 获取Bot的登录信息
		/// </summary>
		/// <returns>发送后返回的值</returns>
		public static async Task<ActionPacketRsp> GetLoginInfo()
		{
			return await SocketClient!.SendMessage("get_login_info", ActionType.Get, new());
		}
	}
}
