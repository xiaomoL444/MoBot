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

		#region 登录API
		public const string BilibiliWebSignQRcodeGenerataApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";//B站的web端扫码链接生成api
		public const string BilibiliWebSignQRcodePollApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/poll";//B站的web端扫码轮询是否登录成功
		#endregion

		#region 直播间API
		public const string BilibiliStartLiveAPI = "https://api.live.bilibili.com/room/v1/Room/startLive";//B站的开播连接
		public const string BilibiliStopLiveApi = "https://api.live.bilibili.com/room/v1/Room/stopLive";//B站的关播连接
		#endregion

	}
}
