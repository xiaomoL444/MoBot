using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool
{
	internal static class Webshot
	{
		public static string GetIPAddress()
		{
#if DEBUG
			return "http://localhost:8080";
#else
			return "http://192.168.100.1:8080";
#endif
		}
	}
}
