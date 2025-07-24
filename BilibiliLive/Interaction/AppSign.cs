using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Interaction
{
	public partial class UserInteraction
	{
		/// <summary>
		/// 计算AppSign
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="appkey"></param>
		/// <param name="appsec"></param>
		/// <returns></returns>
		public static Dictionary<string, string> AppSign(Dictionary<string, string> parameters, string appkey, string appsec)
		{
			// 添加 appkey
			parameters["appkey"] = appkey;

			// 按 key 排序
			var sortedParams = parameters.OrderBy(p => p.Key);

			// 拼接 URL 查询字符串（urlencode 编码）
			var query = string.Join("&", sortedParams.Select(p =>
				$"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value)}"));

			// 计算 MD5 签名
			var input = query + appsec;
			var sign = string.Empty;
			using (var md5 = MD5.Create())
			{
				byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
				StringBuilder sb = new StringBuilder();
				foreach (var b in data)
					sb.Append(b.ToString("x2")); // 小写十六进制
				sign = sb.ToString();
			}

			// 加入签名字段
			parameters["sign"] = sign;

			return parameters;
		}

	}
}
