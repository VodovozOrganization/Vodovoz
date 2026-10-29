using System;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class FirstOnlineOrderDiscount : DiscountReasonBase
	{
		public override DiscountReasonType DiscountReasonType => DiscountReasonType.FirstOnlineOrderDiscount;
		
		public static FirstOnlineOrderDiscount Create(
			DiscountReasonBase copyingDiscount,
			IObservableList<PromotionalSet> promoSets = null)
		{
			var newDiscount = new FirstOnlineOrderDiscount();
			newDiscount.Copy(copyingDiscount, promoSets);
			
			return newDiscount;
		}
	}
}
