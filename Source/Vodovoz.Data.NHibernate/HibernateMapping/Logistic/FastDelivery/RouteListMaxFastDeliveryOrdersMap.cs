using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.FastDelivery
{
	public class RouteListMaxFastDeliveryOrdersMap : ClassMap<RouteListMaxFastDeliveryOrders>
	{
		public RouteListMaxFastDeliveryOrdersMap()
		{
			Table("route_list_max_fast_delivery_orders");

			Id(x => x.Id, "id").GeneratedBy.Native();

			Map(x => x.StartDate, "start_date");
			Map(x => x.EndDate, "end_date");
			Map(x => x.MaxOrders, "max_orders");

			References(x => x.RouteList).Column("route_list_id");
		}
	}
}
