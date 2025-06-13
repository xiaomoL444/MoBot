using MoBot.Core.Models.Event;
using MoBot.Core.Models.Event.Message;
using MoBot.Core.Serialization;
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

			"message" => jo.ToObject<MessageBase>(JsonSerializer.Create(new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new MessageEventConvert() } })),
			"meta_event" => jo.ToObject<MetaEventTypeBase>(serializer),
			_ => jo.ToObject<EventPacketBase>(new JsonSerializer())
		};
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		// 默认序列化行为
		serializer.Serialize(writer, value);
	}
}
