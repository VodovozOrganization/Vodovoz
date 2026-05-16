using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Service;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <inheritdoc/>
	public class GoodsPriceCalculator : IGoodsPriceCalculator
	{
		/// <inheritdoc/>
		public decimal CalculateItemPrice(
			IEnumerable<IGoods> products,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGoods currentProduct,
			bool hasPermissionsForAlternativePrice)
		{
			var fixedPrice = GetFixedPriceOrNull(
				products,
				deliveryPoint,
				counterparty,
				currentProduct);

			if(fixedPrice != null)
			{
				return fixedPrice.Price;
			}

			var count = currentProduct.PromoSet == null
				? GetTotalWater19LCount(products, true)
				: currentProduct.Count;

			var canApplyAlternativePrice =
				hasPermissionsForAlternativePrice
				&& currentProduct.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return currentProduct.Nomenclature.GetPrice(count, canApplyAlternativePrice);
		}

		private decimal GetTotalWater19LCount(IEnumerable<IGoods> products, bool doNotCalculateWaterFromPromoSets = false)
		{
			var water19L = doNotCalculateWaterFromPromoSets
				? products.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L && x.PromoSet == null)
				: products.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L);
			
			return (int)water19L.Sum(x => x.Count);
		}
		
		private NomenclatureFixedPrice GetFixedPriceOrNull(
			IEnumerable<IGoods> products,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGoods currentProduct)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(currentProduct.PromoSet != null)
			{
				return null;
			}

			if(!currentProduct.IsFixedPrice)
			{
				return null;
			}
			
			if(deliveryPoint is null)
			{
				if(counterparty is null)
				{
					return null;
				}

				fixedPrices = counterparty.NomenclatureFixedPrices;
			}
			else
			{
				fixedPrices = deliveryPoint.NomenclatureFixedPrices;
			}

			var influentialNomenclature = currentProduct.Nomenclature.DependsOnNomenclature;
			var bottlesCount = GetTotalWater19LCount(products);

			if(influentialNomenclature is null
				&& fixedPrices.Any(x =>
					x.Nomenclature.Id == currentProduct.Nomenclature.Id
					&& bottlesCount >= x.MinCount))
			{
				return fixedPrices
					.OrderBy(x=> x.MinCount)
					.Last(x => x.Nomenclature.Id == currentProduct.Nomenclature.Id && bottlesCount >= x.MinCount);
			}

			if(influentialNomenclature != null
				&& fixedPrices.Any(x =>
					x.Nomenclature.Id == influentialNomenclature.Id
					&& bottlesCount >= x.MinCount))
			{
				return fixedPrices
					.OrderBy(x => x.MinCount)
					.Last(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount);
			}

			return null;
		}
	}
}
