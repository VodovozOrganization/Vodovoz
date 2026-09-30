using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class FirstOnlineOrderDiscountMap : SubclassMap<FirstOnlineOrderDiscount>
	{
		public FirstOnlineOrderDiscountMap()
		{
			DiscriminatorValue(nameof(DiscountReasonType.FirstOnlineOrderDiscount));
		}
	}
}
