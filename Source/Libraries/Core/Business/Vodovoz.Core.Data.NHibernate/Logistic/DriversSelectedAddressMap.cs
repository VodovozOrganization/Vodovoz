using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Logistic
{
	public class DriversSelectedAddressMap : ClassMap<DriversSelectedAddress>
	{
		public DriversSelectedAddressMap()
		{
			Table("drivers_selected_addresses");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.DriverId)
				.Column("driver_id");

			Map(x => x.NextAddressId)
				.Column("next_address_id");

			Map(x => x.PreviousUncompletedAddressId)
				.Column("previous_uncompleted_address_id");

			Map(x => x.SelectedAt)
				.Column("selected_at");
		}
	}
}
