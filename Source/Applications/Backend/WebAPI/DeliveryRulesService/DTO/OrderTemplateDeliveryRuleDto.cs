using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Sale;

namespace DeliveryRulesService.DTO
{
	/// <summary>
	/// Доступные графики доставки по дням
	/// </summary>
	[Serializable]
	public class OrderTemplateDeliveryRuleDto
	{
		/// <summary>
		/// День недели
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WeekDayName Weekday { get; set; }
		/// <summary>
		/// Список интервалов доставки
		/// </summary>
		public IList<ExtendedScheduleRestrictionDto> ScheduleRestrictions { get; set; }

		public static OrderTemplateDeliveryRuleDto Create(
			WeekDayName weekday,
			IList<ExtendedScheduleRestrictionDto> scheduleRestrictions) => new OrderTemplateDeliveryRuleDto
		{
			Weekday = weekday,
			ScheduleRestrictions = scheduleRestrictions,
		};
	}
}
