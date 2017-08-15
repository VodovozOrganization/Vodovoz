using DataAccess.NhibernateFixes;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class RouteListMap : ClassMap<RouteList>
	{
		public RouteListMap ()
		{
			Table ("route_lists");

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
			Map(x => x.OnLoadTimeStart).Column("on_load_start").CustomType<TimeAsTimeSpanTypeClone>();
			Map(x => x.OnLoadTimeEnd).Column("on_load_end").CustomType<TimeAsTimeSpanTypeClone>();
			Map(x => x.OnLoadGate).Column("on_load_gate");
			Map(x => x.OnloadTimeFixed).Column("on_load_time_fixed");

			References (x => x.Car).Column ("car_id");
			References (x => x.Shift).Column ("delivery_shift_id");
			References (x => x.Driver).Column ("driver_id");
			References (x => x.Forwarder).Column ("forwarder_id");
			References (x => x.Logistican).Column ("logistican_id");
			References (x => x.BottleFine).Column("bottles_fine_id");
			References (x => x.Cashier).Column("cashier_id");
			References (x => x.FuelOutlayedOperation).Column("fuel_outlayed_operation_id").Cascade.All();
			References (x => x.FuelGivedDocument).Column("fuel_gived_document_id");
			References (x => x.DriverWageOperation).Column("driver_wages_movement_operations_id");
			References (x => x.ForwarderWageOperation).Column("forwarder_wages_movement_operations_id");

			HasMany (x => x.Addresses).Cascade.AllDeleteOrphan ().Inverse ()
				.KeyColumn ("route_list_id")
				.AsList (x => x.Column ("order_in_route"));
		}
	}
}

