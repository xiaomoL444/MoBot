using MoBot.Core.Interfaces.MessageHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive
{
	public class BilibiliRootModel : IRootModel
	{
		public string Name => "B站直播功能";

		public string Description => "B站直播功能";

		public string Icon => "./Asserts/BilibiliLive/icon/icon.png";
	}
}
