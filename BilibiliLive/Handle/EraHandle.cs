using BilibiliLive.Constant;
using BilibiliLive.Models;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using MoBot.Core.Interfaces;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Models.Message;
using MoBot.Handle.Extensions;
using MoBot.Handle.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using HttpClient = BilibiliLive.Tool.HttpClient;

namespace BilibiliLive.Handle
{
	public class EraHandle : IMessageHandle<Group>
	{
		private List<string> commands = ["/更新激励计划", "/查询任务"];

		private readonly ILogger<StreamHandle> _logger;
		private readonly IDataStorage _dataStorage;


		public EraHandle(
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
			if (message.GroupId == 1079464803 && message.Sender.UserId == Constants.OPAdmin && commands.Contains(message.RawMessage))
			{
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}


		public async Task HandleAsync(Group message)
		{
			switch (message.RawMessage)
			{
				case "/更新激励计划":
					await RefreshEraData(message);
					break;
				case "/查询任务":
					await QueryTasks(message);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 刷新活动信息
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		async Task RefreshEraData(Group group)
		{
			var eraTaskConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);
			//获取原神激励计划的url
			using var getEraUrlResponse = await HttpClient.SendAsync(new(HttpMethod.Get, $"{Constants.GetEraUrl}?topic_id=3219"));
			getEraUrlResponse.EnsureSuccessStatusCode();
			var getEraUrlResponseString = await getEraUrlResponse.Content.ReadAsStringAsync();
			_logger.LogDebug("获取原神eraUrl信息{url}", getEraUrlResponseString);
			var eraUrl = ((string?)JObject.Parse(getEraUrlResponseString)["data"]["functional_card"]["traffic_card"]["jump_url"]);

			//下载html内容
			async Task<string> GetHtmlContent(string url)
			{
				using var eraUrlHtmlResponse = await HttpClient.SendAsync(new(HttpMethod.Get, url));
				eraUrlHtmlResponse.EnsureSuccessStatusCode();
				return await eraUrlHtmlResponse.Content.ReadAsStringAsync();
			}
			//获取原神激励计划页面的html
			var eraUrlHtmlContent = await GetHtmlContent(eraUrl);

			//获得h5链接的跳转连接
			var h5Url = "";
			var match = System.Text.RegularExpressions.Regex.Match(eraUrlHtmlContent, @"var\s+jumpUrl\s*=\s*'([^']+)'");
			if (match.Success)
			{
				h5Url = match.Groups[1].Value;
			}

			//获取原神激励计划页面的html
			var eraUrlH5HtmlContent = await GetHtmlContent(h5Url);

			//获得html内的json元素（）若抓取为空会报错
			string ExtractJsonAfter(string html, string marker)
			{
				int start = html.IndexOf(marker);
				if (start == -1) return null;

				int braceStart = html.IndexOf('{', start);
				if (braceStart == -1) return null;

				int braceCount = 0;
				bool inString = false;
				char lastChar = '\0';

				for (int i = braceStart; i < html.Length; i++)
				{
					char c = html[i];

					if (c == '"' && lastChar != '\\')
					{
						inString = !inString;
					}

					if (!inString)
					{
						if (c == '{') braceCount++;
						else if (c == '}') braceCount--;
					}

					if (braceCount == 0 && !inString)
					{
						return html.Substring(braceStart, i - braceStart + 1);
					}

					lastChar = c;
				}

				return null; // 如果沒匹配到完整的
			}
			var eraPageDataString = ExtractJsonAfter(eraUrlH5HtmlContent, "window.__BILIACT_EVAPAGEDATA__ = ");

			_logger.LogDebug("获取到的活动Data:{data}", eraPageDataString);

			//解析每个元件
			var eraPageElementData = JObject.Parse(eraPageDataString)["layerTree"][0]["slots"][0]["children"];
			var taskJsonPath = "$.slots[0].children[1].slots[0].children[0]";
			string liveTaskUrl = ((string?)eraPageElementData.FirstOrDefault(q => ((string?)q.SelectToken($"{taskJsonPath}.alias")) == "直播任务").SelectToken($"{taskJsonPath}.props.jumpAddress"));
			var viewLiveTaskUrl = ((string?)eraPageElementData.FirstOrDefault(q => ((string?)q.SelectToken($"{taskJsonPath}.alias")) == "看播任务").SelectToken($"{taskJsonPath}.props.jumpAddress"));
			var activityID = string.Empty;
			var activityIDMatch = System.Text.RegularExpressions.Regex.Match(eraUrlH5HtmlContent, @"var\s+activityPageEvaConfigActivityId\s*=\s*[""']([^""']+)[""']");
			if (activityIDMatch.Success)
			{
				activityID = activityIDMatch.Groups[1].Value;
			}
			eraTaskConfig.ActivityID = activityID;

			var eraTitleString = ExtractJsonAfter(eraUrlH5HtmlContent, "window.__BILIACT_PAGEINFO__ = ");
			var eraTitle = (string?)JObject.Parse(eraTitleString)["title"];
			eraTaskConfig.TaskTitle = eraTitle;
			_logger.LogInformation("获得的标题：{title}，活动id：{activity_id},直播任务url:{liveurl}，看播任务url:{viewurl}", eraTitle, activityID, liveTaskUrl, viewLiveTaskUrl);

			//处理直播任务url
			var liveUrlHtmlContent = await GetHtmlContent(liveTaskUrl);
			var liveInitailStateString = ExtractJsonAfter(liveUrlHtmlContent, "window.__initialState = ");
			var liveTaskElementsData = JObject.Parse(liveInitailStateString)["EvaTaskButton"];

			var liveTaskIDs = liveTaskElementsData.Select(s => ((string?)s["taskItem"]["taskId"])).Distinct().ToList();
			_logger.LogInformation("获取到的所有直播任务id：{@ids}", liveTaskIDs);
			eraTaskConfig.LiveTaskIDs = liveTaskIDs;

			//同样处理看播任务url
			var viewUrlHtmlContent = await GetHtmlContent(viewLiveTaskUrl);
			var viewInitailStateString = ExtractJsonAfter(viewUrlHtmlContent, "window.__initialState = ");
			var viewTaskElementsData = JObject.Parse(viewInitailStateString)["EvaTaskButton"];

			var viewTaskIDs = viewTaskElementsData.Select(s => ((string?)s["taskItem"]["taskId"])).Distinct().ToList();
			_logger.LogInformation("获取到的所有看播任务id：{@ids}", viewTaskIDs);
			eraTaskConfig.ViewTaskIDs = viewTaskIDs;

			_dataStorage.Save(Constants.EraFile, eraTaskConfig);

			await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Text($"[{eraTitle}]更新成功啦~").Build());

			return;
		}

		/// <summary>
		/// 获得任务信息
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		async Task QueryTasks(Group group)
		{
			var accountConfig = _dataStorage.Load<AccountConfig>(Constants.AccountFile);
			var eraConfig = _dataStorage.Load<EraTaskConfig>(Constants.EraFile);

			//获取任务

			foreach (var account in accountConfig.Users)
			{
				var userCredential = account.UserCredential;
				var userInfo = await BilibiliApiTool.GetUserInfo(userCredential);

				//直播任务
				var liveRes = await GetTaskInfo(userCredential, eraConfig.LiveTaskIDs);
				if (liveRes is not { Code: 0 })
				{
					_logger.LogError("获取{user}直播任务信息错误CODE:{code}", userCredential.DedeUserID, liveRes?.Code);
				}

				//看播任务
				var viewRes = await GetTaskInfo(userCredential, eraConfig.ViewTaskIDs);
				if (viewRes is not { Code: 0 })
				{
					_logger.LogError("获取{user}看播任务信息错误CODE:{code}", userCredential.DedeUserID, viewRes?.Code);
				}

				var startChar = " ♪ ";
				var text = $@"[{userInfo.Data.Name}]
新人任务：
{startChar}{liveRes.Data.List[0].TaskName}：{liveRes.Data.List[0].CheckPoints[0].AwardName}({liveRes.Data.List[0].TaskStatus})
每日任务：
{string.Join("\n", liveRes.Data.List.Skip(1).Take(4).Select(s => $"{startChar}{s.TaskName}：{s.CheckPoints[0].AwardName}({s.TaskStatus})"))}
完成“每日直播任务” ————完成天数[{liveRes.Data.List[5].AccumulativeCount}]
{string.Join("\n", liveRes.Data.List[5].AccumulativeCheckPoints.Select(s => $"{startChar}{s.AwardName}({s.Status})"))}";

				var image = await HttpClient.SendAsync(new(HttpMethod.Get, userInfo.Data.Face));

				var imageStream = await image.Content.ReadAsStreamAsync();
				//准备绘画
				var base64 = DrawImage("./Asserts/images/MyLover.png", text, imageStream);

				await MessageSender.SendGroupMsg(group.GroupId, MessageChainBuilder.Create().Image("base64://" + base64).Build());

				//File.Delete(filePath);
			}



			return;
		}

		//获取激励计划的任务
		async Task<TaskInfoRsp> GetTaskInfo(UserCredential userCredential, List<string> taskIds)
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, $"{Constants.GetEraTask}?task_ids={String.Join(",", taskIds)}");
			request.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata}");
			using var response = await HttpClient.SendAsync(request);
			var responseString = await response.Content.ReadAsStringAsync();
			_logger.LogDebug("获取{user}任务：{@taskID}的任务结果{Info}", userCredential.DedeUserID, taskIds, responseString);

