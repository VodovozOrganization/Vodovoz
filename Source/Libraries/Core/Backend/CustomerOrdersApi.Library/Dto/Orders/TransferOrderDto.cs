using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// DTO для запроса на перенос заказа
	/// </summary>
	public class TransferOrderDto
	{
		/// <summary>
		/// Источник заказа (id ИПЗ)
		/// </summary>
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public Source Source { get; set; }

		/// <summary>
		/// ID заказа из ИПЗ
		/// </summary>
		[Required]
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Новая дата доставки
		/// </summary>
		[Required]
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// ID нового интервала доставки
		/// </summary>
		[Required]
		public int DeliveryScheduleId { get; set; }

		/// <summary>
		/// Контрольная сумма (подпись)
		/// </summary>
		[Required]
		public string Signature { get; set; }
	}
}
