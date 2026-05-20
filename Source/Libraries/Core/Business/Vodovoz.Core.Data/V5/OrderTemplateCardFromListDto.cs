using System;

namespace Vodovoz.Core.Data.V5
{
	/// <summary>
	/// Информация для карточки шаблона автозаказа из списка
	/// </summary>
	public class OrderTemplateCardFromListDto : OrderTemplateData
	{
		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime? NextDeliveryDate { get; set; }
		
		public static OrderTemplateCardFromListDto Create(
			OrderTemplateData template,
			DateTime? nextDeliveryDate
			)
		{
			return new OrderTemplateCardFromListDto
			{
				OrderTemplateId = template.OrderTemplateId,
				IsActive = template.IsActive,
				DeliveryAddress = template.DeliveryAddress,
				Weekdays = template.Weekdays,
				DeliverySchedule = template.DeliverySchedule,
				DeliveryFrequency = template.DeliveryFrequency,
				NextDeliveryDate = nextDeliveryDate
			};
		}
	}
}
