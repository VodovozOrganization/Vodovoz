using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class AdditionalFuelTypeMap : ClassMap<CarAdditionalFuelType>
	{
		public AdditionalFuelTypeMap()
		{
			Table("car_additional_fuel_types");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.Car)
				.Column("car_id");

			References(x => x.FuelType)
				.Column("fuel_type_id");
		}
	}
}
