﻿using Newtonsoft.Json;
using OpenBLive.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class AccountConfig
	{
		/// <summary>
		/// B站的cookie
		/// </summary>
		[JsonProperty("cookie")]
		public string Cookie { get; set; } = "";

		/// <summary>
		/// 测试用（）远程推流的链接
		/// </summary>
		[JsonProperty("rtmp_url")]
		[Obsolete]
		public string RtmpUrl { get; set; } = "";

		public List<UserCredential> Accounts { get; set; } = new();
	}
}
