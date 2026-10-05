using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public abstract class FixedPriceHandler
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

		protected bool CanApplyFixedPriceByType(IEnumerable<DiscountReasonBase> discountReasons)
		{
			return discountReasons
				.SelectMany(x => x.DiscountApplicabilities)
				.Any(x => x.UseDiscountType == UseDiscountType.NotApplicable
					&& x.DiscountType == DiscountType.FixedPrice);
		}
	}
}
