using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Данные по созданию шаблона автозаказа
	/// </summary>
	public class CreatingOrderTemplate
	{
		/// <summary>
		/// Дни недели
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public IEnumerable<WeekDayName> Weekdays { get; set; }
		/// <summary>
		/// Интервал повторений
		/// </summary>
		public RepeatOnlineOrderType? RepeatOrder { get; set; }
		/// <summary>
		/// Идентификатор графика доставки
		/// </summary>
		public int? DeliveryScheduleId { get; set; }
	}
}
