using MoBot.Core.Interfaces.MessageHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelManager.Handle
{
	public class InitialHandle : IInitializer
	{
		public IRootModel RootModel => new ModelManagerRootModel();

		public Task Initialize()
		{
			Core.LastInitialTime = DateTimeOffset.Now.ToUnixTimeSeconds();
			return Task.CompletedTask;
		}
	}
}
