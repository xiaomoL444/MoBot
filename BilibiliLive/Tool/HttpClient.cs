namespace BilibiliLive.Tool
{
	public static class HttpClient
	{
		private static System.Net.Http.HttpClient _httpClient = new();

		public static async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage httpRequestMessage)
		{
			return (await _httpClient.SendAsync(httpRequestMessage));

		}
	}
}
