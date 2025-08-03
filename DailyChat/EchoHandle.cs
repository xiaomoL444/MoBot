using DailyChat.Constant;
using DailyChat.Models;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Action;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Group = MoBot.Core.Models.Event.Message.Group;

namespace DailyChat
{
	public class EchoHandle : IMessageHandle<Group>
	{
		private readonly ILogger<EchoHandle> _logger;
		private readonly IDataStorage _dataStorage;

		public IRootModel RootModel =>new DailyChatRootModel();

		public string Name => "关键词回复功能";

		public string Description => "/抱抱、/贴贴、/晚安";

		public EchoHandle(
			ILogger<EchoHandle> logger,
			IDataStorage dataStorage
			)
		{
			_logger = logger;
			_dataStorage = dataStorage;
		}

		public Task Initial()
		{
			return Task.CompletedTask;
		}
		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group group)
		{
			var MsgChain = MessageChainBuilder.Create();

			var EchoRules = _dataStorage.Load<EchoRule>("EchoRules");

			//获得当次的回复
			//这里只是粗略的写string==，如果有必要以后可以用正则表达式 
			var selfIDMessage = string.Empty;
			try
			{
#if !DEBUG
				var QQData = ((JObject)(await MessageSender.GetLoginInfo()).Data).ToObject<GetLoginInfo>();
				selfIDMessage = $"[CQ:at,qq={QQData.UserId},name={QQData.Nickname}]";
#else
				selfIDMessage = $"[CQ:at,qq=3485806003,name=黑塔]";
#endif
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "获取bot登录信息失败");
			}


			var echoRule = EchoRules.ReplyItems.FirstOrDefault(q => q.Trigger.Any(s =>
			{
				string result = Regex.Replace(s, @"\[@selfID\]", selfIDMessage);
				if (group.RawMessage == result) return true;
				return false;
			}));

			if (echoRule == null) return;

			//白名单处理
			var whiteRule = echoRule.WhiteList.FirstOrDefault(q => group.IsUserID(q.UserID));
			if (whiteRule != null)
			{
				//组装消息
				var msg = whiteRule.message[Random.Shared.Next(0, whiteRule.message.Count)];
				_logger.LogInformation("触发{uid}的白名单回复，选择的消息是{@msg}", group.UserId, msg);

				//异步发送消息，因为每个消息间要有停顿
				_ = Task.Run(async () =>
				{
					try
					{
						foreach (var msgChain in msg.MessageChains)
						{
							var buildMessage = BuildMessage(msgChain.MessageItems);
							await MessageSender.SendGroupMsg(group.GroupId, buildMessage);
							await Task.Delay(Random.Shared.Next(500, 1500));
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "异步发送白名单消息失败");
					}
				});
				return;
			}
			var normalRule = echoRule.Normal.FirstOrDefault(q => q.UserID == 0);//获取uid为0的消息，若uid不为0，则不处理默认消息，要自己去json文件里面改，或者是uid为0里面没有消息，则理应跳过
			if (normalRule != null)
			{
				//组装消息
				var msg = normalRule.message[Random.Shared.Next(0, normalRule.message.Count)];
				_logger.LogInformation("触发{uid}的普通回复，选择的消息是{@msg}", group.UserId, msg);

				//异步发送消息，因为每个消息间要有停顿
				_ = Task.Run(async () =>
				{
					try
					{
						foreach (var msgChain in msg.MessageChains)
						{
							var buildMessage = BuildMessage(msgChain.MessageItems);
							await MessageSender.SendGroupMsg(group.GroupId, buildMessage);
							await Task.Delay(Random.Shared.Next(500, 1500));
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "异步发送普通消息失败");
					}
				});
				return;
			}
			return;
		}

		List<MessageSegment> BuildMessage(List<MessageItem> messages)
		{
			var msgChain = MessageChainBuilder.Create();
			var randonContent = _dataStorage.Load<EchoRule>("EchoRules");
			foreach (var message in messages)
			{
				//替换随机内容
				string result = Regex.Replace(message.content, @"\[RandomContent:(.*?)\]", match =>
				{
					string key = match.Groups[1].Value;
					if (randonContent.RandomContent.TryGetValue(key, out var output))
					{
						return output[Random.Shared.Next(0, output.Count)];
					}
					else
					{
						_logger.LogWarning("未能匹配到关键词");
						return $"[未知内容:{key}]"; // 如果没有匹配到
					}
				});

				switch (message.MessageItemType)
				{
					case MessageItemType.text:
						msgChain.Text(result);
						break;
					case MessageItemType.image:
						msgChain.Image(result, ImageType.Emoticon);
						break;
					default:
						break;
				}
			}
			return msgChain.Build();
		}
	}
}
