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

			Id (x => x.Id).Column ("id").GeneratedBy.Native();

			Map (x => x.IndexInRoute)			.Column("order_in_route");
			Map (x => x.BottlesReturned)		.Column("bottles_returned");
			Map (x => x.DriverBottlesReturned)	.Column("driver_bottles_returned");
			Map (x => x.DepositsCollected)		.Column("deposits_collected");
			Map (x => x.TotalCash)				.Column("total_cash");
			Map (x => x.DriverWage)				.Column("driver_wage");
			Map (x => x.ForwarderWage)			.Column("forwarder_wage");
			Map (x => x.WithForwarder)			.Column("with_forwarder");
			Map (x => x.StatusLastUpdate)		.Column("status_last_update");
			Map (x => x.Comment)				.Column("comment").Length(150);
			Map (x => x.Status)					.Column("status").CustomType<RouteListItemStatusStringType>();
			Map (x => x.NeedToReload)			.Column("need_to_reload");
			Map (x => x.WasTransfered)			.Column("was_transfered");
			Map (x => x.CashierComment)			.Column("cashier_comment").Length(150);
			Map(x => x.CashierCommentCreateDate).Column("cashier_comment_create_date");
			Map(x => x.CashierCommentLastUpdate).Column("cashier_comment_last_update");
			Map (x => x.Notified30Minutes)		.Column("notified_30minutes");
			Map (x => x.NotifiedTimeout)		.Column("notified_timeout");

			References (x => x.RouteList)			.Column ("route_list_id").Not.Nullable ();
			References (x => x.Order)				.Column ("order_id").Cascade.SaveUpdate();
			References (x => x.TransferedTo)		.Column ("transfered_to_id");
			References (x => x.CashierCommentAuthor).Column ("cashier_comment_author");
		}
	}
}

