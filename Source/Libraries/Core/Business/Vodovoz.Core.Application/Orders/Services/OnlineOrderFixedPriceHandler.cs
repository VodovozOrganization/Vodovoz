using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Handlers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OnlineOrderFixedPriceHandler : FixedPriceHandler, IOnlineOrderFixedPriceHandler
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

		public Result<IEnumerable<IOnlineOrderedProductWithFixedPrice>> TryApplyFixedPrice(
			IUnitOfWork uow,
			CanApplyOnlineOrderFixedPrice canApplyOnlineOrderFixedPrice)
		{
			if(!HasFixedPrices(
				uow,
				canApplyOnlineOrderFixedPrice.CounterpartyId,
				canApplyOnlineOrderFixedPrice.DeliveryPointId,
				canApplyOnlineOrderFixedPrice.IsSelfDelivery,
				out var fixedPrices))
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProductWithFixedPrice>>(Vodovoz.Errors.Orders.FixedPriceErrors.NotFound);
			}

			return TryApplyFixedPrice(canApplyOnlineOrderFixedPrice, fixedPrices);
		}
		
		public Result<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems)> TryApplyFixedPriceV7(
			IUnitOfWork uow,
			CanApplyOnlineOrderFixedPriceV7 canApplyOnlineOrderFixedPrice)
		{
			if(!HasFixedPrices(
				uow,
				canApplyOnlineOrderFixedPrice.CounterpartyId,
				canApplyOnlineOrderFixedPrice.DeliveryPointId,
				canApplyOnlineOrderFixedPrice.IsSelfDelivery,
				out var fixedPrices))
			{
				return Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems)>(
					Vodovoz.Errors.Orders.FixedPriceErrors.NotFound);
			}

			return TryApplyFixedPrice(uow, canApplyOnlineOrderFixedPrice.OnlineOrderItems, fixedPrices);
		}

		private Result<IEnumerable<IOnlineOrderedProductWithFixedPrice>> TryApplyFixedPrice(
			CanApplyOnlineOrderFixedPrice canApplyOnlineOrderFixedPrice,
			IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			var itemsWithFixedPrice = new List<IOnlineOrderedProductWithFixedPrice>();

			foreach(var onlineItem in canApplyOnlineOrderFixedPrice.OnlineOrderItems)
			{
				var onlineOrderedProductWithFixedPrice = new OnlineOrderItemWithFixedPrice
				{
					Count = onlineItem.Count,
					NomenclatureId = onlineItem.NomenclatureId,
					PromoSetId = onlineItem.PromoSetId,
					OldPrice = onlineItem.Price
				};
				
				foreach(var fixedPrice in fixedPrices)
				{
					if(!CanApplyFixedPrice(onlineItem, fixedPrice))
					{
						onlineOrderedProductWithFixedPrice.IsDiscountInMoney = onlineItem.IsDiscountInMoney;
						onlineOrderedProductWithFixedPrice.Discount = onlineItem.Discount;
						onlineOrderedProductWithFixedPrice.DiscountReasonId = onlineItem.DiscountReasonId;
						continue;
					}

					onlineOrderedProductWithFixedPrice.NewPrice = fixedPrice.Price;
					
					break;
				}
				
				itemsWithFixedPrice.Add(onlineOrderedProductWithFixedPrice);
			}
			
			return Result.Success(itemsWithFixedPrice.AsEnumerable());
		}

		private bool CanApplyFixedPrice(ICanApplyFixedPriceOnline onlineItem, NomenclatureFixedPrice fixedPrice)
		{
			if(onlineItem.PromoSetId.HasValue)
			{
				return false;
			}

			if(onlineItem.NomenclatureId != fixedPrice.Nomenclature.Id)
			{
				return false;
			}

			if(onlineItem.Count < fixedPrice.MinCount)
			{
				return false;
			}

			if(fixedPrice.Price >= onlineItem.PriceWithDiscount)
			{
				return false;
			}
			
			return true;
		}
		
		private Result<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems)> TryApplyFixedPrice(
			IUnitOfWork uow,
			IEnumerable<IOrderedCartItem> cartItems,
			IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			var cartItemsWithDiscountDetails = new List<IOrderedCartItemWithDiscountDetails>();
			var fixedPriceAppliedToAllItems = true;
			
			foreach(var cartItem in cartItems)
			{
				var cartItemWithDiscountDetails = OnlineOrderItemWithDiscountDetailsDto.Create(cartItem);
				var applied = false;
				
				foreach(var fixedPrice in fixedPrices)
				{
					if(!CanApplyFixedPriceV7(uow, cartItemWithDiscountDetails, fixedPrice, out var discountReasons))
					{
						_discountHandler.CalculateDiscount(cartItemWithDiscountDetails, discountReasons);
						continue;
					}

					ApplyFixedPrice(cartItemWithDiscountDetails, discountReasons, fixedPrice.Price);
					applied = true;
					break;
				}

				fixedPriceAppliedToAllItems &= applied;
				cartItemsWithDiscountDetails.Add(cartItemWithDiscountDetails);
			}
			
			return Result.Success((fixedPriceAppliedToAllItems, cartItemsWithDiscountDetails.AsEnumerable()));
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
			IUnitOfWork uow,
			IOrderedCartItemWithDiscountDetails cartItem,
			NomenclatureFixedPrice fixedPrice,
			out IEnumerable<DiscountReasonBase> discountReasons)
		{
			discountReasons = _discountReasonRepository.GetDiscountReasons(
				uow,
				cartItem.Discounts
					.Select(x => x.Id)
					.ToArray());
			
			if(!IsApplicable(cartItem, discountReasons, fixedPrice).IsSuccess)
			{
				return false;
			}
			
			return true;
		}
	}
}
