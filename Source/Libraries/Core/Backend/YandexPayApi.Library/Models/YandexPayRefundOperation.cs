using System.Text.Json.Serialization;

namespace YandexPayApi.Library.Models
{
	/// <summary>
	/// Операция возврата Yandex Pay
	/// </summary>
	public class YandexPayRefundOperation
	{
		/// <summary>
		/// Идентификатор операции в Yandex Pay
		/// </summary>
		[JsonPropertyName("operationId")]
		public string OperationId { get; set; }

		/// <summary>
		/// Статус операции возврата
		/// </summary>
		[JsonPropertyName("status")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public YandexPayOperationStatus Status { get; set; }

		/// <summary>
		/// Внешний идентификатор операции (в системе магазина)
		/// </summary>
		[JsonPropertyName("externalOperationId")]
		public string ExternalOperationId { get; set; }
	}
}
