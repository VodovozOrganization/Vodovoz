using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class OnlineOrderPromoSetNode : IOnlineOrderPromoSetInfo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Count { get; set; }
		public decimal? ReceivedPrice { get; set; }
		public decimal? ReceivedSum => ReceivedPrice.HasValue ? Math.Round(Count * ReceivedPrice.Value, 2) : (decimal?)null;
		public decimal OurPrice { get; set; }
		public decimal OurSum => Math.Round(Count * OurPrice, 2);
		public string DiscountReasonNames { get; set; }
		public OnlineOrderErrorState? OnlineOrderErrorState { get; set; }
		public List<OnlineOrderPromoSetItemNode> Items { get; set; } = new List<OnlineOrderPromoSetItemNode>();

		public void AddItem(OnlineOrderPromoSetItemNode item)
		{
			Items.Add(item);
			OurPrice += item.OurPrice;
			item.PromoSet = this;
		}
	}
}
