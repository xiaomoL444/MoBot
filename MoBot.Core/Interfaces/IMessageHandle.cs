using MoBot.Core.Models.Event;
using MoBot.Core.Models.Net;

namespace MoBot.Core.Interfaces
{
	public interface IMessageHandle<T> where T : EventPacketBase
	{
		/// <summary>
		/// 初始化程序，没什么好传的，也没什么好返回的吧？自己处理初始化错误好了
		/// </summary>
		/// <returns></returns>
		Task Initial();

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
