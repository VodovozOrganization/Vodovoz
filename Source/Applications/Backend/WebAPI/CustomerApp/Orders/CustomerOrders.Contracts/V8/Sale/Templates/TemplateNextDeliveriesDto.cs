using System;
using System.Text.Json.Serialization;
using CustomerApp.Contracts.Sale.Templates;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class TemplateNextDeliveriesDto
	{
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }
		
		/// <summary>
		/// День недели
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DayOfWeek WeekDay { get; set; }
		
		/// <summary>
		/// Интервал доставки
		/// </summary>
		public DeliveryScheduleDto DeliveryInterval { get; set; }

		public static TemplateNextDeliveriesDto Create(
			DateTime deliveryDate,
			DayOfWeek weekDay,
			DeliveryScheduleDto deliveryInterval)
		{
			return new TemplateNextDeliveriesDto
			{
				DeliveryDate = deliveryDate,
				WeekDay = weekDay,
				DeliveryInterval = deliveryInterval
			};
		}
	}
}
