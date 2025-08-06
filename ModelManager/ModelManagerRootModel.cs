using MoBot.Core.Interfaces.MessageHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelManager
{
	public class ModelManagerRootModel : IRootModel
	{
		public string Name => "模块管理";

		public string Description => "模块管理";

		public string Icon => "./Assets/ModelManager/icon/rootModelIcon.png";
	}
}
