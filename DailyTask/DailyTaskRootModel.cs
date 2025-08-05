using MoBot.Core.Interfaces.MessageHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTask
{
	internal class DailyTaskRootModel : IRootModel
	{
		public string Name => "每日定时任务";

		public string Description =>"目前只实现了每日夸夸和每日古文";

		public string Icon => "./Asserts/DailyTask/icon/rootModelIcon.png";
	}
}
