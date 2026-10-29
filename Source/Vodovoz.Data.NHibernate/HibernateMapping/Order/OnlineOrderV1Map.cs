using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderV1Map : SubclassMap<OnlineOrderV1>
	{
		public OnlineOrderV1Map()
		{
			DiscriminatorValue(nameof(OnlineOrderVersion.V1));
		}
	}
}
