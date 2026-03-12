using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Response
{
	[Serializable]
	public class ResponseDto
	{
		/// <summary>
		/// true - запрос успешно обработан, false - произошла ошибка
		/// </summary>
		[JsonPropertyName("ok")]
		public bool Ok { get; set; }
		/// <summary>
		/// Тело ответа, если нет ошибки
		/// </summary>
		[JsonPropertyName("result")]
		public RequestStatus Result { get; set; }
		/// <summary>
		/// Описание ошибки
		/// </summary>
		[JsonPropertyName("error")]
		public string Error { get; set; }
	}
}
