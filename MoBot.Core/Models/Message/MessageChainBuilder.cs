using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoBot.Core.Models.Message
{
	public class MessageChainBuilder
	{
		private List<MessageSegment> _msgSegment = [];

		public static MessageChainBuilder Create() => new();


	}
}
