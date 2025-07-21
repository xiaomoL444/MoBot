using System.Net;

namespace BilibiliLive.Tool
{
	public static class HttpClient
	{
		private static System.Net.Http.HttpClient _httpClient = new(new HttpClientHandler
		{
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
		});

		public static async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage httpRequestMessage)
		{
			return (await _httpClient.SendAsync(httpRequestMessage));
		}
	}
}
