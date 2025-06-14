using MoBot.Core.Models.Event.Message;

namespace MoBot.Handle.Extensions
{
	public static class GroupMessageExtension
	{
		/// <summary>
		/// 是否是某个群号
		/// </summary>
		/// <param name="group"></param>
		/// <param name="groupID">群号</param>
		/// <returns></returns>
		public static bool isGroupID(this Group group, long groupID) => group.GroupId == groupID;

		/// <summary>
		/// 是否是某个发送用户的ID
		/// </summary>
		/// <param name="group"></param>
		/// <param name="userID">发送者的ID</param>
		/// <returns></returns>
		public static bool isUserID(this Group group, long userID) => group.UserId == userID;

		/// <summary>
		/// 自己的ID是否是指定ID
		/// </summary>
		/// <param name="group"></param>
		/// <param name="selfID">自己的ID</param>
		/// <returns></returns>
		public static bool isSelfID(this Group group, long selfID) => group.SelfID == selfID;
	}
}
