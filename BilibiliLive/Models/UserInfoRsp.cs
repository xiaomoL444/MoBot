using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Models
{
	public class UserInfoRsp
	{
		/// <summary>
		/// 返回码：0 成功，-400 请求错误，-403 无权限，-404 用户不存在
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; } = -400;

		/// <summary>
		/// 错误信息，默认为空
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// 固定值 1
		/// </summary>
		[JsonProperty("ttl")]
		public int Ttl { get; set; } = 1;

		/// <summary>
		/// 用户信息主体
		/// </summary>
		[JsonProperty("data")]
		public UserInfoData Data { get; set; } = new UserInfoData();

		public class UserInfoData
		{
			/// <summary>
			/// 用户 mid
			/// </summary>
			[JsonProperty("mid")]
			public long Mid { get; set; } = 0;

			/// <summary>
			/// 昵称
			/// </summary>
			[JsonProperty("name")]
			public string Name { get; set; } = string.Empty;

			/// <summary>
			/// 性别 男/女/保密
			/// </summary>
			[JsonProperty("sex")]
			public string Sex { get; set; } = "保密";

			/// <summary>
			/// 头像链接
			/// </summary>
			[JsonProperty("face")]
			public string Face { get; set; } = string.Empty;

			/// <summary>
			/// 是否为 NFT 头像：0 否，1 是
			/// </summary>
			[JsonProperty("face_nft")]
			public int FaceNft { get; set; } = 0;

			/// <summary>
			/// NFT 头像类型
			/// </summary>
			[JsonProperty("face_nft_type")]
			public int FaceNftType { get; set; } = 0;

			/// <summary>
			/// 签名
			/// </summary>
			[JsonProperty("sign")]
			public string Sign { get; set; } = string.Empty;

			/// <summary>
			/// 用户权限等级
			/// </summary>
			[JsonProperty("rank")]
			public int Rank { get; set; } = 10000;

			/// <summary>
			/// 当前等级（0~6）
			/// </summary>
			[JsonProperty("level")]
			public int Level { get; set; } = 0;

			/// <summary>
			/// 注册时间，恒为 0
			/// </summary>
			[JsonProperty("jointime")]
			public long JoinTime { get; set; } = 0;

			/// <summary>
			/// 节操值，恒为 0
			/// </summary>
			[JsonProperty("moral")]
			public int Moral { get; set; } = 0;

			/// <summary>
			/// 封禁状态：0 正常，1 被封
			/// </summary>
			[JsonProperty("silence")]
			public int Silence { get; set; } = 0;

			/// <summary>
			/// 硬币数，仅可查看自己，默认 0
			/// </summary>
			[JsonProperty("coins")]
			public double Coins { get; set; } = 0;

			/// <summary>
			/// 是否具有粉丝勋章
			/// </summary>
			[JsonProperty("fans_badge")]
			public bool FansBadge { get; set; } = false;

			/// <summary>
			/// 标签列表（可能为 null）
			/// </summary>
			[JsonProperty("tags")]
			public List<string> Tags { get; set; } = new List<string>();

			/// <summary>
			/// 是否为硬核会员
			/// </summary>
			[JsonProperty("is_senior_member")]
			public int IsSeniorMember { get; set; } = 0;

			/// <summary>
			/// 主页头图链接
			/// </summary>
			[JsonProperty("top_photo")]
			public string TopPhoto { get; set; } = string.Empty;

			/// <summary>
			/// 生日，格式 MM-DD
			/// </summary>
			[JsonProperty("birthday")]
			public string Birthday { get; set; } = string.Empty;

			/// <summary>
			/// 是否已关注该用户
			/// </summary>
			[JsonProperty("is_followed")]
			public bool IsFollowed { get; set; } = false;

			/// <summary>
			/// 系统通知
			/// </summary>
			[JsonProperty("sys_notice")]
			public SysNoticeData SysNotice { get; set; } = new();

			public class SysNoticeData
			{
				/// <summary>
				/// 公告 ID
				/// </summary>
				[JsonProperty("id")]
				public long Id { get; set; } = 0;

				/// <summary>
				/// 显示文案内容
				/// </summary>
				[JsonProperty("content")]
				public string Content { get; set; } = string.Empty;

				/// <summary>
				/// 点击跳转地址
				/// </summary>
				[JsonProperty("url")]
				public string Url { get; set; } = string.Empty;

				/// <summary>
				/// 提示类型（如 1 或 2）
				/// </summary>
				[JsonProperty("notice_type")]
				public int NoticeType { get; set; } = 0;

				/// <summary>
				/// 前缀图标地址（或类名）
				/// </summary>
				[JsonProperty("icon")]
				public string Icon { get; set; } = string.Empty;

				/// <summary>
				/// 文字颜色（十六进制格式，如 #FFFFFF）
				/// </summary>
				[JsonProperty("text_color")]
				public string TextColor { get; set; } = string.Empty;

				/// <summary>
				/// 背景颜色（十六进制格式，如 #000000）
				/// </summary>
				[JsonProperty("bg_color")]
				public string BgColor { get; set; } = string.Empty;
			}

			/// <summary>
			/// 学校信息
			/// </summary>
			[JsonProperty("school")]
			public SchoolData School { get; set; } = new();

			public class SchoolData
			{
				/// <summary>
				/// 就读大学名称（没有则为空）
				/// </summary>
				[JsonProperty("name")]
				public string Name { get; set; } = string.Empty;
			}


			/// <summary>
			/// 专业资质信息
			/// </summary>
			[JsonProperty("profession")]
			public ProfessionData Profession { get; set; } = new();

			public class ProfessionData
			{
				/// <summary>
				/// 资质名称
				/// </summary>
				[JsonProperty("name")]
				public string Name { get; set; } = string.Empty;

				/// <summary>
				/// 职位名称
				/// </summary>
				[JsonProperty("department")]
				public string Department { get; set; } = string.Empty;

				/// <summary>
				/// 所属机构
				/// </summary>
				[JsonProperty("title")]
				public string Title { get; set; } = string.Empty;

				/// <summary>
				/// 是否显示：0 = 不显示，1 = 显示
				/// </summary>
				[JsonProperty("is_show")]
				public int IsShow { get; set; } = 0;
			}

			/// <summary>
			/// 系列相关信息
			/// </summary>
			[JsonProperty("series")]
			public SeriesData Series { get; set; } = new();

			public class SeriesData
			{
				[JsonProperty("user_upgrade_status")]
				public int UserUpgradeStatus { get; set; } = 0;

				[JsonProperty("show_upgrade_window")]
				public bool ShowUpgradeWindow { get; set; } = false;
			}

			/// <summary>
			/// 粉丝勋章信息
			/// </summary>
			[JsonProperty("fans_medal")]
			public FansMedalData FansMedal { get; set; } = new();

			public class FansMedalData
			{
				/// <summary>
				/// 是否显示
				/// </summary>
				[JsonProperty("show")]
				public bool Show { get; set; } = false;

				/// <summary>
				/// 是否佩戴了粉丝勋章
				/// </summary>
				[JsonProperty("wear")]
				public bool Wear { get; set; } = false;

				/// <summary>
				/// 粉丝勋章信息
				/// </summary>
				[JsonProperty("medal")]
				public MedalData Medal { get; set; } = new();

				public class MedalData
				{
					/// <summary>
					/// 用户mid
					/// </summary>
					[JsonProperty("uid")]
					public long Uid { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章所属UP的mid
					/// </summary>
					[JsonProperty("target_id")]
					public long TargetId { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章id
					/// </summary>
					[JsonProperty("medal_id")]
					public int MedalId { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章等级
					/// </summary>
					[JsonProperty("level")]
					public int Level { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章名称
					/// </summary>
					[JsonProperty("medal_name")]
					public string MedalName { get; set; } = string.Empty;

					/// <summary>
					/// 颜色
					/// </summary>
					[JsonProperty("medal_color")]
					public int MedalColor { get; set; } = 0;

					/// <summary>
					/// 当前亲密度
					/// </summary>
					[JsonProperty("intimacy")]
					public int Intimacy { get; set; } = 0;

					/// <summary>
					/// 下一等级所需亲密度
					/// </summary>
					[JsonProperty("next_intimacy")]
					public int NextIntimacy { get; set; } = 0;

					/// <summary>
					/// 每日亲密度获取上限
					/// </summary>
					[JsonProperty("day_limit")]
					public int DayLimit { get; set; } = 0;

					/// <summary>
					/// 今日已获得亲密度
					/// </summary>
					[JsonProperty("today_feed")]
					public int TodayFeed { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章颜色，十进制数，可转为十六进制颜色代码
					/// </summary>
					[JsonProperty("medal_color_start")]
					public int MedalColorStart { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章颜色，十进制数，可转为十六进制颜色代码
					/// </summary>
					[JsonProperty("medal_color_end")]
					public int MedalColorEnd { get; set; } = 0;

					/// <summary>
					/// 粉丝勋章边框颜色，十进制数，可转为十六进制颜色代码
					/// </summary>
					[JsonProperty("medal_color_border")]
					public int MedalColorBorder { get; set; } = 0;

					/// <summary>
					/// 作用尚不明确
					/// </summary>
					[JsonProperty("is_lighted")]
					public int IsLighted { get; set; } = 0;

					/// <summary>
					/// 作用尚不明确
					/// </summary>
					[JsonProperty("light_status")]
					public int LightStatus { get; set; } = 0;

					/// <summary>
					/// 当前是否佩戴，0：未佩戴，1：已佩戴
					/// </summary>
					[JsonProperty("wearing_status")]
					public int WearingStatus { get; set; } = 0;

					/// <summary>
					/// 作用尚不明确
					/// </summary>
					[JsonProperty("score")]
					public int Score { get; set; } = 0;
				}
			}


			/// <summary>
			/// 认证信息
			/// </summary>
			[JsonProperty("official")]
			public OfficialData Official { get; set; } = new();

			public class OfficialData
			{
				/// <summary>
				/// 认证类型，见用户认证类型一览
				/// </summary>
				[JsonProperty("role")]
				public int Role { get; set; } = -1;

				/// <summary>
				/// 认证信息，无为空
				/// </summary>
				[JsonProperty("title")]
				public string Title { get; set; } = string.Empty;

				/// <summary>
				/// 认证备注，无为空
				/// </summary>
				[JsonProperty("desc")]
				public string Desc { get; set; } = string.Empty;

				/// <summary>
				/// 是否认证，-1：无，0：个人认证，1：机构认证
				/// </summary>
				[JsonProperty("type")]
				public int Type { get; set; } = -1;
			}


			/// <summary>
			/// 会员信息
			/// </summary>
			[JsonProperty("vip")]
			public VipData Vip { get; set; } = new();

			public class VipData
			{
				/// <summary>
				/// 会员类型，0：无，1：月大会员，2：年度及以上大会员
				/// </summary>
				[JsonProperty("type")]
				public int Type { get; set; } = 0;

				/// <summary>
				/// 会员状态，0：无，1：有
				/// </summary>
				[JsonProperty("status")]
				public int Status { get; set; } = 0;

				/// <summary>
				/// 会员过期时间（毫秒时间戳）
				/// </summary>
				[JsonProperty("due_date")]
				public long DueDate { get; set; } = 0;

				/// <summary>
				/// 支付类型，0：未开启自动续费，1：已开启自动续费
				/// </summary>
				[JsonProperty("vip_pay_type")]
				public int VipPayType { get; set; } = 0;

				/// <summary>
				/// 作用尚不明确
				/// </summary>
				[JsonProperty("theme_type")]
				public int ThemeType { get; set; } = 0;

				/// <summary>
				/// 会员标签
				/// </summary>
				[JsonProperty("label")]
				public VipLabelData Label { get; set; } = new();

				public class VipLabelData
				{
					/// <summary>
					/// 空，作用尚不明确
					/// </summary>
					[JsonProperty("path")]
					public string Path { get; set; } = string.Empty;

					/// <summary>
					/// 会员类型文案
					/// </summary>
					[JsonProperty("text")]
					public string Text { get; set; } = string.Empty;

					/// <summary>
					/// 会员标签，示例：vip、annual_vip等
					/// </summary>
					[JsonProperty("label_theme")]
					public string LabelTheme { get; set; } = string.Empty;

					/// <summary>
					/// 会员标签文字颜色
					/// </summary>
					[JsonProperty("text_color")]
					public string TextColor { get; set; } = string.Empty;

					/// <summary>
					/// 背景样式，通常为1
					/// </summary>
					[JsonProperty("bg_style")]
					public int BgStyle { get; set; } = 1;

					/// <summary>
					/// 会员标签背景颜色
					/// </summary>
					[JsonProperty("bg_color")]
					public string BgColor { get; set; } = string.Empty;

					/// <summary>
					/// 会员标签边框颜色，未使用
					/// </summary>
					[JsonProperty("border_color")]
					public string BorderColor { get; set; } = string.Empty;

					/// <summary>
					/// 是否使用图片标签
					/// </summary>
					[JsonProperty("use_img_label")]
					public bool UseImgLabel { get; set; } = true;

					/// <summary>
					/// 简体版图片标签uri，空串
					/// </summary>
					[JsonProperty("img_label_uri_hans")]
					public string ImgLabelUriHans { get; set; } = string.Empty;

					/// <summary>
					/// 繁体版图片标签uri，空串
					/// </summary>
					[JsonProperty("img_label_uri_hant")]
					public string ImgLabelUriHant { get; set; } = string.Empty;

					/// <summary>
					/// 简体版大会员牌子图片
					/// </summary>
					[JsonProperty("img_label_uri_hans_static")]
					public string ImgLabelUriHansStatic { get; set; } = string.Empty;

					/// <summary>
					/// 繁体版大会员牌子图片
					/// </summary>
					[JsonProperty("img_label_uri_hant_static")]
					public string ImgLabelUriHantStatic { get; set; } = string.Empty;
				}

				/// <summary>
				/// 是否显示会员图标，0：不显示，1：显示
				/// </summary>
				[JsonProperty("avatar_subscript")]
				public int AvatarSubscript { get; set; } = 0;

				/// <summary>
				/// 会员昵称颜色，颜色码
				/// </summary>
				[JsonProperty("nickname_color")]
				public string NicknameColor { get; set; } = string.Empty;

				/// <summary>
				/// 大角色类型，1：月度大会员，3：年度大会员，7：十年大会员，15：百年大会员
				/// </summary>
				[JsonProperty("role")]
				public int Role { get; set; } = 0;

				/// <summary>
				/// 大会员角标地址
				/// </summary>
				[JsonProperty("avatar_subscript_url")]
				public string AvatarSubscriptUrl { get; set; } = string.Empty;

				/// <summary>
				/// 电视大会员状态，0：未开通
				/// </summary>
				[JsonProperty("tv_vip_status")]
				public int TvVipStatus { get; set; } = 0;

				/// <summary>
				/// 电视大会员支付类型
				/// </summary>
				[JsonProperty("tv_vip_pay_type")]
				public int TvVipPayType { get; set; } = 0;

				/// <summary>
				/// 电视大会员过期时间（秒级时间戳）
				/// </summary>
				[JsonProperty("tv_due_date")]
				public long TvDueDate { get; set; } = 0;

				/// <summary>
				/// 大会员角标信息
				/// </summary>
				[JsonProperty("avatar_icon")]
				public AvatarIconData AvatarIcon { get; set; } = new();
				public class AvatarIconData
				{
					/// <summary>
					/// 作用尚不明确
					/// </summary>
					[JsonProperty("icon_type")]
					public int IconType { get; set; } = 0;

					/// <summary>
					/// 作用尚不明确
					/// </summary>
					[JsonProperty("icon_resource")]
					public object IconResource { get; set; } = null;
				}
			}


			/// <summary>
			/// 头像框信息
			/// </summary>
			[JsonProperty("pendant")]
			public PendantData Pendant { get; set; } = new();

			public class PendantData
			{
				/// <summary>
				/// 头像框ID
				/// </summary>
				[JsonProperty("pid")]
				public long Pid { get; set; } = 0;

				/// <summary>
				/// 头像框名称
				/// </summary>
				[JsonProperty("name")]
				public string Name { get; set; } = string.Empty;

				/// <summary>
				/// 头像框图片URL
				/// </summary>
				[JsonProperty("image")]
				public string Image { get; set; } = string.Empty;

				/// <summary>
				/// 过期时间，此接口返回恒为0
				/// </summary>
				[JsonProperty("expire")]
				public int Expire { get; set; } = 0;

				/// <summary>
				/// 头像框图片URL（增强版）
				/// </summary>
				[JsonProperty("image_enhance")]
				public string ImageEnhance { get; set; } = string.Empty;

				/// <summary>
				/// 头像框图片逐帧序列URL
				/// </summary>
				[JsonProperty("image_enhance_frame")]
				public string ImageEnhanceFrame { get; set; } = string.Empty;

				/// <summary>
				/// 新版头像框ID
				/// </summary>
				[JsonProperty("n_pid")]
				public long NPid { get; set; } = 0;
			}

			/// <summary>
			/// 勋章信息
			/// </summary>
			[JsonProperty("nameplate")]
			public NameplateData Nameplate { get; set; } = new();

			public class NameplateData
			{
				/// <summary>
				/// 勋章ID
				/// </summary>
				[JsonProperty("nid")]
				public long Nid { get; set; } = 0;

				/// <summary>
				/// 勋章名称
				/// </summary>
				[JsonProperty("name")]
				public string Name { get; set; } = string.Empty;

				/// <summary>
				/// 勋章图标
				/// </summary>
				[JsonProperty("image")]
				public string Image { get; set; } = string.Empty;

				/// <summary>
				/// 勋章图标（小）
				/// </summary>
				[JsonProperty("image_small")]
				public string ImageSmall { get; set; } = string.Empty;

				/// <summary>
				/// 勋章等级
				/// </summary>
				[JsonProperty("level")]
				public string Level { get; set; } = string.Empty;

				/// <summary>
				/// 获取条件
				/// </summary>
				[JsonProperty("condition")]
				public string Condition { get; set; } = string.Empty;
			}

			/// <summary>
			/// 用户荣誉信息
			/// </summary>
			[JsonProperty("user_honour_info")]
			public UserHonourInfoData UserHonourInfo { get; set; } = new();

			public class UserHonourInfoData
			{
				[JsonProperty("mid")]
				public long Mid { get; set; } = 0;

				[JsonProperty("colour")]
				public string Colour { get; set; } = string.Empty;

				[JsonProperty("tags")]
				public List<string> Tags { get; set; } = new List<string>();
			}

			/// <summary>
			/// 直播间信息
			/// </summary>
			[JsonProperty("live_room")]
			public LiveRoomData LiveRoom { get; set; } = new();

			public class LiveRoomData
			{
				/// <summary>
				/// 直播间状态：0 = 无房间，1 = 有房间
				/// </summary>
				[JsonProperty("roomStatus")]
				public int RoomStatus { get; set; } = 0;

				/// <summary>
				/// 直播状态：0 = 未开播，1 = 直播中
				/// </summary>
				[JsonProperty("liveStatus")]
				public int LiveStatus { get; set; } = 0;

				/// <summary>
				/// 直播间网页地址
				/// </summary>
				[JsonProperty("url")]
				public string Url { get; set; } = string.Empty;

				/// <summary>
				/// 直播间标题
				/// </summary>
				[JsonProperty("title")]
				public string Title { get; set; } = string.Empty;

				/// <summary>
				/// 直播间封面地址
				/// </summary>
				[JsonProperty("cover")]
				public string Cover { get; set; } = string.Empty;

				/// <summary>
				/// 直播间ID
				/// </summary>
				[JsonProperty("roomid")]
				public long RoomId { get; set; } = 0;

				/// <summary>
				/// 轮播状态：0 = 未轮播，1 = 轮播
				/// </summary>
				[JsonProperty("roundStatus")]
				public int RoundStatus { get; set; } = 0;

				/// <summary>
				/// 广播类型（0）
				/// </summary>
				[JsonProperty("broadcast_type")]
				public int BroadcastType { get; set; } = 0;

				/// <summary>
				/// 观看信息对象
				/// </summary>
				[JsonProperty("watched_show")]
				public WatchedShowData WatchedShow { get; set; } = new();

				public class WatchedShowData
				{
					/// <summary>
					/// 开关？
					/// </summary>
					[JsonProperty("switch")]
					public bool Switch { get; set; } = false;

					/// <summary>
					/// 总观看人数
					/// </summary>
					[JsonProperty("num")]
					public long Num { get; set; } = 0;

					[JsonProperty("text_small")]
					public string TextSmall { get; set; } = string.Empty;

					[JsonProperty("text_large")]
					public string TextLarge { get; set; } = string.Empty;

					[JsonProperty("icon")]
					public string Icon { get; set; } = string.Empty;

					[JsonProperty("icon_location")]
					public string IconLocation { get; set; } = string.Empty;

					[JsonProperty("icon_web")]
					public string IconWeb { get; set; } = string.Empty;
				}

			}

			/// <summary>
			/// 充电信息
			/// </summary>
			[JsonProperty("elec")]
			public ElecData Elec { get; set; } = new();

			public class ElecData
			{
				[JsonProperty("show_info")]
				public ShowInfoData ShowInfo { get; set; } = new();

				public class ShowInfoData
				{
					/// <summary>
					/// 是否显示充电按钮
					/// </summary>
					[JsonProperty("show")]
					public bool Show { get; set; } = false;

					/// <summary>
					/// 充电状态：
					/// -1 = 未开通，1 = 自定义，2 = 包月自定义，3 = 包月高级自定义
					/// </summary>
					[JsonProperty("state")]
					public int State { get; set; } = -1;

					/// <summary>
					/// 按钮文字（如“充电”）
					/// </summary>
					[JsonProperty("title")]
					public string Title { get; set; } = string.Empty;

					[JsonProperty("icon")]
					public string Icon { get; set; } = string.Empty;

					[JsonProperty("jump_url")]
					public string JumpUrl { get; set; } = string.Empty;
				}
			}

			/// <summary>
			/// 老粉计划显示信息
			/// </summary>
			[JsonProperty("contract")]
			public ContractData Contract { get; set; } = new();

			public class ContractData
			{
				/// <summary>
				/// 是否显示（字段页面中未使用）
				/// </summary>
				[JsonProperty("is_display")]
				public bool IsDisplay { get; set; } = false;

				/// <summary>
				/// 是否显示老粉计划
				/// </summary>
				[JsonProperty("is_follow_display")]
				public bool IsFollowDisplay { get; set; } = false;
			}

		}
	}

}
