using MoBot.Core.Models.Event;
using MoBot.Core.Models.Net;

namespace MoBot.Core.Interfaces.MessageHandle
{
	public interface IMessageHandle<T> where T : EventPacketBase
	{
		/// <summary>
		/// Handle所属的根模块
		/// </summary>
		IRootModel RootModel { get; }

		/// <summary>
		/// Handle名
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Handle的描述
		/// </summary>
		string Description { get; }

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
