using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;

namespace Vodovoz.HibernateMapping.Payments
{
	public class OrderIdProviderForMobileAppMap : ClassMap<OrderIdProviderForMobileApp>
	{
		public OrderIdProviderForMobileAppMap()
		{
			Table("mobile_app_order_ids");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Uuid).Column("uuid");
			Map(x => x.OrderSum).Column("order_sum");
			Map(x => x.Created).Column("created").ReadOnly();
		}
	}
}