			return JsonConvert.DeserializeObject<TaskInfoRsp>(responseString);
		}

		/// <summary>
		/// 绘制推片
		/// </summary>
		/// <param name="backgroud">背景图片</param>
		/// <param name="text">文字</param>
		/// <returns>base64</returns>
		string DrawImage(string backgroudPath, string text, Stream iconStream)
		{
			if (!File.Exists(backgroudPath))
			{
				_logger.LogError("图片[{path}]未找到", backgroudPath);
				return null;
			}

			int padding = 60;//内边距
			int cornerRadius = 40;//圆角半径
			List<string> fontPaths = ["./Asserts/fonts/Goth.ttf", "./Asserts/fonts/msyh.ttc", "./Asserts/fonts/SegUIVar.ttf"];
			int fontSize = 64;//字体大小
			int strokeWidth = 4;//描边宽度
			int iconDiameter = 250;//头像直径
			int innerHorizontalPadding = 50;//水平内边距
			int innerVerticalPadding = 60;//垂直内边距

			static List<string> WrapText(string text, SKFont font, SKPaint paint, float maxWidth)
			{
				var resultLines = new List<string>();
				var lines = text.Replace("\r\n", "\n").Split('\n'); // 支持 Windows 和 Unix 换行

				foreach (var line in lines)
				{
					string currentLine = string.Empty;
					foreach (char c in line)
					{
						string testLine = currentLine + c;
						if (font.MeasureText(testLine) <= maxWidth)
						{
							currentLine = testLine;
						}
						else
						{
							if (!string.IsNullOrEmpty(currentLine))
								resultLines.Add(currentLine);
							currentLine = c.ToString();
						}
					}
					if (!string.IsNullOrEmpty(currentLine))
						resultLines.Add(currentLine);
				}

				return resultLines;
			}

			//读取背景
			using var background = SKBitmap.Decode(backgroudPath);
			int imageWidth = background.Width;
			int imageHeight = background.Height;

			//创建画布
			using var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
			var canvas = surface.Canvas;
			canvas.Clear();
			canvas.DrawBitmap(background, 0, 0);

			var blurRect = new SKRect(
					padding,
					padding,
					imageWidth - padding,
					imageHeight - padding
				);

			//创建毛玻璃效果区域
			using var cropped = new SKBitmap((int)blurRect.Width, (int)blurRect.Height);
			using (var croppedCanvas = new SKCanvas(cropped))
			{
				croppedCanvas.DrawBitmap(background, blurRect, new SKRect(0, 0, blurRect.Width, blurRect.Height));
			}

			//创建模糊画笔
			var blurFilter = SKImageFilter.CreateBlur(10, 10); // 模糊程度
			using var blurPaint = new SKPaint
			{
				ImageFilter = blurFilter
			};

			//模糊
			var roundRect = new SKRoundRect(blurRect, cornerRadius, cornerRadius);
			canvas.Save();
			canvas.ClipRoundRect(roundRect, antialias: true);
			canvas.DrawBitmap(cropped, blurRect.Left, blurRect.Top, blurPaint);
			canvas.Restore();

			var overlayPaint = new SKPaint
			{
				Color = new SKColor(255, 255, 255, 100)
			};
			canvas.DrawRoundRect(roundRect, overlayPaint);

			//制作笔
			var fonts = fontPaths.Select(f => new SKFont(SKTypeface.FromFile(f), fontSize) { Edging = SKFontEdging.Antialias }).ToList();

			var textPaint = new SKPaint
			{
				IsAntialias = true,
				Color = SKColors.Black
			};
			var strokePaint = new SKPaint
			{
				IsStroke = true,        // 仅描边
				StrokeWidth = strokeWidth,
				Color = SKColor.Parse("fad9f4"),
				IsAntialias = true,
			};

			//贴上文字
			float textX = blurRect.Left + innerHorizontalPadding;
			float lineHeight = fonts[0].Spacing;
			var textLines = WrapText(text, fonts[0], textPaint, blurRect.Width - innerHorizontalPadding * 2);
			//计算垂直居中效果
			var metrics = fonts[0].Metrics;
			float textY = blurRect.MidY - ((metrics.Ascent + metrics.Descent) + (textLines.Count - 1) * lineHeight) / 2;
			foreach (var line in textLines)
			{
				bool HasGlyph(SKTypeface typeface, char c)
				{
					// SkiaSharp 没有直接接口判断字符是否存在
					// 但可通过测量字符对应的字形ID是否为0来判断
					try
					{
						ushort glyphId = typeface.GetGlyphs(new[] { c })[0];
						return glyphId != 0;
					}
					catch (Exception)
					{
						return false;
					}

				}
				float x = 0;
				foreach (char c in line)
				{
					// 判断主字体是否支持字符
					SKFont availableFont = fonts.FirstOrDefault(f => HasGlyph(f.Typeface, c)) ?? fonts[0];
					string s = c.ToString();
					// 测量字符宽度，用于计算绘制起点
					float charWidth = availableFont.MeasureText(s);

					// 绘制字符
					//canvas.DrawText(s, textX + x, textY, availableFont, strokePaint);
					canvas.DrawText(s, textX + x, textY, availableFont, textPaint);
					// 递增X坐标，准备绘制下一个字符
					x += charWidth;
				}

				textY += lineHeight;
			}

			canvas.Save(); // 保存主画布状态

			//添加头像
			using var icon = SKBitmap.Decode(iconStream);
			using var circlePath = new SKPath();
			var iconX = blurRect.Right - innerHorizontalPadding - iconDiameter / 2f;
			var iconY = blurRect.Top + innerHorizontalPadding + iconDiameter / 2f;
			circlePath.AddCircle(iconX, iconY, iconDiameter / 2f);
			canvas.ClipPath(circlePath, SKClipOperation.Intersect, true);

			// 计算缩放比
			var scale = Math.Min((float)iconDiameter / icon.Width, (float)iconDiameter / icon.Height);
			var scaledWidth = icon.Width * scale;
			var scaledHeight = icon.Height * scale;

			var destRectX = iconX - scaledWidth / 2;
			var destRectY = iconY - scaledHeight / 2;

			// 绘制缩放后的图片到圆形区域
			var destRect = new SKRect(destRectX, destRectY, destRectX + scaledWidth, destRectY + scaledHeight);
			canvas.DrawBitmap(icon, destRect);
			canvas.Restore();

			//添加生成时间
			var time = "Create By MoBot :" + DateTimeOffset.Now.ToString("u");
			canvas.DrawText(time, blurRect.Right - innerHorizontalPadding - fonts[0].MeasureText(time), blurRect.Bottom - innerVerticalPadding, fonts[0], textPaint);

			using var image = surface.Snapshot(); // 从画布截图
			using var data = image.Encode(SKEncodedImageFormat.Png, 100); // 编码为 PNG
			string base64 = Convert.ToBase64String(data.ToArray());

#if DEBUG
			File.WriteAllBytes("output.png", data.ToArray());
#endif

			return base64;
		}

		async Task ReceiveAward(UserCredential userCredential, string taskID, string activityID, string activityName, string taskName, string rewaredName, int duration = 60)
		{
			Dictionary<string, string> param = new() {
				{ "task_id", taskID },
				{ "activity_id", activityID },
				{ "activity_name", activityName },
				{ "task_name", taskName },
				{ "reward_name", rewaredName },
				{ "gaia_vtoken", "" },//默认为空
				{ "receive_from", "missionPage" },
				{ "csrf", userCredential.Bili_Jct },
			};
			var wbi = BilibiliApiTool.GetWbi(param);
			using var request = new HttpRequestMessage(HttpMethod.Post, $"{Constants.ReceiveAward}?{wbi}");
			request.Headers.Add("cookie", $"SESSDATA={userCredential.Sessdata}");
			request.Content = new FormUrlEncodedContent(param);

			var result = HttpClient.SendAsync(request);
			_logger.LogInformation("用户{user}领取的任务id：{}活动id：{}活动名称：{}任务名称：{}奖励名称：{}", userCredential.DedeUserID, taskID, activityID, activityName, taskName, rewaredName);
		}
	}
}
