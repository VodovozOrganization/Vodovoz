using System;
using System.Text.Json.Serialization;

namespace YandexPayApi.Library.Models
{
	/// <summary>
	/// Модель транзакции из Яндекс Пэй
	/// </summary>
	public class YandexPayTransaction
	{
		/// <summary>
		/// ID операции в Яндекс Пэй
		/// </summary>
		[JsonPropertyName("operationId")]
		public string OperationId { get; set; }

		/// <summary>
		/// Тип операции
		/// </summary>
		[JsonPropertyName("operationType")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public YandexPayOperationType OperationType { get; set; }

		/// <summary>
		/// ID заказа
		/// </summary>
		[JsonPropertyName("orderId")]
		public string OrderId { get; set; }

		/// <summary>
		/// Сумма операции
		/// </summary>
		[JsonPropertyName("amount")]
		public string Amount { get; set; }

		/// <summary>
		/// Статус операции
		/// </summary>
		[JsonPropertyName("status")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public YandexPayOperationStatus Status { get; set; }

		/// <summary>
		/// Дата создания
		/// </summary>
		[JsonPropertyName("created")]
		public DateTime Created { get; set; }

		/// <summary>
		/// Дата обновления
		/// </summary>
		[JsonPropertyName("updated")]
		public DateTime Updated { get; set; }
	}
}
