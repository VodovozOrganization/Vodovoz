using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Abstractions.V5.Sale;
using CustomerOrdersApi.Library.V5.Dto.Orders.FixedPrice;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;

namespace CustomerOrdersApi.Library.V5.Services
{
	public class OnlineOrderFixedPriceHandler : FixedPriceHandler, IOnlineOrderFixedPriceHandler
	{
		public OnlineOrderFixedPriceHandler(
			IGenericRepository<NomenclatureFixedPrice> nomenclatureFixedPriceRepository
			) : base(nomenclatureFixedPriceRepository)
		{
		}

		public Result<IEnumerable<OnlineOrderItemWithFixedPriceDto>> TryApplyFixedPrice(
			IUnitOfWork uow,
			IApplyingFixedPrice receivedData
			)
		{
			if(!HasFixedPrices(
				uow,
				receivedData.ErpCounterpartyId,
				receivedData.ErpDeliveryPointId,
				receivedData.IsSelfDelivery,
				out var fixedPrices))
			{
				return Result.Failure<IEnumerable<OnlineOrderItemWithFixedPriceDto>>(Vodovoz.Errors.Orders.FixedPriceErrors.NotFound);
			}

			return TryApplyFixedPrice(receivedData, fixedPrices);
		}

		private Result<IEnumerable<OnlineOrderItemWithFixedPriceDto>> TryApplyFixedPrice(
			IApplyingFixedPrice receivedData,
			IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			var itemsWithFixedPrice = new List<OnlineOrderItemWithFixedPriceDto>();

			foreach(var onlineItem in receivedData.OnlineOrderItems)
			{
				var onlineOrderedProductWithFixedPrice = new OnlineOrderItemWithFixedPriceDto
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
	}
}
