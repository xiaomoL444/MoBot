using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Interfaces.MessageHandle
{
	/// <summary>
	/// 每个模块的初始化
	/// </summary>
	public interface IInitializer
	{
		IRootModel RootModel { get; }

		/// <summary>
		/// 初始化的函数
		/// </summary>
		Task Initialize();
	}
}
