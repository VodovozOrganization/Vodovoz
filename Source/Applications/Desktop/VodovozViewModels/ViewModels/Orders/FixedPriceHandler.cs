using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Handlers;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class FixedPriceHandler : IFixedPriceHandler
	{
		public virtual decimal? GetWaterFixedPrice(
			IGetFixedPriceSource addProductSource,
			INomenclatureCount addingItem
			)
		{
			decimal? result = null;

			/*if(addProductSource.IsLoadedFrom1C)
			{
				return result;
			}*/
			
			var nomenclature = addingItem.Nomenclature;

			//влияющая номенклатура
			if(nomenclature.Category == NomenclatureCategory.water)
			{
				var fixedPrice = GetFixedPriceOrNull(addProductSource, addingItem, addProductSource.TotalItemCount(addingItem));
				
				if(fixedPrice != null)
				{
					return fixedPrice.Price;
				}
			}
			
			return result;
		}

		public virtual NomenclatureFixedPrice GetFixedPriceOrNull(
			IGetFixedPriceSource addProductSource,
			INomenclatureCount addingItem,
			decimal count
			)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(addProductSource.DeliveryPoint is null)
			{
				if(addProductSource.Counterparty is null)
				{
					return null;
				}

				fixedPrices = addProductSource.Counterparty.NomenclatureFixedPrices;
			}
			else
			{
				fixedPrices = addProductSource.DeliveryPoint.NomenclatureFixedPrices;
			}

			var nomenclature = addingItem.Nomenclature;
			var influentialNomenclature = nomenclature.DependsOnNomenclature;

			if(fixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id && count >= x.MinCount && influentialNomenclature == null)) 
			{
				return fixedPrices.OrderBy(x=>x.MinCount)
					.Last(x => x.Nomenclature.Id == nomenclature.Id && count >= x.MinCount);
			}

			if(influentialNomenclature != null && fixedPrices.Any(x => x.Nomenclature.Id == influentialNomenclature.Id && count >= x.MinCount)) 
			{
				return fixedPrices.OrderBy(x => x.MinCount)
					.Last(x => x.Nomenclature.Id == influentialNomenclature?.Id && count >= x.MinCount);
			}

			return null;
		}
	}
}
