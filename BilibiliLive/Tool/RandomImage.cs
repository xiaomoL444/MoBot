using MoBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	public static class RandomImage
	{
		private static readonly IDataStorage _dataStorage = GlobalDataStorage.DataStorage;
		public static string Get()
		{
			var directory = _dataStorage.GetDirectory(MoBot.Core.Models.DirectoryType.Config);
			return "";
		}
	}
}
