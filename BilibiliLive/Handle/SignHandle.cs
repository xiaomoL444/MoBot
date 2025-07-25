﻿using BilibiliLive.Constant;
using BilibiliLive.Models;
using BilibiliLive.Models.config;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using QRCoder;
using System.Collections.Specialized;
using System.Web;
using static QRCoder.PayloadGenerator;
using HttpClient = BilibiliLive.Tool.HttpClient;

namespace BilibiliLive.Handle
{
	public class SignHandle : IMessageHandle<Group>
	{
		private readonly long _opGroupID = Constant.Constants.OPGroupID;
		private readonly long _opAdmin = Constant.Constants.OPAdmin;

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;


		public SignHandle(
			ILogger<StreamHandle> logger,
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
			if (message.IsGroupID(_opGroupID) && message.IsUserID(_opAdmin) && (message.IsMsg("/登录"))) return Task.FromResult(true);

			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group group)
		{
			Sign(group);

			return;
		}

		async void Sign(Group group)
		{
			//获得QRcode 的url并转化为图像
			var QRcodeGen = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, Constants.BilibiliWebSignQRcodeGenerataApi));
			_logger.LogDebug("扫码登录生成QRcode:{@code_url}", (await QRcodeGen.Content.ReadAsStringAsync()));
			try
			{
				var QRcodeGenResult = JsonConvert.DeserializeObject<WebQRcodeGenerateRsp>(await QRcodeGen.Content.ReadAsStringAsync());

				QRCodeGenerator qrGenerator = new QRCodeGenerator();
				QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRcodeGenResult.Data.Url!, QRCodeGenerator.ECCLevel.Q);
				Base64QRCode qrCode = new Base64QRCode(qrCodeData);
				string qrCodeImageAsBase64 = qrCode.GetGraphic(20);
				AsciiQRCode qrCodeAscii = new AsciiQRCode(qrCodeData);
				string qrCodeAsAsciiArt = qrCodeAscii.GetGraphic(1);
				_logger.LogInformation($"\n{qrCodeAsAsciiArt}");

				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("勾修金sama~，请登录~(＾ω＾)").Image($"base64://{qrCodeImageAsBase64}").Build());

				//启动线程，等待连接成功
				await Task.Run(() => { WaitForScan(group, QRcodeGenResult.Data.QrcodeKey); });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取QRcode链接失败");
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("获取二维码失败了哦(｡•́︿•̀｡)").Build());
				throw;
			}
		}
		async void WaitForScan(Group group, string qrcode_key)
		{
			try
			{
				bool isPolling = true;
				int pollingCount = 0;
				int pollingMaxCount = 150;
				while (isPolling)
				{
					if (pollingCount >= pollingMaxCount)
					{
						isPolling = false;
						break;
					}
					pollingCount++;
					var QRcodePoll = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{Constants.BilibiliWebSignQRcodePollApi}?qrcode_key={qrcode_key}"));
					_logger.LogDebug("{pollingCount} 轮询结果:{@poll_url}", pollingCount, (await QRcodePoll.Content.ReadAsStringAsync()));

					var QRcodePollResult = JsonConvert.DeserializeObject<WebQRcodePollRsp>(await QRcodePoll.Content.ReadAsStringAsync());
					switch (QRcodePollResult.Data.Code)
					{
						case 0:
							isPolling = false;

							string queryString = QRcodePollResult.Data.Url.Substring(QRcodePollResult.Data.Url.IndexOf('?') + 1);
							// 解析URL参数
							NameValueCollection cookies = HttpUtility.ParseQueryString(JsonConvert.DeserializeObject<string>($"\"{queryString}\"")!);
							var accountConfig = _dataStorage.Load<AccountConfig>("account");
							var userCredential = new UserCredential();
							userCredential.DedeUserID = cookies["DedeUserID"]!;
							userCredential.DedeUserID__ckMd5 = cookies["DedeUserID__ckMd5"]!;
							userCredential.Expires = cookies["Expires"]!;
							userCredential.Sessdata = cookies["SESSDATA"]!;
							userCredential.Bili_Jct = cookies["bili_jct"]!;

							//判断该用户是否已存在
							if (accountConfig.Users.Any(q => q.Uid == userCredential.DedeUserID))
							{
								_logger.LogWarning("已经有相同的用户登录过了{uid}", userCredential.DedeUserID);
								await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("勾修金sama已经有相同的账号登陆过了哦").Build());
								break;
							}
							accountConfig.Users.Add(InitailUser(userCredential));
							_dataStorage.Save("account", accountConfig);
							_logger.LogInformation("登录成功");
							await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("勾修金sama登录成功~ヾ(✿ﾟ▽ﾟ)ノ").Build());
							break;
						case 86038:
							isPolling = false;

							//TODO 撤回过期图片
							await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("二维码过期了哦，勾修金sama请再来一次吧|･ω･｀)").Build());
							_logger.LogWarning("二维码已失效");
							break;
						case 86090:
							_logger.LogInformation("二维码已扫描未确认");
							break;
						case 86101:
							_logger.LogInformation("未扫码");
							break;
						default:
							_logger.LogWarning("轮询遇见错误返回信息");
							break;
					}
					await Task.Delay(1000 * 150 / pollingMaxCount);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "轮询失败");
				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text("轮询...失败了?Σ( ° △ °|||)︴，请查看一下控制台吧").Build());
				throw;
			}
		}
		AccountConfig.User InitailUser(UserCredential userCredential)
		{
			return new()
			{
				UserCredential = userCredential,
				Uid = userCredential.DedeUserID
			};
		}
	}
}
