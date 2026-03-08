using System;

namespace CustomerApps.Contracts.V5
{
	/// <summary>
	/// Информация для карточки шаблона автозаказа из списка
	/// </summary>
	public class OrderTemplateCardFromListDto : OrderTemplateData
	{
		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime NextDeliveryDate { get; set; }
		
		public static OrderTemplateCardFromListDto Create(
			OrderTemplateData template,
			DateTime nextDeliveryDate
			)
		{
			return new OrderTemplateCardFromListDto
			{
				OrderTemplateId = template.OrderTemplateId,
				IsActive = template.IsActive,
				DeliveryAddress = template.DeliveryAddress,
				Weekdays = template.Weekdays,
				DeliverySchedule = template.DeliverySchedule,
				RepeatOrder = template.RepeatOrder,
				NextDeliveryDate = nextDeliveryDate
			};
		}
	}
}
