using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Constant
{
	public static class Constants
	{
		public const long OPGroupID = 1079464803;
		public const long OPAdmin = 2580139692;

		public const string AccountFile = "account";
		public const string EraFile = "era";
		public const string StreamFile = "stream";

		public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36";//user-agent

		public const string JobGroup = "BilibiliLive";
		public const string TriggerGroup = "BilibiliLive";

		#region 登录API
		public const string BilibiliWebSignQRcodeGenerataApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";//B站的web端扫码链接生成api
		public const string BilibiliWebSignQRcodePollApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/poll";//B站的web端扫码轮询是否登录成功
		#endregion

		#region 直播间API
		public const string BilibiliStartLiveAPI = "https://api.live.bilibili.com/room/v1/Room/startLive";//B站的开播连接
		public const string BilibiliStopLiveApi = "https://api.live.bilibili.com/room/v1/Room/stopLive";//B站的关播连接
		public const string BilibiliGetRoomInfoOld = "https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld";//获取用户对应的直播间状态（主要是为了获取roomID）
		public const string GetLivehimeVersion = "https://api.live.bilibili.com/xlive/app-blink/v1/liveVersionInfo/getHomePageLiveVersion";//获取直播姬版本信息
		public const string SendGift = "https://api.live.bilibili.com/xlive/revenue/v1/gift/sendGold";//发送礼物
		public const string SendDanmaku = "https://api.live.bilibili.com/msg/send";//发送弹幕消息

		public const string EHeartBeat = "https://live-trace.bilibili.com/xlive/data-interface/v1/x25Kn/E";//直播心跳E回复
		public const string XHeartBeat = "https://live-trace.bilibili.com/xlive/data-interface/v1/x25Kn/X";//直播心跳X回复
		public const string LiveHeartGenerateSUrl = "http://bilivekeepheart.lan:3788";// 生成直播的S加密的链接
		#endregion

		#region 用户API
		public const string GetUserInfo = "https://api.bilibili.com/x/space/wbi/acc/info";
		#endregion

		#region 激励计划API
		public const string GetEraUrl = "https://app.bilibili.com/x/topic/web/details/top";//获取激励计划的页面链接
		public const string GetEraTask = "https://api.bilibili.com/x/task/totalv2";//获取任务完成情况
		public const string ReceiveAward = "https://api.bilibili.com/x/activity_components/mission/receive";//领取任务奖励
		#endregion
	}
}
