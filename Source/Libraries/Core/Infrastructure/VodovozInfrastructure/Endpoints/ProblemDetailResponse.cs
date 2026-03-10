using System.Text.Json.Serialization;

namespace VodovozInfrastructure.Endpoints
{
	/// <summary>
	/// Упрощённая модель для десериализации ProblemDetails (RFC 7807) из ответов API	
	/// </summary>
	public class ProblemDetailResponse
	{
		/// <summary>
		/// Краткое название проблемы	
		/// </summary>
		[JsonPropertyName("title")]
		public string Title { get; set; }

		/// <summary>
		/// HTTP-статус-код ошибки
		/// </summary>
		[JsonPropertyName("status")]
		public int? Status { get; set; }

		/// <summary>
		/// Основное сообщение об ошибке для пользователя
		/// </summary>
		[JsonPropertyName("detail")]
		public string Detail { get; set; }

		/// <summary>
		/// Возвращает сообщение для показа пользователю
		/// </summary>
		public string GetMessage()
		{
			if(!string.IsNullOrWhiteSpace(Detail))
			{
				return Detail;
			}

			if(!string.IsNullOrWhiteSpace(Title))
			{
				return Title;
			}

			return Status.HasValue ? $"Ошибка {Status.Value}" : "Неизвестная ошибка";
		}
	}
}
