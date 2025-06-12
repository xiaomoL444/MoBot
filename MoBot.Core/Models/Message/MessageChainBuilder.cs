using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MoBot.Core.Models.Message
{
	public class MessageChainBuilder
	{
		private List<MessageSegment> _msgSegment = [];

		public static MessageChainBuilder Create() => new();

		public List<MessageSegment> Build() => _msgSegment;

		/// <summary>
		/// 普通文本消息
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public MessageChainBuilder Text(string text)
		{
			_msgSegment.Add(new()
			{
				Type = "text",
				Data = new
				{
					text
				}
			});
			return this;
		}

		/// <summary>
		/// 图片消息
		/// </summary>
		/// <param name="content">
		/// <para>本地路径 file://D:/a.jpg</para>
		/// <para>网络路径 http://i0.hdslb.com/bfs/archive/c8fd97a40bf79f03e7b76cbc87236f612caef7b2.png</para>
		/// <para>base64编码 base64://xxxxxxxx</para>
		/// </param>
		/// <returns></returns>
		public MessageChainBuilder Image(string content)
		{
			_msgSegment.Add(new()
			{
				Type = "image",
				Data = new
				{
					file = content
				}
			});
			return this;
		}
	}
}
