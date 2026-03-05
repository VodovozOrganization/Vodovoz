using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V4;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Handlers;
using VodovozBusiness.Domain.Orders.V4;
using VodovozBusiness.Nodes.V4;

namespace Vodovoz.Application.Orders.Services.V4
{
	public class OnlineOrderFixedPriceHandlerV4 : IOnlineOrderFixedPriceHandlerV4
	{
		private readonly IGenericRepository<NomenclatureFixedPrice> _nomenclatureFixedPriceRepository;

		public OnlineOrderFixedPriceHandlerV4(IGenericRepository<NomenclatureFixedPrice> nomenclatureFixedPriceRepository)
		{
			_nomenclatureFixedPriceRepository =
				nomenclatureFixedPriceRepository ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceRepository));
		}
		
		public bool HasFixedPrices(
			IUnitOfWork uow,
			int? counterpartyId,
			int? deliveryPointId,
			bool isSelfDelivery,
			out IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			fixedPrices = new List<NomenclatureFixedPrice>();
			
			if(isSelfDelivery)
			{
				if(!counterpartyId.HasValue)
				{
					return false;
				}
				
				fixedPrices = _nomenclatureFixedPriceRepository
					.Get(uow, x => x.Counterparty.Id == counterpartyId.Value)
					.ToList();
				
				return fixedPrices.Any();
			}

			if(!deliveryPointId.HasValue)
			{
				return false;
			}
			
			fixedPrices = _nomenclatureFixedPriceRepository
				.Get(uow, x => x.DeliveryPoint.Id == deliveryPointId.Value)
				.ToList();
				
			return fixedPrices.Any();
		}

		public Result<IEnumerable<IOnlineOrderedProductWithFixedPriceV4>> TryApplyFixedPrice(
			IUnitOfWork uow,
			CanApplyOnlineOrderFixedPriceV4 canApplyOnlineOrderFixedPrice)
		{
			if(!HasFixedPrices(
				uow,
				canApplyOnlineOrderFixedPrice.CounterpartyId,
				canApplyOnlineOrderFixedPrice.DeliveryPointId,
				canApplyOnlineOrderFixedPrice.IsSelfDelivery,
				out var fixedPrices))
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProductWithFixedPriceV4>>(Vodovoz.Errors.Orders.FixedPriceErrors.NotFound);
			}

			return TryApplyFixedPrice(canApplyOnlineOrderFixedPrice, fixedPrices);
		}

		private Result<IEnumerable<IOnlineOrderedProductWithFixedPriceV4>> TryApplyFixedPrice(
			CanApplyOnlineOrderFixedPriceV4 canApplyOnlineOrderFixedPrice,
			IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			var itemsWithFixedPrice = new List<IOnlineOrderedProductWithFixedPriceV4>();

			foreach(var onlineItem in canApplyOnlineOrderFixedPrice.OnlineOrderItems)
			{
				var onlineOrderedProductWithFixedPrice = new OnlineOrderItemWithFixedPriceV4
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

		private bool CanApplyFixedPrice(IOnlineOrderedProductV4 onlineItem, NomenclatureFixedPrice fixedPrice)
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
	}
}
