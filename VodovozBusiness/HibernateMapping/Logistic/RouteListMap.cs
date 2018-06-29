using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class RouteListMap : ClassMap<RouteList>
	{
		public RouteListMap ()
		{
			Table ("route_lists");

			OptimisticLock.Version();
			Version(x => x.Version)
				.Column("version");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.ActualDistance).Column ("actual_distance");
			Map (x => x.ConfirmedDistance).Column("confirmed_distance");
			Map (x => x.Date).Column ("date");
			Map (x => x.Status).Column ("status").CustomType<RouteListStatusStringType> ();
			Map (x => x.ClosingDate).Column("closing_date");
			Map (x => x.ClosingComment).Column("closing_comment");
			Map (x => x.ClosingFilled).Column("closing_filled");
			Map (x => x.LastCallTime).Column ("last_call_time");
			Map (x => x.DifferencesConfirmed).Column ("differences_confirmed");
			Map (x => x.IsManualAccounting).Column("is_manual_accounting");
			Map(x => x.OnLoadTimeStart).Column("on_load_start").CustomType<TimeAsTimeSpanType>();
			Map(x => x.OnLoadTimeEnd).Column("on_load_end").CustomType<TimeAsTimeSpanType>();
			Map(x => x.OnLoadGate).Column("on_load_gate");
			Map(x => x.OnloadTimeFixed).Column("on_load_time_fixed");
			Map(x => x.PlanedDistance).Column("plan_distance");
			Map(x => x.Printed).Column("printed");
			Map(x => x.RecalculatedDistance).Column("recalculated_distance");
			Map(x => x.MileageComment).Column("mileage_comment");
			Map(x => x.MileageCheck).Column("mileage_check");
			Map(x => x.NormalWage).Column("normal_wage");
			Map(x => x.FixedDriverWage).Column("fixed_driver_wage");
			Map(x => x.FixedForwarderWage).Column("fixed_forwarder_wage");

			References (x => x.Car).Column ("car_id");
			References (x => x.Shift).Column ("delivery_shift_id");
			References (x => x.Driver).Column ("driver_id");
			References (x => x.Forwarder).Column ("forwarder_id");
			References (x => x.Logistican).Column ("logistican_id");
			References (x => x.BottleFine).Column("bottles_fine_id");
			References (x => x.Cashier).Column("cashier_id");
			References (x => x.FuelOutlayedOperation).Column("fuel_outlayed_operation_id").Cascade.All();
			References (x => x.DriverWageOperation).Column("driver_wages_movement_operations_id");
			References (x => x.ForwarderWageOperation).Column("forwarder_wages_movement_operations_id");
			References (x => x.ClosedBy).Column("closed_by_employee_id");

			HasMany (x => x.Addresses).Cascade.AllDeleteOrphan ().Inverse ()
				.KeyColumn ("route_list_id")
				.AsList (x => x.Column ("order_in_route"));
			HasMany(x => x.FuelDocuments).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("route_list_id");
		}
	}
}

