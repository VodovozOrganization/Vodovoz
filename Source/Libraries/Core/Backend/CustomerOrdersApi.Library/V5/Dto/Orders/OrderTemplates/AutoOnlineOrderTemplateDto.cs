using System;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Sale;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderTemplates
{
	public class AutoOnlineOrderTemplateDto
	{
		/// <summary>
		/// День недели доставки
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WeekDayName Weekday { get; set; }
		/// <summary>
		/// Частота повторения
		/// </summary>
		public RepeatOnlineOrderType RepeatOrder { get; set; }
		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime NextDeliveryDate { get; set; }
	}
}
