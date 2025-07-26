using BilibiliLive.Constant;
using BilibiliLive.Models.Live;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{/// <summary>
	 /// 获取用户对应的直播间状态
	 /// </summary>
	 /// <param name="userID">用户ID</param>
	 /// <returns></returns>
		public static async Task<LiveRoomInfoRsp> GetUserRoomInfo(string userID)
		{
			var getLiveRoomRequest = new HttpRequestMessage(HttpMethod.Get, $"{Constants.BilibiliGetRoomInfoOld}?mid={userID}");
			var getLiveRoomResponse = await Tool.HttpClient.SendAsync(getLiveRoomRequest);
			var getLiveRoomResponseString = await getLiveRoomResponse.Content.ReadAsStringAsync();
			_logger.LogDebug("获取{user}直播信息的回复{@response}", userID, getLiveRoomResponseString);

			var getLiveRoomData = JsonConvert.DeserializeObject<LiveRoomInfoRsp>(getLiveRoomResponseString);
			return getLiveRoomData;
		}
	}
}
