using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Handlers;
using VodovozBusiness.Domain.Orders.V5;
using VodovozBusiness.Nodes;
using VodovozBusiness.Nodes.V5;

namespace Vodovoz.Application.Orders.Services.V5
{
	public class OnlineOrderFixedPriceHandlerV5 : IOnlineOrderFixedPriceHandlerV5
	{
		private readonly IGenericRepository<NomenclatureFixedPrice> _nomenclatureFixedPriceRepository;

		public OnlineOrderFixedPriceHandlerV5(IGenericRepository<NomenclatureFixedPrice> nomenclatureFixedPriceRepository)
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

		public Result<IEnumerable<IOnlineOrderedProductWithFixedPriceV5>> TryApplyFixedPrice(
			IUnitOfWork uow,
			CanApplyOnlineOrderFixedPriceV5 canApplyOnlineOrderFixedPrice)
		{
			if(!HasFixedPrices(
				uow,
				canApplyOnlineOrderFixedPrice.CounterpartyId,
				canApplyOnlineOrderFixedPrice.DeliveryPointId,
				canApplyOnlineOrderFixedPrice.IsSelfDelivery,
				out var fixedPrices))
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProductWithFixedPriceV5>>(Vodovoz.Errors.Orders.FixedPriceErrors.NotFound);
			}

			return TryApplyFixedPrice(canApplyOnlineOrderFixedPrice, fixedPrices);
		}

		private Result<IEnumerable<IOnlineOrderedProductWithFixedPriceV5>> TryApplyFixedPrice(
			CanApplyOnlineOrderFixedPriceV5 canApplyOnlineOrderFixedPrice,
			IEnumerable<NomenclatureFixedPrice> fixedPrices)
		{
			var itemsWithFixedPrice = new List<IOnlineOrderedProductWithFixedPriceV5>();

			foreach(var onlineItem in canApplyOnlineOrderFixedPrice.OnlineOrderItems)
			{
				var onlineOrderedProductWithFixedPrice = new OnlineOrderItemWithFixedPriceV5
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
						onlineOrderedProductWithFixedPrice.Discounts = onlineItem.Discounts;
						continue;
					}

					onlineOrderedProductWithFixedPrice.NewPrice = fixedPrice.Price;
					
					break;
				}
				
				itemsWithFixedPrice.Add(onlineOrderedProductWithFixedPrice);
			}
			
			return Result.Success(itemsWithFixedPrice.AsEnumerable());
		}

		private bool CanApplyFixedPrice(IOnlineOrderedProductV5 onlineItem, NomenclatureFixedPrice fixedPrice)
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
