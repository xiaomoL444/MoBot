using MoBot.Core.Models.Event;
using MoBot.Core.Models.Net;

namespace MoBot.Core.Interfaces
{
	public interface IMessageHandle<T> where T : EventPacketBase
	{
		/// <summary>
		/// 判断是否执行这个模块
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task<bool> CanHandleAsync(T message);

		/// <summary>
		/// 模块的执行函数
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task HandleAsync(T message);
	}
}
