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
			References (x => x.BottleFine).Column("bottles_fine_id");
			References (x => x.Cashier).Column("cashier_id");
			References (x => x.RouteList).Column("route_list_id");
			References (x => x.FuelOutlayedOperation).Column("fuel_outlayed_operation_id").Cascade.All();
			References (x => x.FuelGivedDocument).Column("fuel_gived_document_id");
		}
	}
}

