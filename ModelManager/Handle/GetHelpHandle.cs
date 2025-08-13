using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Interfaces.MessageHandle;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using MoBot.Infra.PuppeteerSharp.Interface;
using MoBot.Infra.PuppeteerSharp.Interfaces;
using ModelManager.Constant;
using ModelManager.Models.WebShot;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModelManager.Handle
{
	public class GetHelpHandle : IMessageHandle<Group>
	{
		public IRootModel RootModel => new ModelManagerRootModel();

		public string Name => "/帮助";

		public string Description => "获取帮助(所有人可用)";

		public string Icon => "./Assets/ModelManager/icon/UI_QuestMain_Finish.png";

		private readonly IServiceProvider _services;
		private readonly ILogger<GetHelpHandle> _logger;
		private readonly IDataStorage _dataStorage;
		private readonly IWebshot _webshot;
		private readonly IWebshotRequestStore _webshotRequestStore;


		public GetHelpHandle(
			IServiceProvider services,
			ILogger<GetHelpHandle> logger,
			IDataStorage dataStorage,
			IWebshot webshot,
			IWebshotRequestStore webshotRequestStore)
		{
			_services = services;
			_logger = logger;
			_dataStorage = dataStorage;
			_webshot = webshot;
			_webshotRequestStore = webshotRequestStore;
		}

		public Task<bool> CanHandleAsync(Group message)
		{
			if (message.IsGroupID(Constants.OPGroupID) && message.IsUserID(Constants.OPAdmin) && message.IsMsg("/帮助"))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}

		public async Task HandleAsync(Group message)
		{
			_logger.LogDebug("获取模块集合中...");

			var helpList = new HelpList();
			foreach (var service in _services.GetServices<IMessageHandle<Group>>())
			{
				var root = service.RootModel;
				if (!helpList.ModelInfos.Any(q => q.Name == root.Name))
				{
					//不存在根，则添加进去
					helpList.ModelInfos.Add(new() { Name = root.Name, Description = root.Description, Icon = root.Icon });
				}
				helpList.ModelInfos.FirstOrDefault(q => q.Name == root.Name).PluginInfos.Add(new() { Name = service.Name, Description = service.Description, Icon = service.Icon });
				helpList.ImageCount++;
			}
			//计算MD5值，查看缓存中是否有该图片，若MD5不同，则为插件有更新，创建新图片

			var input = JsonConvert.SerializeObject(helpList);
			string md5_result = string.Empty;
			using (var md5 = MD5.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// 把字节数组转换成十六进制字符串
				StringBuilder sb = new StringBuilder();
				foreach (var b in hashBytes)
					sb.Append(b.ToString("x2"));  // 小写16进制

				md5_result = sb.ToString();
			}

			_logger.LogDebug("List:{@input},md5:{md5}", input, md5_result);

#if DEBUG
			bool debug = true;
#else
			bool debug = false;
#endif

			var path = $"{_dataStorage.GetDirectory(MoBot.Core.Models.DirectoryType.Cache)}/{md5_result}.png";
			if (!File.Exists(path) || debug)
			{
				_logger.LogDebug("不存在图片，创建图片");
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Reply(message).Text("插件信息有更新或是暂无缓存，请等待生成...").Build());
				//给每个icon创建http连接
				foreach (var model in helpList.ModelInfos)
				{
					var modelUuid = Guid.NewGuid().ToString();
					_webshotRequestStore.SetNewContent(modelUuid, MoBot.Infra.PuppeteerSharp.Models.HttpServerContentType.ImagePng, File.ReadAllBytes(model.Icon));
					model.Icon = $"{_webshotRequestStore.GetIPAddress()}?id={modelUuid}";

					foreach (var plugin in model.PluginInfos)
					{
						var pluginUuid = Guid.NewGuid().ToString();
						_webshotRequestStore.SetNewContent(pluginUuid, MoBot.Infra.PuppeteerSharp.Models.HttpServerContentType.ImagePng, File.ReadAllBytes(plugin.Icon));
						plugin.Icon = $"{_webshotRequestStore.GetIPAddress()}?id={pluginUuid}";
					}
				}
				_logger.LogDebug("Http_List:{@input}", helpList);

				var uuid = Guid.NewGuid().ToString();
				_webshotRequestStore.SetNewContent(uuid, MoBot.Infra.PuppeteerSharp.Models.HttpServerContentType.TextPlain, JsonConvert.SerializeObject(helpList));

				var base64 = await _webshot.ScreenShot($"{_webshot.GetIPAddress()}/HelpList?id={uuid}", screenshotOptions: new() { FullPage = true });
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Reply(message).Image($"base64://{base64}").Build());

				byte[] imageBytes = Convert.FromBase64String(base64);
				File.WriteAllBytes(path, imageBytes);
			}
			else
			{
				_logger.LogDebug("{path}存在，直接发送图片", path);
				await MessageSender.SendGroupMsg(message.GroupId, MessageChainBuilder.Create().Reply(message).Image($"base64://{Convert.ToBase64String(File.ReadAllBytes(path))}").Build());
			}

		}
	}
}
