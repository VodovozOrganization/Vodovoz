using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.V2.DTO
{
	public class DeliveryRulesDTO
	{
		[JsonPropertyOrder(1)]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeliveryRulesResponseStatus Status { get; set; }

		[JsonPropertyOrder(0)]
		public string Message { get; set; }

		[JsonPropertyOrder(2)]
		public IList<WeekDayDeliveryRuleDTO> WeekDayDeliveryRules { get; set; }
	}
}
