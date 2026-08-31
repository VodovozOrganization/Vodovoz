using System;

namespace VodovozBusiness.Nodes
{
	public class OnlineOrderPromoSetItemNode : IOnlineOrderPromoSetInfo
	{
		public int OnlinePromoSetId { get; set; }
		public string Name { get; set; }
		public decimal Count { get; set; }
		public decimal? ReceivedPrice { get; set; }
		public decimal? ReceivedSum => Count * ReceivedPrice;
		public decimal OurPrice { get; set; }
		public decimal OurSum => Math.Round(Count * OurPrice, 2);
		public OnlineOrderPromoSetNode PromoSet { get; set; }
	}
}
