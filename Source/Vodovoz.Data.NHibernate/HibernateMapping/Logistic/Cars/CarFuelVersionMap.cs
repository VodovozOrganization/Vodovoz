using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarFuelVersionMap : ClassMap<CarFuelVersion>
	{
		public CarFuelVersionMap()
		{
			Table("car_fuel_versions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.FuelConsumption).Column("fuel_consumption");

			References(x => x.CarModel).Column("car_model_id");
		}
	}
}
