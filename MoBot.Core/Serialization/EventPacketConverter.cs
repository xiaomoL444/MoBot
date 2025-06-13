using MoBot.Core.Models.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class EventPacketConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(EventPacketBase);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		// 先读取成 JObject
		var jo = JObject.Load(reader);
		var type = jo["post_type"]?.ToString();

		// 根据 Type 字段反序列化成不同子类
		return type switch
		{

			"message" => jo.ToObject<EventPacketBase>(serializer),
			"message_sent" => jo.ToObject<EventPacketBase>(serializer),
			"request" => jo.ToObject<EventPacketBase>(serializer),
			"notice" => jo.ToObject<EventPacketBase>(serializer),
			"meta_event" => jo.ToObject<MetaEventType>(serializer),
			_ => jo.ToObject<EventPacketBase>(serializer)
		};
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		// 默认序列化行为
		serializer.Serialize(writer, value);
	}
}
