using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V5.Carts;
using CustomerOrders.Contracts.V5.Orders.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Errors.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OnlineOrderTemplateFromOnlineOrderValidator : IOnlineOrderTemplateFromOnlineOrderValidator
	{
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private IList<Error> _validationResults;
		
		public OnlineOrderTemplateFromOnlineOrderValidator(
			IPromotionalSetRepository promotionalSetRepository
			)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
		}
		
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
		
		public Result Validate(IUnitOfWork uow, CheckUsersBasketRequest checkRequest)
		{
			_validationResults = new List<Error>();

			if(checkRequest.IsSelfDelivery)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateForSelfDelivery);
			}

			if(checkRequest.CounterpartyErpId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyCounterparty);
			}

			if(checkRequest.DeliveryPointId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyDeliveryPoint);
			}

			var promoSetIds = checkRequest.OnlineOrderItems
				.Where(x => x.PromoSetId != null)
				.Select(x => x.PromoSetId.Value)
				.ToArray();

			if(promoSetIds.Any())
			{
				if(_promotionalSetRepository.HasPromoSetsForNewClients(uow, promoSetIds))
				{
					_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithPromosetForNewClients);
				}
			}

			if(checkRequest.OnlineRentPackages.Any())
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithFreeRentPackages);
			}
			
			return !_validationResults.Any() ? Result.Success() : Result.Failure(_validationResults);
		}
	}
}
