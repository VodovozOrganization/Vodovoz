using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto.Responses
{
	public class MailganerResponseBase
	{
		/// <summary>
		/// Статус запроса (например, "ok" при успешном выполнении)
		/// </summary>
		[JsonPropertyName("status")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public MailganerResponseStatusTypeDto Status { get; set; }

		/// <summary>
		/// Сообщение об ошибке, если запрос не был успешным
		/// </summary>
		[JsonPropertyName("message")]
		public string ErrorMessage { get; set; }
	}
}
