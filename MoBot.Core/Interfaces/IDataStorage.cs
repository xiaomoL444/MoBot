using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Interfaces
{
	public interface IDataStorage
	{
		T Load<T>(string fileName) where T : new();
		void Save<T>(string fileName, T data);
	}
}
