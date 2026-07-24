using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Ответ API ВАТС при превышении лимита количества запросов.
	/// Отличается по составу полей от остальных ответов и не содержит кода результата
	/// </summary>
	public class VpbxErrorResponse
	{
		/// <summary>
		/// Название ошибки, например "Service Unavailable"
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Описание ошибки, например "Rate limit exceeded."
		/// </summary>
		[JsonPropertyName("message")]
		public string Message { get; set; }

		/// <summary>
		/// Код ошибки
		/// </summary>
		[JsonPropertyName("code")]
		public int? Code { get; set; }

		/// <summary>
		/// HTTP-статус, продублированный в теле ответа
		/// </summary>
		[JsonPropertyName("status")]
		public int? Status { get; set; }
	}
}
