using MoBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Interfaces
{
	public interface IDataStorage
	{
		/// <summary>
		/// 加载文件
		/// </summary>
		/// <typeparam name="T">读取的文件类型</typeparam>
		/// <param name="fileName">文件名</param>
		/// <param name="pluginName">插件名，默认为空，则是调用Load的函数所在的插件名作为config路径</param>
		/// <returns></returns>
		T Load<T>(string fileName, DirectoryType directoryType = DirectoryType.Config, string pluginName = "") where T : new();
		/// <summary>
		/// 保存文件
		/// </summary>
		/// <typeparam name="T">保存的文件类型</typeparam>
		/// <param name="fileName">文件名</param>
		/// <param name="data">保存的内容</param>
		/// <param name="pluginName">插件名，默认为空，则是调用Save的函数所在的插件名作为config路径</param>
		void Save<T>(string fileName, T data, DirectoryType directoryType = DirectoryType.Config, string pluginName = "");

		/// <summary>
		/// 返回自己程序集所在类型的路径
		/// </summary>
		/// <param name="directoryType"></param>
		/// <param name="pluginName">插件名，默认为空，则是调用Save的函数所在的插件名作为config路径</param>
		/// <returns></returns>
		string GetDirectory(DirectoryType directoryType, string pluginName = "");
	}
}
