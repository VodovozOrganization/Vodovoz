using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
		[JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
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
