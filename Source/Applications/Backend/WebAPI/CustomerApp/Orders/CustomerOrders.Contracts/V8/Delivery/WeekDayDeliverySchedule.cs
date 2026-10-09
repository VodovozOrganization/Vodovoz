using System;
using System.Text.Json.Serialization;
using CustomerApp.Contracts.Sale.Templates;

namespace CustomerOrders.Contracts.V8.Delivery
{
	/// <summary>
	/// График доставки по дню
	/// </summary>
	public class WeekDayDeliverySchedule
	{
		/// <summary>
		/// День недели
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DayOfWeek WeekDay { get; set; }
		/// <summary>
		/// График доставки
		/// </summary>
		public DeliveryScheduleDto Schedule { get; set; }
	}
}
