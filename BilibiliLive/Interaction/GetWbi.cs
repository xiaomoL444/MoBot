using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		/// <summary>
		/// WBI 签名 https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/docs/misc/sign/wbi.md#csharp
		/// 
		/// </summary>
		/// <param name="parameters">Url参数</param>
		/// <returns>WBI签名需要的wts和w_rid</returns>
		public static async Task<string> GetWbi(Dictionary<string, string> parameters)
		{
			int[] MixinKeyEncTab =
   {
		46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
		12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
		57, 62, 11, 36, 20, 34, 44, 52
	};
			//GetWbiKeys
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.bilibili.com/x/web-interface/nav");
			httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
			httpRequestMessage.Headers.Referrer = new Uri("https://www.bilibili.com/");

			HttpResponseMessage responseMessage = await Tool.HttpClient.SendAsync(httpRequestMessage);

			JsonNode response = JsonNode.Parse(await responseMessage.Content.ReadAsStringAsync())!;

			string imgUrl = (string)response["data"]!["wbi_img"]!["img_url"]!;
			imgUrl = imgUrl.Split("/")[^1].Split(".")[0];

			string subUrl = (string)response["data"]!["wbi_img"]!["sub_url"]!;
			subUrl = subUrl.Split("/")[^1].Split(".")[0];

			//EncWbi
			string mixinKey = MixinKeyEncTab.Aggregate("", (s, i) => s + (imgUrl + subUrl)[i])[..32];//MixinKey对 imgKey 和 subKey 进行字符顺序打乱编码
			string currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
			//添加 wts 字段
			parameters["wts"] = currTime;
			// 按照 key 重排参数
			parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
			//过滤 value 中的 "!'()*" 字符
			parameters = parameters.ToDictionary(
				kvp => kvp.Key,
				kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
			);
			// 序列化参数
			string query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
			//计算 w_rid
			using MD5 md5 = MD5.Create();
			byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
			string wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			parameters["w_rid"] = wbiSign;

			return (await new FormUrlEncodedContent(parameters).ReadAsStringAsync());

		}

	}
}
