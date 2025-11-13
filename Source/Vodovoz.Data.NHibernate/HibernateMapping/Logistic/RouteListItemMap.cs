using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteListItemMap : ClassMap<RouteListItem>
	{
		public RouteListItemMap()
		{
			Table("route_list_addresses");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.IndexInRoute).Column("order_in_route");
			Map(x => x.BottlesReturned).Column("bottles_returned");
			Map(x => x.DriverBottlesReturned).Column("driver_bottles_returned");
			Map(x => x.OldBottleDepositsCollected).Column("deposits_collected");
			Map(x => x.OldEquipmentDepositsCollected).Column("equipment_deposits_collected");
			Map(x => x.ExtraCash).Column("extra_cash");
			Map(x => x.TotalCash).Column("total_cash");
			Map(x => x.DriverWage).Column("driver_wage");
			Map(x => x.DriverWageSurcharge).Column("driver_wage_surcharge");
			Map(x => x.ForwarderWage).Column("forwarder_wage");
			Map(x => x.WithForwarder).Column("with_forwarder");
			Map(x => x.StatusLastUpdate).Column("status_last_update");
			Map(x => x.Comment).Column("comment").Length(150);
			Map(x => x.Status).Column("status");
			Map(x => x.AddressTransferType).Column("address_transfer_type");
			Map(x => x.WasTransfered).Column("was_transfered");
			Map(x => x.RecievedTransferAt).Column("recieved_transfer_at");
			Map(x => x.CashierComment).Column("cashier_comment");
			Map(x => x.CashierCommentCreateDate).Column("cashier_comment_create_date");
			Map(x => x.CashierCommentLastUpdate).Column("cashier_comment_last_update");
			Map(x => x.Notified30Minutes).Column("notified_30minutes");
			Map(x => x.NotifiedTimeout).Column("notified_timeout");
			Map(x => x.PlanTimeStart).Column("plan_time_start").CustomType<TimeAsTimeSpanType>();
			Map(x => x.PlanTimeEnd).Column("plan_time_end").CustomType<TimeAsTimeSpanType>();
			Map(x => x.CommentForFine).Column("comment_for_fine");
			Map(x => x.IsDriverForeignDistrict).Column("is_driver_foreign_district");
			Map(x => x.UnscannedCodesReason).Column("unscanned_codes_reason");

			Map(x => x.CreationDate).Column("creation_date")
				.Insert().Not.Update().Access.ReadOnlyPropertyThroughCamelCaseField(Prefix.Underscore);

			References(x => x.RouteList).Column("route_list_id").Not.Nullable();
			References(x => x.Order).Column("order_id").Cascade.SaveUpdate();
			References(x => x.TransferedTo).Column("transfered_to_id");
			References(x => x.CashierCommentAuthor).Column("cashier_comment_author");
			References(x => x.DriverWageCalculationMethodic).Column("driver_wage_calculation_methodic_id");
			References(x => x.ForwarderWageCalculationMethodic).Column("forwarder_wage_calculation_methodic_id");
			References(x => x.LateArrivalReason).Column("late_arrival_reason_id");
			References(x => x.LateArrivalReasonAuthor).Column("late_arrival_reason_author_id");
			References(x => x.CommentForFineAuthor).Column("comment_for_fine_author_id");

			HasMany(x => x.TrueMarkCodes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("route_list_item_id");

			HasManyToMany(x => x.Fines)
				.Table("fines_to_route_list_addresses")
				.ParentKeyColumn("route_list_address_id")
				.ChildKeyColumn("fine_id")
				.LazyLoad();
		}
	}
}
