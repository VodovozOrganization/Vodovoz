using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// DTO для запроса на перенос заказа
	/// </summary>
	/// <param name="Source"> Источник заказа (id ИПЗ) </param>
	/// <param name="ExternalOrderId"> ID заказа из ИПЗ </param>
	/// <param name="DeliveryDate"> Новая дата доставки </param>
	/// <param name="DeliveryScheduleId"> ID нового интервала доставки </param>
	/// <param name="Signature"> Контрольная сумма (подпись) </param>
	public record TransferOrderDto(
		[property: Required][property: JsonConverter(typeof(JsonStringEnumConverter))] Source Source,
		[property: Required] Guid ExternalOrderId,
		[property: Required] DateTime DeliveryDate,
		[property: Required] int DeliveryScheduleId,
		[property: Required] string Signature);
}
