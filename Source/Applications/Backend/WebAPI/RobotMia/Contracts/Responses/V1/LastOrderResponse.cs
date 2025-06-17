using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Ответ на хапрос прошлого заказа
	/// </summary>
	public class LastOrderResponse
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int id { get; set; }

		/// <summary>
		/// Идентификатор точки доставки
		/// </summary>
		[JsonPropertyName("delivery_point_id")]
		public int DeliveryPointId { get; set; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		[JsonPropertyName("delivery_date")]
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Товары на продажу
		/// </summary>
		[JsonPropertyName("items")]
		public IEnumerable<OrderSaleItemDto> OrderItems { get; set; }
	}

}
