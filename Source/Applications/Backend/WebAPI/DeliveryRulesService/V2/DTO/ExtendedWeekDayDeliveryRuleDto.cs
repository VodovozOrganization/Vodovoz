using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Данные по доставке
	/// </summary>
	public class ExtendedWeekDayDeliveryRuleDto
	{
		/// <summary>
		/// День недели
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WeekDayName WeekDay { get; set; }
		/// <summary>
		/// Платная доставка
		/// </summary>
		public decimal? PaidDeliveryPrice { get; set; }
		/// <summary>
		/// Список интервалов
		/// </summary>
		public IEnumerable<ScheduleRestrictionDto> DeliveryIntervals { get; set; }
		/// <summary>
		/// Информационные сообщения
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; set; }

		public static ExtendedWeekDayDeliveryRuleDto Create(
			WeekDayName weekDay,
			IEnumerable<ScheduleRestrictionDto> deliveryIntervals,
			decimal? paidDeliveryPrice = null,
			IEnumerable<InfoMessage> infoMessages = null
		) =>
			new ExtendedWeekDayDeliveryRuleDto
			{
				WeekDay = weekDay,
				DeliveryIntervals = deliveryIntervals,
				PaidDeliveryPrice = paidDeliveryPrice,
				InfoMessages = infoMessages
			};
		
		public static ExtendedWeekDayDeliveryRuleDto Create(
			WeekDayName weekDay,
			IEnumerable<ScheduleRestrictionDto> deliveryIntervals,
			decimal? paidDeliveryPrice = null,
			InfoMessage infoMessage = null
		) =>
			new ExtendedWeekDayDeliveryRuleDto
			{
				WeekDay = weekDay,
				DeliveryIntervals = deliveryIntervals,
				PaidDeliveryPrice = paidDeliveryPrice,
				InfoMessages = infoMessage is null ? null : new []{ infoMessage }
			};
	}
}
