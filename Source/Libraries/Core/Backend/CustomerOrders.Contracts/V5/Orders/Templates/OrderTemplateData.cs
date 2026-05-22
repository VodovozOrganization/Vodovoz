using System.Collections.Generic;

namespace CustomerOrders.Contracts.V5.Orders.Templates
{
	/// <summary>
	/// Информация о шаблоне автозаказа
	/// </summary>
	public class OrderTemplateData
	{
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int OrderTemplateId { get; set; }
		/// <summary>
		/// Активен шаблон
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string DeliveryAddress { get; set; }
		/// <summary>
		/// Интервал доставки в формате с 07:00 до 16:00
		/// </summary>
		public string DeliverySchedule { get; set; }
		/// <summary>
		/// Дни недели
		/// </summary>
		public IEnumerable<string> Weekdays { get; set; }
		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public string DeliveryFrequency { get; set; }

		public static OrderTemplateData Create(
			int orderTemplateId,
			bool isActive,
			string deliveryAddress,
			string deliverySchedule,
			IEnumerable<string> weekdays,
			string deliveryFrequency)
		{
			return new OrderTemplateData
			{
				OrderTemplateId = orderTemplateId,
				IsActive = isActive,
				DeliveryAddress = deliveryAddress,
				DeliverySchedule = deliverySchedule,
				Weekdays = weekdays,
				DeliveryFrequency = deliveryFrequency
			};
		}
	}
}
