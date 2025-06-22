using DailyChat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyChatSettingWinForm
{
	internal static class ShareField
	{
		public static EchoRule EchoRule;
		public static string FilePath;

		public static int SeleteKeyWord = -1;

		public static int SeleteTrigger = -1;
		public static int SeleteWhiteList = -1;

		public static ResponseType responseType = ResponseType.WhiteList;
		public static void Save()
		{
			File.WriteAllText(FilePath, JsonConvert.SerializeObject(EchoRule, Formatting.Indented));
		}
	}
	public enum ResponseType
	{
		WhiteList = 0,
		Normal = 1,
		BlackList = 2,
	}
}
