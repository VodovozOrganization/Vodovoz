using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Service;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Orders.V5;
using VodovozBusiness.Services.Orders.V5;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <inheritdoc/>
	public class GoodsPriceCalculator : IGoodsPriceCalculator
	/// <inheritdoc/>
	public class GoodsPriceCalculator : IGoodsPriceCalculatorV5
	{
		/// <inheritdoc/>
		public decimal CalculateItemPrice(
			IEnumerable<IGoods> products,
		/// <inheritdoc/>
		public virtual decimal CalculatePrice(
			IEnumerable<ICalculatingPriceV5> products,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGoods currentProduct,
			bool hasPermissionsForAlternativePrice)
			Nomenclature nomenclature,
			bool isPromoSet,
			bool hasPermissionsForAlternativePrice,
			decimal addingGoodsCount = 0,
			bool needGetFixedPrice = true
			)
		{
			var fixedPrice = GetFixedPriceOrNull(
				counterparty,
				deliveryPoint,
				nomenclature,
				isPromoSet,
				GetTotalWater19LCount(products, doNotCountPresentsDiscount: true) + addingGoodsCount,
				needGetFixedPrice);
				
			if(fixedPrice != null)
			{
				return fixedPrice.Price;
			}

			var count = !isPromoSet
				? GetTotalWater19LCount(products, true, true)
				: addingGoodsCount;

			var canApplyAlternativePrice =
				hasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return nomenclature.GetPrice(count, canApplyAlternativePrice);
		}

		private decimal GetTotalWater19LCount(IEnumerable<IGoods> products, bool doNotCalculateWaterFromPromoSets = false)
		protected virtual decimal GetTotalWater19LCount(
			IEnumerable<ICalculatingPriceV5> products,
			bool doNotCalculateWaterFromPromoSets = false,
			bool doNotCountPresentsDiscount = false)
		{
			var water19L = products.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L);

			if(doNotCalculateWaterFromPromoSets)
			{
				water19L = water19L.Where(x => x.Nomenclature != null && x.Nomenclature.IsWater19L && x.PromoSet == null);
			}

			if(doNotCountPresentsDiscount)
			{
				water19L = water19L.Where(
					x => x.Discounts != null
					&& x.Discounts.All(y => y.DiscountReason?.IsPresent != true));
			}
			
			return (int)water19L.Sum(x => x.Count);
		}
		
		private NomenclatureFixedPrice GetFixedPriceOrNull(
			IEnumerable<IGoods> products,
		protected virtual NomenclatureFixedPrice GetFixedPriceOrNull(
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGoods currentProduct)
			Nomenclature nomenclature,
			bool isPromoSet,
			decimal bottlesCount,
			bool needGetFixedPrice = true)
		{
			if(isPromoSet)
			{
				return null;
			}

			//Т.к. в онлайн заказах можно применять скидку(промокод), если скидка больше чем фикса, но на прайс
			//то могут быть ситуации, когда у клиента есть фикса, но на позицию применена скидка,
			//для этого передаем флаг нужно ли подбирать фиксу
			if(!needGetFixedPrice)
			{
				return null;
			}
			
			IList<NomenclatureFixedPrice> fixedPrices;

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

			var influentialNomenclature = nomenclature.DependsOnNomenclature;

			if(fixedPrices.Any(x => 
				x.Nomenclature.Id == nomenclature.Id
					&& bottlesCount >= x.MinCount
					&& influentialNomenclature == null)) 
			{
				return fixedPrices
					.OrderBy(x=>x.MinCount)
					.Last(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount);
			}

			if(influentialNomenclature != null
				&& fixedPrices.Any(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount)) 
			{
				return fixedPrices
					.OrderBy(x => x.MinCount)
					.Last(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount);
			}

			return null;
		}
	}
}
