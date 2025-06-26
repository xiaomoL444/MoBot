using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTask.Models
{
	public class MingjuListResponse
	{
		/// <summary>
		/// 返回码，如 200 表示成功
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = 0;

		/// <summary>
		/// 错误信息（通常为空）
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// 结果数据
		/// </summary>
		[JsonProperty("result")]
		public MingjuResult Result { get; set; } = new MingjuResult();

		/// <summary>
		/// 是否为扩展对象
		/// </summary>
		[JsonProperty("objectEx")]
		public bool ObjectEx { get; set; } = false;

		/// <summary>
		/// 页面时间（字符串格式）
		/// </summary>
		[JsonProperty("pageTime")]
		public string PageTime { get; set; } = string.Empty;
	}

	public class MingjuResult
	{
		/// <summary>
		/// 名句列表
		/// </summary>
		[JsonProperty("mingjuList")]
		public List<MingjuItem> MingjuList { get; set; } = new List<MingjuItem>();
	}

	public class MingjuItem
	{
		/// <summary>
		/// 唯一标识 ID（字符串）
		/// </summary>
		[JsonProperty("idStr")]
		public string IdStr { get; set; } = string.Empty;

		/// <summary>
		/// 名句正文
		/// </summary>
		[JsonProperty("nameStr")]
		public string NameStr { get; set; } = string.Empty;

		/// <summary>
		/// 作者
		/// </summary>
		[JsonProperty("author")]
		public string Author { get; set; } = string.Empty;

		/// <summary>
		/// 出处（作品名）
		/// </summary>
		[JsonProperty("source")]
		public string Source { get; set; } = string.Empty;

		/// <summary>
		/// 出处 ID
		/// </summary>
		[JsonProperty("sourceIdStr")]
		public string SourceIdStr { get; set; } = string.Empty;

		/// <summary>
		/// 归属类别（1/2/4 等）
		/// </summary>
		[JsonProperty("guishu")]
		public int Guishu { get; set; } = 0;

		/// <summary>
		/// 图片地址（相对路径）
		/// </summary>
		[JsonProperty("picUrl")]
		public string PicUrl { get; set; } = string.Empty;

		/// <summary>
		/// 图片标题
		/// </summary>
		[JsonProperty("picName")]
		public string PicName { get; set; } = string.Empty;

		/// <summary>
		/// 图片作者
		/// </summary>
		[JsonProperty("picAuthor")]
		public string PicAuthor { get; set; } = string.Empty;

		/// <summary>
		/// 藏馆信息
		/// </summary>
		[JsonProperty("picCangguan")]
		public string PicCangguan { get; set; } = string.Empty;

		/// <summary>
		/// 图片朝代
		/// </summary>
		[JsonProperty("picChaodai")]
		public string PicChaodai { get; set; } = string.Empty;

		/// <summary>
		/// 更新时间（字符串）
		/// </summary>
		[JsonProperty("upTime")]
		public string UpTime { get; set; } = string.Empty;

		/// <summary>
		/// 更新时间戳（毫秒）
		/// </summary>
		[JsonProperty("upTimeSpan")]
		public long UpTimeSpan { get; set; } = 0;
	}
}
