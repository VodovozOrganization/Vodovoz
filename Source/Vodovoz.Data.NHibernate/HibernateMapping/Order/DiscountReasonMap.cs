using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class DiscountReasonMap : SubclassMap<DiscountReason>
	{
		public DiscountReasonMap()
		{
			DiscriminatorValue(nameof(DiscountReasonType.Discount));
		}
	}
}
