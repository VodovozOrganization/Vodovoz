using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Handlers;

namespace Vodovoz.Core.Application.Sale
{
	public class FixedPriceHandler : IFixedPriceHandler
	{
		public FixedPriceHandler(
			IGenericRepository<NomenclatureFixedPrice> nomenclatureFixedPriceRepository
			)
		{
			NomenclatureFixedPriceRepository =
				nomenclatureFixedPriceRepository ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceRepository));
		}
		
		protected IGenericRepository<NomenclatureFixedPrice> NomenclatureFixedPriceRepository { get; }
		
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
				
				fixedPrices = NomenclatureFixedPriceRepository
					.Get(uow, x => x.Counterparty.Id == counterpartyId.Value)
					.ToList();
				
				return fixedPrices.Any();
			}

			if(!deliveryPointId.HasValue)
			{
				return false;
			}
			
			fixedPrices = NomenclatureFixedPriceRepository
				.Get(uow, x => x.DeliveryPoint.Id == deliveryPointId.Value)
				.ToList();
				
			return fixedPrices.Any();
		}
		
		public Result IsApplicable(
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

		private bool CanApplyFixedPriceByType(IEnumerable<DiscountReasonBase> discountReasons)
		{
			return discountReasons
				.SelectMany(x => x.DiscountApplicabilities)
				.Any(x => x.UseDiscountType == UseDiscountType.NotApplicable
					&& x.DiscountType == DiscountType.FixedPrice);
		}

		protected virtual Result CanApplyFixedPrice(
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
