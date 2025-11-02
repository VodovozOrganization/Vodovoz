using DateTimeHelpers;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsApi.Library.Converters
{
	public class DateTimeJsonConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Debug.Assert(typeToConvert == typeof(DateTime));
			return DateTime.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString(DateTimeFormats.QueryDateTimeFormat));
		}
	}
}
