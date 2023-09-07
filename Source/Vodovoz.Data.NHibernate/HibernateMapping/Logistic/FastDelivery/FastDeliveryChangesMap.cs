using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.FastDelivery
{
	public class FastDeliveryChangeMap : ClassMap<FastDeliveryChange>
	{
		public FastDeliveryChangeMap()
		{
			Table("fast_delivery_changes");

			Id(x => x.Id, "id").GeneratedBy.Native();

			Map(x => x.ChangeType).Column("type");
			Map(x => x.CreatedAt).Column("created_at");

			References(x => x.Order).Column("order_id");
			References(x => x.RouteList).Column("route_list_id");
		}
	}
}
