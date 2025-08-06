using MoBot.Core.Models.Net;
using Newtonsoft.Json.Linq;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Core.Models.Event;
using MoBot.Handle.Message;
using MoBot.Core.Interfaces.MessageHandle;

namespace BilibiliLive.Handle
{
	public class EchoHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "复活吧我的爱人";

		public string Description => "Ciallo~";

		public string Icon => "./Assets/BilibiliLive/icon/ciallo.jpg";

		public Task Initial()
		{
			return Task.CompletedTask;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.GroupId == 1079464803 && message.Sender.UserId == 2580139692 && message.RawMessage == "复活吧我的爱人")
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}


		public async Task HandleAsync(Group message)
		{
			var messageChain = MessageChainBuilder
				.Create()
				.Text("⎛⎝≥⏝⏝≤⎛⎝")
				.Build();


			await MessageSender.SendGroupMsg(message.GroupId, messageChain);
		}

	}
}
