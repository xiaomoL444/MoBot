using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	internal class StartLiveRsp
	{
		/// <summary>
		/// 返回值：
		/// 0：成功，65530：token错误，1：错误，
		/// 60009：分区不存在，60024：需要人脸认证，
		/// 60013：地区实名认证限制
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = 1;

		/// <summary>
		/// 错误信息（默认为空）
		/// </summary>
		[JsonProperty("msg")]
		public string Msg { get; set; } = string.Empty;

		/// <summary>
		/// 错误信息（默认为空）
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// 信息本体
		/// </summary>
		[JsonProperty("data")]
		public StartLiveData Data { get; set; } = new StartLiveData();
	}

	public class StartLiveData
	{
		/// <summary>
		/// 是否改变状态：0 未改变，1 改变
		/// </summary>
		[JsonProperty("change")]
		public int Change { get; set; } = 0;

		/// <summary>
		/// 直播间状态，例如 "LIVE"
		/// </summary>
		[JsonProperty("status")]
		public string Status { get; set; } = string.Empty;

		/// <summary>
		/// 房间类型（作用尚不明确）
		/// </summary>
		[JsonProperty("room_type")]
		public int RoomType { get; set; } = 0;

		/// <summary>
		/// RTMP 推流地址信息
		/// </summary>
		[JsonProperty("rtmp")]
		public RtmpInfo Rtmp { get; set; } = new RtmpInfo();

		/// <summary>
		/// 协议列表（作用尚不明确）
		/// </summary>
		[JsonProperty("protocols")]
		public List<ProtocolInfo> Protocols { get; set; } = new List<ProtocolInfo>();

		/// <summary>
		/// 尝试时间？（作用尚不明确）
		/// </summary>
		[JsonProperty("try_time")]
		public string TryTime { get; set; } = string.Empty;

		/// <summary>
		/// 标记直播场次的 key
		/// </summary>
		[JsonProperty("live_key")]
		public string LiveKey { get; set; } = string.Empty;

		/// <summary>
		/// 信息变动标识
		/// </summary>
		[JsonProperty("sub_session_key")]
		public string SubSessionKey { get; set; } = string.Empty;

		/// <summary>
		/// 通知信息（作用尚不明确）
		/// </summary>
		[JsonProperty("notice")]
		public NoticeInfo Notice { get; set; } = new NoticeInfo();

		/// <summary>
		/// 未知用途字段（通常为空字符串）
		/// </summary>
		[JsonProperty("qr")]
		public string Qr { get; set; } = string.Empty;

		/// <summary>
		/// 是否需要人脸认证
		/// </summary>
		[JsonProperty("need_face_auth")]
		public bool NeedFaceAuth { get; set; } = false;

		/// <summary>
		/// 服务来源（作用尚不明确）
		/// </summary>
		[JsonProperty("service_source")]
		public string ServiceSource { get; set; } = string.Empty;

		/// <summary>
		/// RTMP 备用地址（为 null）
		/// </summary>
		[JsonProperty("rtmp_backup")]
		public object RtmpBackup { get; set; } = null;

		/// <summary>
		/// 主播推流额外信息
		/// </summary>
		[JsonProperty("up_stream_extra")]
		public UpStreamExtra UpStreamExtra { get; set; } = new UpStreamExtra();
	}

	public class RtmpInfo
	{
		/// <summary>
		/// RTMP 推流地址
		/// </summary>
		[JsonProperty("addr")]
		public string Addr { get; set; } = string.Empty;

		/// <summary>
		/// RTMP 推流参数（密钥）
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; } = string.Empty;

		/// <summary>
		/// CDN 推流 IP 重定向信息地址（一般没用）
		/// </summary>
		[JsonProperty("new_link")]
		public string NewLink { get; set; } = string.Empty;

		/// <summary>
		/// 推流服务提供者（可能为 txy 等）
		/// </summary>
		[JsonProperty("provider")]
		public string Provider { get; set; } = string.Empty;
	}

	public class ProtocolInfo
	{
		/// <summary>
		/// 协议类型，例如 "rtmp"
		/// </summary>
		[JsonProperty("protocol")]
		public string Protocol { get; set; } = string.Empty;

		/// <summary>
		/// RTMP 推流地址
		/// </summary>
		[JsonProperty("addr")]
		public string Addr { get; set; } = string.Empty;

		/// <summary>
		/// 推流参数（密钥）
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; } = string.Empty;

		/// <summary>
		/// 重定向地址
		/// </summary>
		[JsonProperty("new_link")]
		public string NewLink { get; set; } = string.Empty;

		/// <summary>
		/// 服务提供者（如 txy）
		/// </summary>
		[JsonProperty("provider")]
		public string Provider { get; set; } = string.Empty;
	}

	public class NoticeInfo
	{
		/// <summary>
		/// 类型（作用尚不明确）
		/// </summary>
		[JsonProperty("type")]
		public int Type { get; set; } = 1;

		/// <summary>
		/// 状态（作用尚不明确）
		/// </summary>
		[JsonProperty("status")]
		public int Status { get; set; } = 0;

		/// <summary>
		/// 标题
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// 消息内容
		/// </summary>
		[JsonProperty("msg")]
		public string Msg { get; set; } = string.Empty;

		/// <summary>
		/// 按钮文字
		/// </summary>
		[JsonProperty("button_text")]
		public string ButtonText { get; set; } = string.Empty;

		/// <summary>
		/// 按钮 URL
		/// </summary>
		[JsonProperty("button_url")]
		public string ButtonUrl { get; set; } = string.Empty;
	}

	public class UpStreamExtra
	{
		/// <summary>
		/// 主播的互联网服务提供商
		/// </summary>
		[JsonProperty("isp")]
		public string Isp { get; set; } = string.Empty;
	}
}
