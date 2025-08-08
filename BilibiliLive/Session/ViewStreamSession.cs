using BilibiliLive.Handle;
using BilibiliLive.Interaction;
using BilibiliLive.Models;
using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using OpenBLive.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Session
{
	/// <summary>
	/// 看播会话（）
	/// </summary>
	sealed class ViewStreamSession
	{
		private ILogger _logger = GlobalSetting.CreateLogger(typeof(ViewStreamSession));
		public UserCredential UserCredential { get; } = new();//自己的用户信息
		public string UserName { get; } = string.Empty;
		public long TargetRoomID { get; } = 0;//目标观看的房间
		public string TargetUserName { get; } = string.Empty;
		public int LivePart { get; } = 0;//大分区
		public int AreaV2 { get; } = 0;//小分区
		public List<(int code, string msg)> HeartResult = new();//心跳结果

		public bool IsView { get; private set; } = false;//是否正在观看直播，是开启和关停观看直播的关键
		public ViewStreamSession(UserCredential userCredential, string userName, string targetUserName, long targetRoomID, int livePart, int areaV2)
		{
			UserCredential = userCredential;
			TargetRoomID = targetRoomID;
			UserName = userName;
			TargetUserName = targetUserName;
			LivePart = livePart;
			AreaV2 = areaV2;
		}

		public void Start()
		{
			IsView = true;
			//生成新的看播的Task
			_ = Task.Run(async () =>
			{
				try
				{
					_logger.LogInformation("[{user}]观看直播间[{targetRoomID}]，EHeartBeat", UserCredential.DedeUserID, TargetRoomID);

					//发送E消息
					var Eresult = await UserInteraction.LiveEHeartBeat(UserCredential, LivePart, AreaV2, TargetRoomID);

					AddResult(ref HeartResult, Eresult.code, Eresult.msg);
					var timeInterval = Eresult.timeInterval;
					//var timeInterval = 1;
					var ts = Eresult.ts;
					var secret_key = Eresult.secret_key;
					int index = 0;
					while (IsView)
					{
						index++;
						_logger.LogDebug("[{user}]:[{targetRoomID}]看播等待{interval}s中", UserCredential.DedeUserID, TargetRoomID, timeInterval);
						await Task.Delay(timeInterval * 1000);
						var Xresult = await UserInteraction.LiveXHeratBeat(UserCredential, LivePart, AreaV2, index, TargetRoomID, ts, secret_key, timeInterval);

						AddResult(ref HeartResult, Xresult.code, Xresult.msg);
						timeInterval = Xresult.timeInterval;
						ts = Xresult.ts;
						secret_key = Xresult.secret_key;
					}
				}
				catch (Exception ex)
				{
					AddResult(ref HeartResult, -1, $"[{DateTimeOffset.Now.ToUnixTimeSeconds()}]遇到错误，请前往控制台查看");
					IsView = false;
					_logger.LogError(ex, "[{user}]:[{targetRoomID}]看播出现错误", UserCredential.DedeUserID, TargetRoomID);
				}
			});
		}
		public void Stop()
		{
			_logger.LogInformation("[{user}]关闭观看直播间[{targetRoomID}]", UserCredential.DedeUserID, TargetRoomID);
			IsView = false;
		}

		void AddResult(ref List<(int code, string msg)> result, int code, string msg)
		{
			if (code != 0)
			{
				result.Add(new(code, msg));
			}
			else
			{
				result.Add(new(0, "心跳成功"));
			}
		}
	}
}
