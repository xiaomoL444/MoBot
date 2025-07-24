using BilibiliLive.Tool;
using Microsoft.Extensions.Logging;
using HttpClient = BilibiliLive.Tool.HttpClient;
using BilibiliLive.Tool;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		private static readonly ILogger _logger = GlobalLogger.CreateLogger(typeof(UserInteraction));
	}
}
