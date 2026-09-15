using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.DTO
{
	public class ExtendedWeekDayDeliveryRuleDto
	{
		private WeekDayName _weekDayEnum;

		[JsonIgnore]
		public WeekDayName WeekDayEnum
		{
			get => _weekDayEnum;
			set
			{
				_weekDayEnum = value;
				WeekDay = _weekDayEnum.ToString();
			}
		}

		public IList<string> DeliveryRules { get; set; }
		public IList<ExtendedScheduleRestrictionDto> ScheduleRestrictions { get; set; }
		public string WeekDay { get; set; }
	}
}
