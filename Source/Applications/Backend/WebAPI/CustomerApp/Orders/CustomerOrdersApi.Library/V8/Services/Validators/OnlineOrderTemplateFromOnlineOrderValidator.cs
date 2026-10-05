using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Abstractions.V8.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Errors.Sale;

namespace CustomerOrdersApi.Library.V8.Services.Validators
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
		
		public Result Validate(IUnitOfWork uow, ICanCreateOnlineOrderTemplate canCreateTemplate)
		{
			_validationResults = new List<Error>();

			if(canCreateTemplate.IsSelfDelivery)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateForSelfDelivery);
			}

			if(canCreateTemplate.ErpCounterpartyId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyCounterparty);
			}

			if(canCreateTemplate.DeliveryPointId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyDeliveryPoint);
			}

			var promoSetIds = canCreateTemplate.CartItems
				.Where(x => x.ItemType == SaleItemType.PromoSet)
				.Select(x => x.ErpId)
				.ToArray();

			if(promoSetIds.Any())
			{
				if(_promotionalSetRepository.HasPromoSetsForNewClients(uow, promoSetIds))
				{
					_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithPromosetForNewClients);
				}
			}

			if(canCreateTemplate.CartItems.Any(x => x.ItemType == SaleItemType.RentPackage))
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithFreeRentPackages);
			}
			
			return !_validationResults.Any() ? Result.Success() : Result.Failure(_validationResults);
		}
	}
}
