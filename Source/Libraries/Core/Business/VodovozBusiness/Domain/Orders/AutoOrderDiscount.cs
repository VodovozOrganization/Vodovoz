using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Скидки при автозаказе",
		Nominative = "Скидка при автозаказе",
		GenitivePlural = "Скидку при автозаказе")]
	public class AutoOrderDiscount : DiscountReasonBase
	{
		public override DiscountReasonType DiscountReasonType => DiscountReasonType.AutoOrder;
		
		public static AutoOrderDiscount Create(DiscountReasonBase copyingDiscount)
		{
			var newDiscount = new AutoOrderDiscount();
			newDiscount.Copy(copyingDiscount);
			
			return newDiscount;
		}
	}
}
