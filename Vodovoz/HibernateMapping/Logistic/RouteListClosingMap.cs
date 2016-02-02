using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class RouteListClosingMap:ClassMap<RouteListClosing>
	{
		public RouteListClosingMap()
		{
			Table("routelist_closing");

			Id (x => x.Id).Column("id").GeneratedBy.Native();
			Map (x => x.ClosingDate).Column("closing_date");
			References (x => x.Cashier).Column("cashier_id");
			References (x => x.RouteList).Column("route_list_id");
		}
	}
}

