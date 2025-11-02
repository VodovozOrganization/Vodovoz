using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Уведомление об изменении статуса оплаты МП или сайта
	/// </summary>
	public class FastPaymentStatusChangeNotificationDto
	{
		/// <summary>
		/// Тип сообщения
		/// </summary>
		[JsonPropertyName("type")]
		public string MessageType => "notification";
		/// <summary>
		/// Статус оплаты
		/// </summary>
		[JsonPropertyName("event")]
		public PaymentStatusNotification PaymentStatus { get; set; }
		/// <summary>
		/// Детали оплаты
		/// </summary>
		[JsonPropertyName("object")]
		public OnlinePaymentDetailsDto PaymentDetails { get; set; }
	}
}
