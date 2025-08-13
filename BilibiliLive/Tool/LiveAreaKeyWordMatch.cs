using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	internal static class LiveAreaKeyWordMatch
	{
		private static Dictionary<string, List<string>> liveAreas = new Dictionary<string, List<string>>() {
			{ "genshin", [
				"genshin",
				"gi",
				"genshinimpact"]},
			{ "starrail",[
				"hsr",
				"starrail",
				"sr"]},
			{ "zzz",[
				"zzz",
				"z",
				"zz"]}};

		public static string Match(string value)
		{
			return liveAreas.FirstOrDefault(q => q.Value.Any(k => k == value.ToLower())).Key;
		}
	}
}
