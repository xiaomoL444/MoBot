using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoBot.Core.Models.Event.Message;

namespace MoBot.Core.Serialization
{
	internal class MessageEventConvert : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(MessageBase);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// 先读取成 JObject
			var jo = JObject.Load(reader);
			var type = jo["message_type"]?.ToString();

			// 根据 Type 字段反序列化成不同子类
			return type switch
			{

				"group" => jo.ToObject<Group>(serializer),
				_ => jo.ToObject<MessageBase>(new JsonSerializer())
			};
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// 默认序列化行为
			serializer.Serialize(writer, value);
		}
	}
}
