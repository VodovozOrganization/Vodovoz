using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.DTO
{
	public class WeekDayDeliveryInfoDTO
	{
		private WeekDayName weekDayEnum;
		[JsonIgnore]
		public WeekDayName WeekDayEnum
		{
			get => weekDayEnum;
			set {
				weekDayEnum = value;
				WeekDay = weekDayEnum.ToString();
			}
		}

		[JsonInclude]
		public string WeekDay { get; set; }
		
		[JsonInclude]
		public IList<DeliveryRuleDTO> DeliveryRules { get; set; }
		
		[JsonInclude]
		public IList<string> ScheduleRestrictions { get; set; }
	}
}
