using MoBot.Core.Interfaces.MessageHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyChat
{
	public class DailyChatRootModel : IRootModel
	{
		public string Name => "关键词回复";

		public string Description => "这是一个关键词回复功能";

		public string Icon => "./Asserts/DailyChat/icon/UI_TempEmoticon.png";
	}
}
