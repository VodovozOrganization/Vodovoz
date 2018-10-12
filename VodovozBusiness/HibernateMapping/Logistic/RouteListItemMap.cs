using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
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
			Map (x => x.EquipmentDepositsCollected).Column("equipment_deposits_collected");
			Map (x => x.ExtraCash)				.Column("extra_cash");
			Map (x => x.TotalCash)				.Column("total_cash");
			Map (x => x.DriverWage)				.Column("driver_wage");
            Map (x => x.DriverWageSurcharge)    .Column ("driver_wage_surcharge");
			Map (x => x.ForwarderWage)			.Column("forwarder_wage");
            Map (x => x.ForwarderWageSurcharge) .Column ("forwarder_wage_surcharge");
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
			Map(x => x.PlanTimeStart)			.Column("plan_time_start").CustomType<TimeAsTimeSpanType>();
			Map(x => x.PlanTimeEnd)				.Column("plan_time_end").CustomType<TimeAsTimeSpanType>();

			References (x => x.RouteList)			.Column ("route_list_id").Not.Nullable ();
			References (x => x.Order)				.Column ("order_id").Cascade.SaveUpdate();
			References (x => x.TransferedTo)		.Column ("transfered_to_id");
			References (x => x.CashierCommentAuthor).Column ("cashier_comment_author");
		}
	}
}

