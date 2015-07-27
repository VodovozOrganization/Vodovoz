using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class RouteListItemMap : ClassMap<RouteListItem>
	{
		public RouteListItemMap ()
		{
			Table("route_list_addresses");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			References (x => x.RouteList).Column ("route_list_id");
			References (x => x.Order).Column ("order_id");
		}
	}
}

