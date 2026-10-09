using System;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Параметры доставки автозаказа
	/// </summary>
	public class TemplateDeliveryScheduleDto : TemplateDeliveryScheduleBaseDto
	{
		/// <summary>
		/// Интервал доставки
		/// </summary>
		public int DeliveryScheduleId { get; set; }
	}
}
