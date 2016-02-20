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
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.IndexInRoute).Column("order_in_route");
			References (x => x.RouteList).Column ("route_list_id").Not.Nullable ();
			References (x => x.Order).Column ("order_id");
			Map (x => x.BottlesReturned).Column("bottles_returned");
			Map (x => x.DepositsCollected).Column("deposits_collected");
			Map (x => x.TotalCash).Column("total_cash");
			Map (x => x.DriverWage).Column("driver_wage");
			Map (x => x.ForwarderWage).Column("forwarder_wage");
			Map (x => x.WithoutForwarder).Column("without_forwarder");
			Map (x => x.StatusLastUpdate).Column("status_last_update");
			Map (x => x.Comment).Column("comment").Length(150);
			Map(x => x.Status).Column("status").CustomType<RouteListItemStatusStringType>();
		}
	}
}

