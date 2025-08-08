using BilibiliLive.Models;
using MoBot.Core.Models.Message;
using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Manager.Era.Core
{
	public interface IEraLogic
	{
		/// <summary>
		/// 刷新激励计划数据
		/// </summary>
		/// <param name="gameName">游戏名</param>
		/// <returns></returns>
		Task<OneOf<Success<string>, Error<string>>> RefreshEraData();

		/// <summary>
		/// 查询任务
		/// </summary>
		/// <param name="uid">查询任务的id(一次只能查询一位，当然用异步同时查也没关系（））</param>
		/// <param name="gameName">游戏名</param>
		/// <returns>返回图查获错误信息</returns>
		Task<OneOf<Success<string>, Error<string>>> QueryTasks(string uid);

		/// <summary>
		/// 领取每日奖励
		/// </summary>
		/// <returns>返回图查或错误信息</returns>
		Task<OneOf<Success<string>, Error<string>>> ReceiveDailyEraAward(List<string> uidList);

		/// <summary>
		/// 领取看播/直播奖励
		/// </summary>
		/// <param name="groupID"></param>
		/// <returns>成功返回图查，失败返回消息</returns>
		Task<OneOf<Success<string>, Error<string>,None>> ReceiveFinallylEraAward(List<string> uidList,string awardName);
	}
}
