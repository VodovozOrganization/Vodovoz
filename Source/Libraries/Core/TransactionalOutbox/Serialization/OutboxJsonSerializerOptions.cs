using System.Text.Unicode;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransactionalOutbox.Serialization
{
	/// <summary>
	/// Настройки сериализации JSON для сообщений Outbox.
	/// </summary>
	public static class OutboxJsonSerializerOptions
	{
		public static readonly JsonSerializerOptions Instance = Create();

		private static JsonSerializerOptions Create()
		{
			return new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				WriteIndented = false,
				Converters ={new JsonStringEnumConverter() },
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};
		}
	}
}
