using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;

namespace Vodovoz.Application.Orders.Services
{
	public class GoodsPriceCalculator : IGoodsPriceCalculator
	{
		public decimal CalculateItemPrice(
			IEnumerable<IProduct> products,
			DeliveryPoint deliveryPoint,
			CounterpartyContract contract,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			decimal bottlesCount,
			bool hasPermissionsForAlternativePrice)
		{
			var fixedPrice = GetFixedPriceOrNull(
				products,
				deliveryPoint,
				contract,
				nomenclature,
				promoSet,
				bottlesCount);

			if(fixedPrice != null)
			{
				return fixedPrice.Price;
			}

			var count = promoSet == null
				? GetTotalWater19LCount(products, true)
				: bottlesCount;

			var canApplyAlternativePrice =
				hasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return nomenclature.GetPrice(count, canApplyAlternativePrice);
		}

		private decimal GetTotalWater19LCount(IEnumerable<IProduct> products, bool doNotCalculateWaterFromPromoSets = false)
		{
			var water19L = doNotCalculateWaterFromPromoSets
				? products.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L && x.PromoSet == null)
				: products.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L);
			
			return (int)water19L.Sum(x => x.Count);
		}
		
		private NomenclatureFixedPrice GetFixedPriceOrNull(
			IEnumerable<IProduct> products,
			DeliveryPoint deliveryPoint,
			CounterpartyContract contract,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			decimal bottlesCount)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(promoSet != null)
			{
				return null;
			}
			
			if(deliveryPoint == null)
			{
				if(contract == null)
				{
					return null;
				}

				fixedPrices = contract.Counterparty.NomenclatureFixedPrices;
			}
			else
			{
				fixedPrices = deliveryPoint.NomenclatureFixedPrices;
			}

			var influentialNomenclature = nomenclature.DependsOnNomenclature;
			bottlesCount += GetTotalWater19LCount(products);

			if(fixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount && influentialNomenclature == null)) 
			{
				return fixedPrices.OrderBy(x=> x.MinCount).Last(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount);
			}

			if(influentialNomenclature != null && fixedPrices.Any(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount)) 
			{
				return fixedPrices.OrderBy(x => x.MinCount).Last(x => x.Nomenclature.Id == influentialNomenclature?.Id && bottlesCount >= x.MinCount);
			}

			return null;
		}
	}
}
