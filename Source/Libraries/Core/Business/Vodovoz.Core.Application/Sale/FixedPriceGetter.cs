using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class FixedPriceGetter : IFixedPriceGetter
	{
		private readonly IGoodsCountCalculator _goodsCountCalculator;

		public FixedPriceGetter(
			IGoodsCountCalculator goodsCountCalculator
			)
		{
			_goodsCountCalculator = goodsCountCalculator ?? throw new ArgumentNullException(nameof(goodsCountCalculator));
		}
		
		public IEnumerable<Nomenclature> GetNomenclaturesWithFixedPrices(ISaleSource saleSource)
		{
			var fixedPrices = new List<NomenclatureFixedPrice>();
			var client = saleSource.Counterparty;

			if(client != null)
			{
				fixedPrices.AddRange(client.NomenclatureFixedPrices);
				fixedPrices.AddRange(client.DeliveryPoints.SelectMany(x => x.NomenclatureFixedPrices));
			}
			
			return fixedPrices
				.Select(x => x.Nomenclature)
				.Distinct();
		}
		
		public (SaleItemPriceType PriceType, decimal Price)? GetFixedPriceOrNull(
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IEnumerable<ISaleItem> allSaleItems,
			ISaleItem saleItem
		)
		{
			var bottlesCount = _goodsCountCalculator.TotalItemCount(saleItem, allSaleItems);
			return GetFixedPriceOrNull(deliveryPoint, counterparty, saleItem, bottlesCount);
		}
		
		public (SaleItemPriceType PriceType, decimal Price)? GetFixedPriceOrNull(
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGetFixedPrice saleItem,
			decimal bottlesCount
		)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(saleItem.PromoSet != null)
			{
				return null;
			}

			//TODO-5967 и проверка на скидку, ведь может прийти без фиксы, но и без скидки и мы должны взять фиксу в этом случае
			/*if(!currentProduct.IsFixedPrice)
			{
				return null;
			}*/
			
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

			var influentialNomenclature = saleItem.Nomenclature.DependsOnNomenclature;
			decimal? fixedPrice = null;

			if(influentialNomenclature is null
				&& fixedPrices.Any(x =>
					x.Nomenclature.Id == saleItem.Nomenclature.Id
					&& bottlesCount >= x.MinCount))
			{
				fixedPrice = fixedPrices
					.OrderBy(x=> x.MinCount)
					.Last(x => x.Nomenclature.Id == saleItem.Nomenclature.Id && bottlesCount >= x.MinCount)
					.Price;
			}

			if(influentialNomenclature != null
				&& fixedPrices.Any(x =>
					x.Nomenclature.Id == influentialNomenclature.Id
					&& bottlesCount >= x.MinCount))
			{
				fixedPrice = fixedPrices
					.OrderBy(x => x.MinCount)
					.Last(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount)
					.Price;
			}

			return fixedPrice.HasValue
				? (SaleItemPriceType.Fixed, fixedPrice.Value)
				: null;
		}
	}
}
