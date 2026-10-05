using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Abstractions.V7.Carts;
using CustomerOrders.Abstractions.V7.Sale;
using CustomerOrders.Contracts.V7.Orders.Items;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Errors.Orders;

namespace CustomerOrdersApi.Library.V7.Services
{
	internal class OnlineOrderFixedPriceHandler : FixedPriceHandler, IOnlineOrderFixedPriceHandler
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IOnlineOrderDiscountHandler _discountHandler;

		public OnlineOrderFixedPriceHandler(
			IGenericRepository<NomenclatureFixedPrice> nomenclatureFixedPriceRepository,
			IDiscountReasonRepository discountReasonRepository,
			IOnlineOrderDiscountHandler discountHandler
			) : base(nomenclatureFixedPriceRepository)
		{
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_discountHandler = discountHandler ?? throw new ArgumentNullException(nameof(discountHandler));
		}
		
		public (bool? AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems) TryApplyFixedPrice(
			IUnitOfWork uow,
			IApplyingFixedPrice receivedData)
		{
			HasFixedPrices(
				uow,
				receivedData.ErpCounterpartyId,
				receivedData.ErpDeliveryPointId,
				receivedData.IsSelfDelivery,
				out var fixedPrices);

			return TryApplyFixedPrice(uow, receivedData.OnlineOrderItems, fixedPrices);
		}
		
		private (bool? AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems) TryApplyFixedPrice(
			IUnitOfWork uow,
			IEnumerable<IOrderedCartItem> cartItems,
			IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			var cartItemsWithDiscountDetails = new List<IOrderedCartItemWithDiscountDetails>();
			bool? fixedPriceAppliedToAllItems = null;
			
			foreach(var cartItem in cartItems)
			{
				var cartItemWithDiscountDetails = OnlineOrderItemWithDiscountDetailsDto.Create(cartItem);
				var applied = false;
				
				var discountReasons = _discountReasonRepository.GetDiscountReasons(
					uow,
					cartItem.DiscountIds ?? new List<int>());

				if(!fixedPrices.Any())
				{
					_discountHandler.CalculateDiscount(cartItemWithDiscountDetails, discountReasons);
				}
				else
				{
					foreach(var fixedPrice in fixedPrices)
					{
						if(!CanApplyFixedPriceV7(cartItemWithDiscountDetails, fixedPrice, discountReasons))
						{
							_discountHandler.CalculateDiscount(cartItemWithDiscountDetails, discountReasons);
							continue;
						}

						ApplyFixedPrice(cartItemWithDiscountDetails, discountReasons, fixedPrice.Price);
						applied = true;
						break;
					}
				}

				if(fixedPriceAppliedToAllItems is null)
				{
					fixedPriceAppliedToAllItems = applied
						? true
						: null;
				}
				else
				{
					fixedPriceAppliedToAllItems &= applied;
				}
				
				cartItemsWithDiscountDetails.Add(cartItemWithDiscountDetails);
			}
			
			return (fixedPriceAppliedToAllItems, cartItemsWithDiscountDetails);
		}

		private void ApplyFixedPrice(
			IOrderedCartItemWithDiscountDetails cartItemWithDiscountDetails,
			IEnumerable<DiscountReasonBase> discountReasons,
			decimal fixedPrice
			)
		{
			cartItemWithDiscountDetails.AddFixedPrice(fixedPrice);
			_discountHandler.CalculateDiscount(cartItemWithDiscountDetails, discountReasons);
		}

		private bool CanApplyFixedPriceV7(
			IOrderedCartItemWithDiscountDetails cartItem,
			NomenclatureFixedPrice fixedPrice,
			IEnumerable<DiscountReasonBase> discountReasons)
		{
			if(!IsApplicable(cartItem, discountReasons, fixedPrice).IsSuccess)
			{
				return false;
			}
			
			return true;
		}
		
		private Result IsApplicable(
			IOrderedCartItemWithDiscountDetails saleItem,
			IEnumerable<DiscountReasonBase> discountReasons,
			NomenclatureFixedPrice fixedPrice)
		{
			var isNotApplicable = CanApplyFixedPriceByType(discountReasons);

			if(isNotApplicable)
			{
				return Result.Failure(FixedPriceErrors.FixedPriceNotAllowed);
			}
			
			return CanApplyFixedPrice(saleItem, fixedPrice);
		}
		
		private Result CanApplyFixedPrice(
			IOrderedCartItemWithDiscountDetails saleItem,
			NomenclatureFixedPrice fixedPrice
		)
		{
			if(saleItem.ItemType is SaleItemType.PromoSet or SaleItemType.RentPackage)
			{
				return Result.Failure(FixedPriceErrors.FixedPriceNotAppliedToPromoSetsAndRentPackages);
			}

			if(saleItem.ErpId != fixedPrice.Nomenclature.Id)
			{
				return Result.Failure(FixedPriceErrors.FixedPriceNotAllowed);
			}

			if(saleItem.Count < fixedPrice.MinCount)
			{
				return FixedPriceErrors.FixedPriceNotAllowed;
			}
			
			return Result.Success();
		}
	}
}
