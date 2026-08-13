using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class FirstOnlineOrderDiscount : DiscountReasonBase
	{
		public override DiscountReasonType DiscountReasonType => DiscountReasonType.FirstOnlineOrderDiscount;
		
		public static FirstOnlineOrderDiscount Create(DiscountReasonBase copyingDiscount)
		{
			var newDiscount = new FirstOnlineOrderDiscount();
			newDiscount.Copy(copyingDiscount);
			
			return newDiscount;
		}
	}
}
