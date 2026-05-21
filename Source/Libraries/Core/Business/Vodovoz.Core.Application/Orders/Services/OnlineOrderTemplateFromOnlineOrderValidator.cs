using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Errors.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderTemplateFromOnlineOrderValidator : IOnlineOrderTemplateFromOnlineOrderValidator
	{
		private IList<Error> _validationResults;
		
		public Result Validate(OnlineOrder onlineOrder, CreatingOrderTemplate creatingTemplate)
		{
			if(creatingTemplate is null)
			{
				return Result.Success();
			}
			
			_validationResults = new List<Error>();

			if(onlineOrder.IsSelfDelivery)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateForSelfDelivery);
			}

			if(onlineOrder.Counterparty is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyCounterparty);
			}

			if(onlineOrder.DeliveryPoint is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyDeliveryPoint);
			}

			if(creatingTemplate.DeliveryScheduleId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyDeliverySchedule);
			}
			
			if(onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateForPaidOnline);
			}
			
			if(!creatingTemplate.DeliveryFrequency.HasValue)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyDeliveryFrequency);
			}
			
			if(creatingTemplate.Weekdays is null || !creatingTemplate.Weekdays.Any())
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyWeekdays);
			}

			if(onlineOrder.OnlineOrderItems
				.Any(onlineOrderItem => onlineOrderItem.PromoSet != null && onlineOrderItem.PromoSet.PromotionalSetForNewClients))
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithPromosetForNewClients);
			}

			if(onlineOrder.OnlineRentPackages.Any())
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithFreeRentPackages);
			}
			
			return !_validationResults.Any() ? Result.Success() : Result.Failure(_validationResults);
		}
	}
}
