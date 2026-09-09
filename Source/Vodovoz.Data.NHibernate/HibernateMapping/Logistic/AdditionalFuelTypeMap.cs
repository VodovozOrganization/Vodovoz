using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class AdditionalFuelTypeMap : ClassMap<AdditionalFuelType>
	{
		public AdditionalFuelTypeMap()
		{
			Table("additional_fuel_types");

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
