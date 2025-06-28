using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Requests.V1
{
	/// <summary>
	/// Запрос создания заказа
	/// </summary>
	public class CreateOrderRequest
	{
		/// <summary>
		/// Идентификатор звонка
		/// </summary>
		[JsonPropertyName("call_id"), Required]
		public Guid CallId { get; set; }

		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		[JsonPropertyName("counterparty_id"), Required]
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор точки доставки
		/// </summary>
		[JsonPropertyName("delivery_point_id"), Required]
		public int DeliveryPointId { get; set; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		[JsonPropertyName("delivery_date")]
		public DateTime? DeliveryDate { get; set; }

		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		[JsonPropertyName("delivery_interval_id")]
		public int? DeliveryIntervalId { get; set; }

		/// <summary>
		/// Подписание документов
		/// </summary>
		[JsonPropertyName("signature_type")]
		public SignatureType? SignatureType { get; set; }

		/// <summary>
		/// Телефон для связи 
		/// </summary>
		[JsonPropertyName("contact_phone")]
		public string ContactPhone { get; set; }

		/// <summary>
		/// Тип оплаты
		/// </summary>
		[JsonPropertyName("payment_type")]
		public PaymentType? PaymentType { get; set; }

		/// <summary>
		/// Сдача с
		/// </summary>
		[JsonPropertyName("trifle")]
		public int? Trifle { get; set; }

		/// <summary>
		/// Комментарий водителю
		/// </summary>
		[JsonPropertyName("driver_app_comment")]
		public string DriverAppComment { get; set; }

		/// <summary>
		/// Отзвон за (в минутых)
		/// </summary>
		[JsonPropertyName("call_before_arrival_minutes")]
		public int? CallBeforeArrivalMinutes { get; set; }

		/// <summary>
		/// Количество бутылей на возврат
		/// </summary>
		[JsonPropertyName("bottles_return")]
		public int? BottlesReturn { get; set; }

		/// <summary>
		/// Причина не возврата тары
		/// </summary>
		[JsonPropertyName("tare_non_return_reason_id")]
		public int? TareNonReturnReasonId { get; set; }

		/// <summary>
		/// Заказываемые товары
		/// </summary>
		[JsonPropertyName("items")]
		public SaleItem[] SaleItems { get; set; }
	}
}
