using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class OnlineOrderPromoSetNode : IOnlineOrderPromoSetInfo
	{
		private decimal? _receivedDiscountMoney;
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Count { get; set; }
		public string PromoSetsCountMultiplierInfo => null;
		public decimal? ReceivedPrice { get; set; }

		public decimal? ReceivedDiscountMoney
		{
			get => _receivedDiscountMoney;
			set
			{
				if(!value.HasValue)
				{
					_receivedDiscountMoney = 0m;
					return;
				}
				
				_receivedDiscountMoney = value;
			}
		}

		public decimal? ReceivedSum => ReceivedPrice.HasValue
			? Math.Round(Count * ReceivedPrice.Value - (ReceivedDiscountMoney ?? 0), 2)
			: (decimal?)null;

		public decimal RoundedOurPrice { get; set; }
		public decimal? NomenclaturePrice => null;
		public decimal? OurDiscountMoneyFromPromoSet => null;
		public decimal OurAdditionalDiscountMoney { get; set; }
		public decimal OurSum { get; set; }
		public string DiscountReasonNames { get; set; }
		public OnlineOrderErrorState? OnlineOrderErrorState { get; set; }
		public List<OnlineOrderPromoSetItemNode> Items { get; set; } = new List<OnlineOrderPromoSetItemNode>();

		public void AddItem(OnlineOrderPromoSetItemNode item)
		{
			Items.Add(item);
			RoundedOurPrice += item.RoundedOurPrice;
			OurSum += item.OurSum;
			OurAdditionalDiscountMoney += item.OurAdditionalDiscountMoney;
			item.PromoSet = this;
		}
	}
}
