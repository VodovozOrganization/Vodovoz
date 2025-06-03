using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Requests.V1
{
	/// <summary>
	/// Запрос на вычисление цены заказа
	/// </summary>
	public class CalculatePriceRequest
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
		[JsonPropertyName("delivery_date"), Required]
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		[JsonPropertyName("delivery_interval_id"), Required]
		public int DeliveryIntervalId { get; set; }

		/// <summary>
		/// Заказываемые товары
		/// </summary>
		[JsonPropertyName("items"), Required]
		public SaleItem[] OrderSaleItems { get; set; }

		/// <summary>
		/// Количество бутылей на возврат
		/// </summary>
		[JsonPropertyName("bottles_return"), Required]
		public int BottlesReturn { get; set; }
	}
}
