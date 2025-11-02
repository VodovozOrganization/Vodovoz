using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RouteListMap : ClassMap<RouteList>
	{
		public RouteListMap()
		{
			Table("route_lists");

			OptimisticLock.Version();
			Version(x => x.Version)
				.Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ConfirmedDistance).Column("confirmed_distance");
			Map(x => x.Date).Column("date").Access.CamelCaseField(Prefix.Underscore);
			Map(x => x.Status).Column("status");
			Map(x => x.ClosingDate).Column("closing_date");
			Map(x => x.FirstClosingDate).Column("first_closing_date");
			Map(x => x.ClosingComment).Column("closing_comment");
			Map(x => x.LogisticiansComment).Column("logisticians_comment");
			Map(x => x.ClosingFilled).Column("closing_filled");
			Map(x => x.LastCallTime).Column("last_call_time");
			Map(x => x.DifferencesConfirmed).Column("differences_confirmed");
			Map(x => x.IsManualAccounting).Column("is_manual_accounting");
			Map(x => x.OnLoadTimeStart).Column("on_load_start").CustomType<TimeAsTimeSpanType>();
			Map(x => x.OnLoadTimeEnd).Column("on_load_end").CustomType<TimeAsTimeSpanType>();
			Map(x => x.OnLoadGate).Column("on_load_gate");
			Map(x => x.OnloadTimeFixed).Column("on_load_time_fixed");
			Map(x => x.PlanedDistance).Column("plan_distance");
			Map(x => x.AddressesOrderWasChangedAfterPrinted).Column("addresses_order_was_changed_after_printed");
			Map(x => x.RecalculatedDistance).Column("recalculated_distance");
			Map(x => x.MileageComment).Column("mileage_comment");
			Map(x => x.MileageCheck).Column("mileage_check");
			Map(x => x.NormalWage).Column("normal_wage");
			Map(x => x.FixedDriverWage).Column("fixed_driver_wage");
			Map(x => x.FixedForwarderWage).Column("fixed_forwarder_wage");
			Map(x => x.NotFullyLoaded).Column("not_fully_loaded");
			Map(x => x.CashierReviewComment).Column("cashier_review_comment");
			Map(x => x.WasAcceptedByCashier).Column("was_accepted_by_cashier");
			Map(x => x.HasFixedShippingPrice).Column("has_fixed_shipping_price");
			Map(x => x.FixedShippingPrice).Column("fixed_shipping_price");
			Map(x => x.DriverTerminalCondition).Column("driver_terminal_condition");
			Map(x => x.DeliveredAt).Column("delivered_at");
			Map(x => x.SpecialConditionsAccepted).Column("special_conditions_accepted");
			Map(x => x.SpecialConditionsAcceptedAt).Column("special_conditions_accepted_at");

			References(x => x.Car).Column("car_id")
				.Access.CamelCaseField(Prefix.Underscore);
			References(x => x.Shift).Column("delivery_shift_id");
			References(x => x.Driver).Column("driver_id")
				.Access.CamelCaseField(Prefix.Underscore);
			References(x => x.Forwarder).Column("forwarder_id");
			References(x => x.Logistician).Column("logistican_id");
			References(x => x.BottleFine).Column("bottles_fine_id");
			References(x => x.Cashier).Column("cashier_id");
			References(x => x.FuelOutlayedOperation).Column("fuel_outlayed_operation_id").Cascade.All();
			References(x => x.DriverWageOperation).Column("driver_wages_movement_operations_id");
			References(x => x.ForwarderWageOperation).Column("forwarder_wages_movement_operations_id");
			References(x => x.ClosedBy).Column("closed_by_employee_id");
			References(x => x.LogisticiansCommentAuthor).Column("logisticians_comment_author_id");
			References(x => x.AdditionalLoadingDocument).Column("additional_loading_document_id").Cascade.AllDeleteOrphan();
			References(x => x.RouteListProfitability).Column("route_list_profitability_id").Cascade.AllDeleteOrphan();

			HasMany(x => x.Addresses).Cascade.AllDeleteOrphan().Inverse()
				.KeyColumn("route_list_id").OrderBy("order_in_route");
			HasMany(x => x.FastDeliveryMaxDistanceItems).Cascade.AllDeleteOrphan().Inverse().KeyColumn("route_list_id").OrderBy("start_date");
			HasMany(x => x.MaxFastDeliveryOrdersItems).Cascade.AllDeleteOrphan().Inverse().KeyColumn("route_list_id").OrderBy("start_date");
			HasMany(x => x.FuelDocuments).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("route_list_id");
			HasMany(x => x.PrintsHistory).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("route_list_id");
			HasMany(x => x.DeliveryFreeBalanceOperations).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("route_list_id");
			HasManyToMany(x => x.GeographicGroups).Table("geo_groups_to_entities")
												  .ParentKeyColumn("route_list_id")
												  .ChildKeyColumn("geo_group_id")
												  .LazyLoad();
		}
	}
}

