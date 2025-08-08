using Newtonsoft.Json;

namespace MoBot.Core.Models.Net
{
	public class ActionPacketRsp
	{
		[JsonIgnore]
		public string RawMessage { get; set; } = string.Empty;

		[JsonProperty("status")]
		public string Status { get; set; } = "";

		[JsonProperty("retcode")]
		public int Retcode { get; set; } = 0;

		[JsonProperty("data")]
		public object? Data { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; } = "";

		[JsonProperty("wording")]
		public string Wording { get; set; } = "";

		[JsonProperty("echo")]
		public string Echo { get; set; } = "";
	}
}
