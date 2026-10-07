using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale;
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
			
			if(canCreateTemplate.IsFastDelivery)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateForFastDelivery);
			}

			if(canCreateTemplate.ErpCounterpartyId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyCounterparty);
			}

			if(canCreateTemplate.DeliveryPointId is null)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.IsEmptyDeliveryPoint);
			}

			var hasEquipments = false;
			var hasServices = false;
			var hasRentPackages = false;
			var promoSetIds = new List<int>();
			
			foreach(var cartItem in canCreateTemplate.CartItems)
			{
				if(cartItem.ItemType == SaleItemType.Service)
				{
					hasServices = true;
				}
				
				if(cartItem.ItemType == SaleItemType.Equipment)
				{
					hasEquipments = true;
				}

				if(cartItem.ItemType == SaleItemType.RentPackage)
				{
					hasRentPackages = true;
				}

				if(cartItem.ItemType == SaleItemType.PromoSet)
				{
					promoSetIds.Add(cartItem.ErpId);
				}
			}

			if(promoSetIds.Any())
			{
				if(_promotionalSetRepository.HasPromoSetsForNewClients(uow, promoSetIds))
				{
					_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithPromosetForNewClients);
				}
			}

			if(hasServices)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithServices);
			}

			if(hasEquipments || hasRentPackages)
			{
				_validationResults.Add(OnlineOrderTemplateErrors.CantCreateWithEquipmentsOrFreeRentPackages);
			}
			
			return !_validationResults.Any() ? Result.Success() : Result.Failure(_validationResults);
		}
	}
}
