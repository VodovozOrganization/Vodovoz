using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.JsonConverters
{
	/// <summary>
	/// Конвертер словаря, который вместо пустого объекта может приходить в виде пустого массива.
	/// Битрикс24 (PHP) сериализует пустые ассоциативные массивы как [], а непустые - как объект,
	/// поэтому обычная десериализация словаря на пустых данных падает
	/// </summary>
	public class EmptyArrayAsEmptyDictionaryConverter<TValue> : JsonConverter<Dictionary<string, TValue>>
	{
		public override Dictionary<string, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if(reader.TokenType == JsonTokenType.StartArray)
			{
				reader.Skip();
				return new Dictionary<string, TValue>();
			}

			return JsonSerializer.Deserialize<Dictionary<string, TValue>>(ref reader, options);
		}

		public override void Write(Utf8JsonWriter writer, Dictionary<string, TValue> value, JsonSerializerOptions options)
		{
			JsonSerializer.Serialize(writer, value, options);
		}
	}
}
