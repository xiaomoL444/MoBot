using BilibiliLive.Constant;
using BilibiliLive.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		/// <summary>
		/// 发送直播间礼物(仅支持电池的吧，毕竟是coin，不支持包裹)
		/// </summary>
		/// <param name="userCredential">用户凭证</param>
		/// <param name="targetUid">目标用户ID</param>
		/// <param name="roomID">目标房间ID</param>
		/// <param name="giftID">礼物ID</param>
		/// <param name="giftNum">礼物数量</param>
		/// <param name="price">价格？什么价格，总之0是免费，牛蛙是100</param>
		/// <param name="storm_beat_id">连击数</param>
		/// <returns></returns>
		public static async Task SendLiveGift(
			UserCredential userCredential,
			string targetUid,
			string roomID,
			string giftID,
			string price = "100",
			string giftNum = "1",
			string storm_beat_id = "0")
		{
			Dictionary<string, string> sendGiftParam = new()
			{
				{"uid",userCredential.DedeUserID},
				{"gift_id",giftID},
				{"ruid",targetUid},
				{"send_ruid","0"},
				{"gift_num",giftNum},
				{"coin_type","coin"},
				{"bag_id","0"},
				{"platform","pc"},
				{"biz_code","Live"},
				{"biz_id",roomID},
				{"storm_beat_id",storm_beat_id},
				{"metadata",""},
				{"price",price},
				{"receive_users",""},
			  {"live_statistics",@"{""pc_client"":""pcWeb"",""jumpfrom"":""-99998"",""room_category"":""0"",""source_event"":0,""trackid"":""-99998"",""official_channel"":{""program_room_id"":""-99998"",""program_up_id"":""-99998""}}"},
				{"statistics",""},
				{"web_location",@"{""platform"":5,""pc_client"":""pcWeb"",""appId"":100}"},
				{"csrf",userCredential.Bili_Jct},
			};
			var wbi = await GetWbi(sendGiftParam);
			var sendGiftRequest = new HttpRequestMessage(HttpMethod.Post, $"{Constants.SendGift}?{wbi}");
			sendGiftRequest.Headers.Add("Cookie", $"SESSDATA={userCredential.Sessdata};bili_jct={userCredential.Bili_Jct}");
			sendGiftRequest.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");

			var sendGiftResponse = await Tool.HttpClient.SendAsync(sendGiftRequest);
			var sendGiftResponseString = await sendGiftResponse.Content.ReadAsStringAsync();

			_logger.LogDebug("赠送礼物消息：{res}", sendGiftResponseString);
		}
	}
}
