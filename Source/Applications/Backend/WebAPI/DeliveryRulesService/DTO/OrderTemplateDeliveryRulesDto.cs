using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace DeliveryRulesService.DTO
{
	/// <summary>
	/// Информация по доступным интервалам доставки с частотой автозаказа
	/// </summary>
	[Serializable]
	public class OrderTemplateDeliveryRulesDto
	{
		/// <summary>
		/// Список интервалов по дням
		/// </summary>
		public IList<OrderTemplateDeliveryRuleDto> ScheduleRestrictions { get; set; }
		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public OnlineOrderDeliveryFrequency[] DeliveryFrequency { get; set; }
		
		public static OrderTemplateDeliveryRulesDto Create(IList<OrderTemplateDeliveryRuleDto> scheduleRestrictions)
		{
			return new OrderTemplateDeliveryRulesDto
			{
				ScheduleRestrictions = scheduleRestrictions,
				DeliveryFrequency = Enum.GetValues<OnlineOrderDeliveryFrequency>()
			};
		}
	}
}
