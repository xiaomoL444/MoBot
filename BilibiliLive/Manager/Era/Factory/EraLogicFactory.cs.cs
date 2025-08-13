using BilibiliLive.Manager.Era.Core;
using BilibiliLive.Manager.Era.Logics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Manager.Era.Factory
{
	public static class EraLogicFactory
	{
		private static readonly Dictionary<string, IEraLogic> _logicMap = new()
		{
			["genshin"] = new GenshinLogic(),
			["starrail"] = new StarRailLogic(),
			["zzz"] = new ZZZLogic()
			// 未来继续加
		};

		public static IEraLogic GetLogic(string gameName)
		{
			if (_logicMap.TryGetValue(gameName, out var logic))
				return logic;
			throw new ArgumentException($"Unknown activityId: {gameName}");
		}
	}
}
