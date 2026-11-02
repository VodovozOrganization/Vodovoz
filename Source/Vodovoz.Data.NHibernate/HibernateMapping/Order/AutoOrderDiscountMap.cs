using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class AutoOrderDiscountMap : SubclassMap<AutoOrderDiscount>
	{
		public AutoOrderDiscountMap()
		{
			DiscriminatorValue(nameof(DiscountReasonType.AutoOrder));
		}
	}
}
