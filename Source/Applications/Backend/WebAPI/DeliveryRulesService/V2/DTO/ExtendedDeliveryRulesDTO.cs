using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DeliveryRulesService.Constants;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.V2.DTO
{
	public class ExtendedDeliveryRulesDto
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DeliveryRulesResponseStatus Status { get; set; }
		public string Message { get; set; }
		public PaidDeliveryDto PaidDelivery { get; set; }
		public FastDeliveryDto FastDelivery { get; set; }
		public IList<ExtendedWeekDayDeliveryRuleDto> WeekDayDeliveryRules { get; set; }

		public void SetErrorState()
		{
			Status = DeliveryRulesResponseStatus.Error;
			WeekDayDeliveryRules = null;
			Message = ServiceConstants.InternalErrorFromGetDeliveryRule;
		}
		
		public void RuleNotFoundState(string message)
		{
			Status = DeliveryRulesResponseStatus.RuleNotFound;
			WeekDayDeliveryRules = null;
			Message = message;
		}

		public void AddFastDelivery(
			int fastDeliveryId,
			decimal fastDeliveryPrice,
			string fastDeliveryName,
			int fastDeliveryScheduleId,
			string fastDeliveryScheduleName
			)
		{
			var todayInfo = WeekDayDeliveryRules.Single(x => x.WeekDay == WeekDayName.Today);
			FastDelivery = FastDeliveryDto.Create(fastDeliveryId, fastDeliveryName, fastDeliveryPrice);

			(todayInfo.DeliveryIntervals as IList<ScheduleRestrictionDto>).Insert(
				0,
				ScheduleRestrictionDto.Create(fastDeliveryScheduleId, fastDeliveryScheduleName));
		}
	}
}
