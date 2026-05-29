using System.Collections.Generic;
using System.Text.Json.Serialization;
using YandexPayApi.Library.Models;

namespace YandexPayApi.Library.Responses
{
	/// <summary>
	/// Ответ на запрос информации о заказе
	/// </summary>
	public class YandexPayOrderResponse
	{
		/// <summary>
		/// Данные заказа
		/// </summary>
		[JsonPropertyName("order")]
		public YandexPayOrder Order { get; set; }

		/// <summary>
		/// Список операций по заказу
		/// </summary>
		[JsonPropertyName("operations")]
		public List<YandexPayTransaction> Operations { get; set; }
	}
}
