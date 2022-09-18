using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryRulesDTO
	{
		private DeliveryRulesResponseStatus statusEnum;
		[JsonIgnore]
		public DeliveryRulesResponseStatus StatusEnum {
			get => statusEnum;
			set {
				statusEnum = value;
				Status = statusEnum.ToString();
			}
		}

		[JsonInclude]
		public string Status { get; set; }

		[JsonInclude]
		public string Message { get; set; }

		[JsonInclude]
		public IList<WeekDayDeliveryRuleDTO> WeekDayDeliveryRules { get; set; }
	}
}
