using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryRulesDTO
	{
		private DeliveryRulesResponseStatus statusEnum;
		[JsonIgnore]
		public DeliveryRulesResponseStatus StatusEnum
		{
			get => statusEnum;
			set
			{
				statusEnum = value;
				Status = statusEnum.ToString();
			}
		}

		[JsonPropertyOrder(1)]
		public string Status { get; set; }

		[JsonPropertyOrder(0)]
		public string Message { get; set; }

		[JsonPropertyOrder(2)]
		public IList<WeekDayDeliveryRuleDTO> WeekDayDeliveryRules { get; set; }
	}
}
