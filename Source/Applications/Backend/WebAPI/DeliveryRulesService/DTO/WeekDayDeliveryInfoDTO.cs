using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Sale;
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
			set
			{
				weekDayEnum = value;
				WeekDay = weekDayEnum.ToString();
			}
		}

		[JsonPropertyOrder(2)]
		public string WeekDay { get; set; }
		
		[JsonPropertyOrder(0)]
		public IList<DeliveryRuleDTO> DeliveryRules { get; set; }
		
		[JsonPropertyOrder(1)]
		public IList<string> ScheduleRestrictions { get; set; }
	}
}
