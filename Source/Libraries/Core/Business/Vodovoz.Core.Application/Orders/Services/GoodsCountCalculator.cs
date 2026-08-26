using System.Collections.Generic;
using System.Linq;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class GoodsCountCalculator : IGoodsCountCalculator
	{
		public decimal TotalItemCount(INomenclatureCount saleItem, IEnumerable<ISaleItem> saleItems)
		{
			return saleItem.Nomenclature.IsWater19L
				? GetTotalWater19LCount(saleItems, true, true)
				: saleItem.Count;
		}
		
		public decimal GetTotalWater19LCount(
			IEnumerable<ISaleItem> saleItems,
			bool doNotCalculateWaterFromPromoSets = false,
			bool doNotCalculatePresentsDiscount = false)
		{
			var water19L = saleItems.Where(x => x.Nomenclature.IsWater19L);

			if(doNotCalculateWaterFromPromoSets)
			{
				water19L = water19L.Where(x => x.PromoSet is null);
			}

			if(doNotCalculatePresentsDiscount)
			{
				water19L = water19L.Where(x => !x.DiscountReasons.Any(r => r.IsPresent));
			}
			
			return (int)water19L.Sum(x => x.Count);
		}
	}
}
