using Microsoft.Extensions.Logging;
using MoBot.Core.Models.Action;
using MoBot.Core.Models.Message;
using MoBot.Core.Models.Net;
using Newtonsoft.Json.Linq;

namespace MoBot.Core.Interfaces
{
	/// <summary>
	/// Bot的连接方式，可以是Http（没写），Websocket(正向)，也可以是控制台发送自定义Json测试等等
	/// </summary>
	public interface IBotSocketClient
	{
		/// <summary>
		/// 客户端初始化
		/// </summary>
		public void Initial();

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="action">接口名</param>
		/// <param name="actionType">接口的请求类别</param>
		/// <param name="message">发送的消息(自己构建一个匿名类型)</param>
		/// <returns>发送api后的返回值</returns>
		public Task<ActionPacketRsp> SendMessage(string action, ActionType actionType, object message);
	}
}
