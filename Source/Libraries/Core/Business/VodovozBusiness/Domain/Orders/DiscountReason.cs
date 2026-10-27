using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Domain.Orders
{
	public class DiscountReason : DiscountReasonBase
	{
		public override DiscountReasonType DiscountReasonType => DiscountReasonType.Discount;

		public static DiscountReason Create(
			DiscountReasonBase copyingDiscount,
			IObservableList<PromotionalSet> promoSets = null)
		{
			var newDiscount = new DiscountReason();
			newDiscount.Copy(copyingDiscount, promoSets);
			
			return newDiscount;
		}
	}
}
