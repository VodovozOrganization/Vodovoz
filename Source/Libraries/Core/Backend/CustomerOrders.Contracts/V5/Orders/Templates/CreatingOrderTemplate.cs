using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.Orders.Templates
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
		public IEnumerable<ExternalWeekDayName> Weekdays { get; set; }
		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public ExternalOnlineOrderDeliveryFrequency? DeliveryFrequency { get; set; }
		/// <summary>
		/// Идентификатор графика доставки
		/// </summary>
		public int? DeliveryScheduleId { get; set; }
	}
}
