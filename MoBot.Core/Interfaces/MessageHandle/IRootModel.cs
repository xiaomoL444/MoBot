using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Interfaces.MessageHandle
{
	public interface IRootModel
	{
		/// <summary>
		/// 模块名字
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 模块描述
		/// </summary>
		string Description { get; }

		/// <summary>
		/// 图标，应是本地地址
		/// </summary>
		string Icon { get; }
	}
}
