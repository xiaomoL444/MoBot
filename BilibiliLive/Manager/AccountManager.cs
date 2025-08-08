using BilibiliLive.Constant;
using BilibiliLive.Interaction;
using BilibiliLive.Manager.Era;
using BilibiliLive.Models;
using BilibiliLive.Models.Config;
using BilibiliLive.Models.Webshot;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using MoBot.Infra.PuppeteerSharp.Interface;
using MoBot.Infra.PuppeteerSharp.Interfaces;
using MoBot.Infra.PuppeteerSharp.Models;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BilibiliLive.Manager
{
	public static class AccountManager
	{
		private static readonly ILogger _logger = GlobalSetting.CreateLogger(typeof(AccountManager));
		private static readonly IDataStorage _dataStorage = GlobalSetting.DataStorage;
		private static IWebshot _webshot = GlobalSetting.Webshot;
		private static IWebshotRequestStore _webshotRequestStore = GlobalSetting.WebshotRequestStore;

		public static async Task Sign(Action<List<MessageSegment>> sendMessage)
		{
			//获得QRcode 的url并转化为图像
			var QRcodeGen = await Tool.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, Constants.BilibiliWebSignQRcodeGenerataApi));
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

				sendMessage(MessageChainBuilder.Create().Text("勾修金sama~，请登录~(＾ω＾)").Image($"base64://{qrCodeImageAsBase64}").Build());

				//启动线程，等待连接成功
				await Task.Run(() => { WaitForScan(sendMessage, QRcodeGenResult.Data.QrcodeKey); });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "获取QRcode链接失败");
				sendMessage(MessageChainBuilder.Create().Text("获取二维码失败了哦(｡•́︿•̀｡)").Build());
				throw;
			}
		}
		static async void WaitForScan(Action<List<MessageSegment>> sendMessage, string qrcode_key)
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
					var QRcodePoll = await Tool.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{Constants.BilibiliWebSignQRcodePollApi}?qrcode_key={qrcode_key}"));
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
								sendMessage(MessageChainBuilder.Create().Text("勾修金sama已经有相同的账号登陆过了哦").Build());
								break;
							}
							accountConfig.Users.Add(InitailUser(userCredential));
							_dataStorage.Save("account", accountConfig);
							_logger.LogInformation("登录成功");
							sendMessage(MessageChainBuilder.Create().Text("勾修金sama登录成功~ヾ(✿ﾟ▽ﾟ)ノ").Build());
							break;
						case 86038:
							isPolling = false;

							//TODO 撤回过期图片
							sendMessage(MessageChainBuilder.Create().Text("二维码过期了哦，勾修金sama请再来一次吧|･ω･｀)").Build());
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
				sendMessage(MessageChainBuilder.Create().Text("轮询...失败了?Σ( ° △ °|||)︴，请查看一下控制台吧").Build());
				throw;
			}
		}
		static AccountConfig.User InitailUser(UserCredential userCredential)
		{
			return new()
			{
				UserCredential = userCredential,
				Uid = userCredential.DedeUserID
			};
		}

		public static async Task<OneOf<Success<string>, Error<string>>> ShowUserList()
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);

			//var msgChain = MessageChainBuilder.Create().Text("末酱为勾修金sama找到了的用户\n");
			Dictionary<string, UserInfoRsp> userInfoCache = (await Task.WhenAll(accountConfig.Users.Select(async q => new { uid = q.Uid, userinfo = await UserInteraction.GetUserInfo(q.UserCredential) }))).ToDictionary(x => x.uid, x => x.userinfo);

			//string insertImgInfo(string uid, string info) => $"<img src='data:image/png;base64,{Convert.ToBase64String(Tool.HttpClient.SendAsync(new(HttpMethod.Get, userInfoCache[uid].Data.Face)).Result.Content.ReadAsByteArrayAsync().Result)}' style='padding-left:4vw; vertical-align: middle; width: 3vw;'/><span style='vertical-align: middle;'>{info}</span>";
			string insertImgInfo(string uid, string info) => $"<img src='{userInfoCache[uid].Data.Face}' @load='OnImageLoad' style='padding-left:4vw; vertical-align: middle; width: 3vw;'/><span style='vertical-align: middle;'>{info}</span>";

			AccountList accountList = new() { ImageCount = accountConfig.Users.Count + 1 };
			foreach (var user in accountConfig.Users)
			{
				var userCredential = user.UserCredential;
				var userInfo = userInfoCache[user.Uid];
				var roomInfo = await UserInteraction.GetUserRoomInfo(user.Uid);
				try
				{
					accountList.AccountInfos.Add(new()
					{
						Name = userInfo.Data.Name,
						Icon = userInfo.Data.Face,
						Info = $@"{(roomInfo.Data is { RoomStatus: 0 } ? "未开通直播间" : "已开通直播间")}
{string.Join("\n", user.LiveDatas.Select(livedata => $@"{livedata.LiveArea}:
  ♪ 看播：
{string.Join("\n", livedata.ViewLiveUsers.Select(viewUser => insertImgInfo(viewUser, userInfoCache[viewUser].Data.Name)))}
  ♪ 发送弹幕：
{string.Join("\n", livedata.SendUserDanmuku.Select(danmukuUser => insertImgInfo(danmukuUser.Key, userInfoCache[danmukuUser.Key].Data.Name + $" x{danmukuUser.Value}")))}
  ♪ 投喂礼物：
{string.Join("\n", livedata.GiftUsers.Select(giftUser => insertImgInfo(giftUser.Key, userInfoCache[giftUser.Key].Data.Name + $" x{giftUser.Value}")))}"))}"
					});
				}
				catch (Exception ex)
				{
					accountList.AccountInfos.Add(new() { Name = user.Uid, Icon = MoBot.Infra.PuppeteerSharp.Constant.Constants.WhiteTransParentBase64, Info = "获取失败" });
					accountList.ImageCount--;
				}
			}

			//截图
			string uuid = Guid.NewGuid().ToString();
			_webshotRequestStore.SetNewContent(uuid, HttpServerContentType.TextPlain, JsonConvert.SerializeObject(accountList));

			var base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/AccountList?id={uuid}", screenshotOptions: new() { FullPage = true });
			return new Success<string>(base64);
		}

		public static async Task DeleteUser(Action<List<MessageSegment>> sendMessage, List<string> args)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			try
			{
				int index = int.Parse(args[0]);
				accountConfig.Users.RemoveAt(index);
				sendMessage(MessageChainBuilder.Create().Text($"成功帮勾修金sama移除第[{index}]个用户(骄傲)").Build());
				_dataStorage.Save(Constants.AccountFile, accountConfig);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "删除用户失败");
				sendMessage(MessageChainBuilder.Create().Text($"移除用户失败惹(｡•́︿•̀｡) ").Build());
			}


		}
	}
}
