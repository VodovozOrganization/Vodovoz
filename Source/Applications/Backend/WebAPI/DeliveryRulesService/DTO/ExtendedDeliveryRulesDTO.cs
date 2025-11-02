using DeliveryRulesService.Constants;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class ExtendedDeliveryRulesDto
	{
		private DeliveryRulesResponseStatus _statusEnum;

		[JsonIgnore]
		public DeliveryRulesResponseStatus StatusEnum
		{
			get => _statusEnum;
			set
			{
				_statusEnum = value;
				Status = _statusEnum.ToString();
			}
		}

		public string Message { get; set; }
		public string Status { get; set; }
		public int? PaidDeliveryId { get; set; }
		public int? FastDeliveryId { get; set; }
		public decimal? FastDeliveryPrice { get; set; }
		public IList<ExtendedWeekDayDeliveryRuleDto> WeekDayDeliveryRules { get; set; }

		public void SetErrorState()
		{
			StatusEnum = DeliveryRulesResponseStatus.Error;
			WeekDayDeliveryRules = null;
			Message = ServiceConstants.InternalErrorFromGetDeliveryRule;
		}
		
		public void RuleNotFoundState(string message)
		{
			StatusEnum = DeliveryRulesResponseStatus.RuleNotFound;
			WeekDayDeliveryRules = null;
			Message = message;
		}
	}
}
