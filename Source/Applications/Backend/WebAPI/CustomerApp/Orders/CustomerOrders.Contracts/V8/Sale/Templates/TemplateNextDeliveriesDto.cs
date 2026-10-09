using System;
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
		public DayOfWeek WeekDay { get; set; }
		
		/// <summary>
		/// Интервал доставки
		/// </summary>
		public DeliveryScheduleDto DeliveryInterval { get; set; }
	}
}
