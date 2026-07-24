using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderV2Map : SubclassMap<OnlineOrderV2>
	{
		public OnlineOrderV2Map()
		{
			DiscriminatorValue(nameof(OnlineOrderVersion.V2));
			
			HasMany(x => x.PromoSets)
				.KeyColumn("online_order_id")
				.Inverse()
				.Cascade
				.AllDeleteOrphan();
		}
	}
}
