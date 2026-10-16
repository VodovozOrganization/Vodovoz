using System;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class OnlineOrderPromoSetItemNode : IOnlineOrderPromoSetInfo
	{
		public int Id { get; set; }
		public int OnlinePromoSetId { get; set; }
		public string Name { get; set; }
		public decimal Count { get; set; }
		public decimal? ReceivedPrice { get; set; }
		public decimal? ReceivedSum => Count * ReceivedPrice;
		public decimal OurPrice => NomenclaturePrice - DiscountMoney;
		public decimal NomenclaturePrice { get; set; }
		public decimal DiscountMoney { get; set; }
		public bool IsDiscountInMoney { get; set; }
		public decimal OurSum => Math.Round(Count * OurPrice, 2);
		public string DiscountReasonNames { get; set; }
		public OnlineOrderPromoSetNode PromoSet { get; set; }
		public OnlineOrderErrorState? OnlineOrderErrorState { get; set; }

		public void UpdateDiscounts(PromoSetItemTotalDiscount itemTotalDiscounts)
		{
			var discountValue = itemTotalDiscounts.DiscountValue;

			IsDiscountInMoney = discountValue.IsDiscountMoney;
			DiscountMoney = discountValue.DiscountMoney;
			DiscountReasonNames = string.Join(
				",",
				itemTotalDiscounts.DiscountReasons
					.Select(x => x.Name)
					.ToArray());
		}

		#region ICalculateTotalDiscountPromoSetItem implementation

		public decimal Sum(bool useAlternativePrice = false) => OurPrice;

		public decimal SumWithoutDiscount(bool useAlternativePrice = false) => NomenclaturePrice;

		#endregion
	}
}
