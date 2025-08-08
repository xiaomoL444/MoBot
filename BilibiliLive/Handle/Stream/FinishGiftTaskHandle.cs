using BilibiliLive.Constant;
using BilibiliLive.Manager;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Handle.Stream
{
	public class FinishGiftTaskHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new BilibiliRootModel();

		public string Name => "/投喂任务";

		public string Description => "完成投喂任务";

		public string Icon => "./Assets/BilibiliLive/icon/live.png";

		private readonly ILogger<FinishGiftTaskHandle> _logger;

		public FinishGiftTaskHandle(
			ILogger<FinishGiftTaskHandle> logger)
		{
			_logger = logger;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.IsMsg("/投喂任务"))
				return Task.FromResult(true);
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			CancellationTokenSource cancellationTokenSource = new();
			_ = Task.Run(async () =>
			{
				try
				{
					await Task.Delay(1000, cancellationTokenSource.Token);
					await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text("请稍等...预计需要一分钟左右时间...").Build());
				}
				catch (OperationCanceledException ex)
				{
					_logger.LogDebug("等待任务取消");
				}
			});
			var result = await LiveManager.FinishGiftTask();
			await result.Match(async success =>
			{
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text("投喂礼物结果").Image($"base64://{success.Value}").Build());
			}, async error =>
			{
				cancellationTokenSource.Cancel();
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Text(error.Value).Build());
			});
		}
	}
}
