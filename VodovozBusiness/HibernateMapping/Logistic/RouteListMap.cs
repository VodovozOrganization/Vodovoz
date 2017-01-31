using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
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
			Map (x => x.ClosingFilled).Column("closing_filled");

			References (x => x.Car).Column ("car_id");
			References (x => x.Shift).Column ("delivery_shift_id");
			References (x => x.Driver).Column ("driver_id");
			References (x => x.Forwarder).Column ("forwarder_id");
			References (x => x.Logistican).Column ("logistican_id");
			References (x => x.BottleFine).Column("bottles_fine_id");
			References (x => x.Cashier).Column("cashier_id");
			References (x => x.FuelOutlayedOperation).Column("fuel_outlayed_operation_id").Cascade.All();
			References (x => x.FuelGivedDocument).Column("fuel_gived_document_id");

			HasMany (x => x.Addresses).Cascade.AllDeleteOrphan ().Inverse ()
				.KeyColumn ("route_list_id")
				.AsList (x => x.Column ("order_in_route"));
		}
	}
}

