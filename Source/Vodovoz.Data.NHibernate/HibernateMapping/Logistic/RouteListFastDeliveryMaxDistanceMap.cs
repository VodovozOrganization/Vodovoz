using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteListFastDeliveryMaxDistanceMap : ClassMap<RouteListFastDeliveryMaxDistance>
	{
		public RouteListFastDeliveryMaxDistanceMap()
		{
			Table("route_list_fast_delivery_max_distance");

			Id(x => x.Id, "id").GeneratedBy.Native();

			Map(x => x.StartDate, "start_date");
			Map(x => x.EndDate, "end_date");
			Map(x => x.Distance, "distance");

			References(x => x.RouteList).Column("route_list_id");
		}
	}
}
