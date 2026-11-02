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
		public string PromoSetsCountMultiplierInfo => $"x {PromoSetsCount:N0}";
		public decimal? ReceivedPrice { get; set; }
		public decimal? ReceivedDiscountMoney => null;
		public decimal? ReceivedSum => Count * ReceivedPrice - ReceivedDiscountMoney;
		public decimal RoundedOurPrice => Math.Round(OurPrice, 2);
		public decimal? OurDiscountMoneyFromPromoSet { get; set; }
		public decimal OurAdditionalDiscountMoney { get; set; }
		public decimal? NomenclaturePrice { get; set; }
		public int NomenclatureUnit { get; set; }
		public bool IsDiscountInMoney { get; set; }
		public decimal OurSum => Math.Round(PromoSetsCount * OurPrice - OurAdditionalDiscountMoney, 2);
		public string DiscountReasonNames { get; set; }
		public OnlineOrderPromoSetNode PromoSet { get; set; }
		public OnlineOrderErrorState? OnlineOrderErrorState { get; set; }
		public decimal PromoSetsCount { get; set; }
		private decimal OurPrice => Count == 0
			? 0m
			: (Count * NomenclaturePrice ?? 0m) - (OurDiscountMoneyFromPromoSet ?? 0m);

		public void UpdateDiscounts(PromoSetItemTotalDiscount itemTotalDiscounts)
		{
			var totalDiscountValue = itemTotalDiscounts.TotalDiscountValue;
			var discountValue = itemTotalDiscounts.AdditionalDiscountValue;

			IsDiscountInMoney = totalDiscountValue.IsDiscountMoney;
			OurDiscountMoneyFromPromoSet = totalDiscountValue.DiscountMoney - discountValue.DiscountMoney;
			OurAdditionalDiscountMoney = discountValue.DiscountMoney;
			DiscountReasonNames = string.Join(
				",",
				itemTotalDiscounts.DiscountReasons
					.Select(x => x.Name)
					.ToArray());
		}
	}
}
