using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class CarModelMap : ClassMap<CarModel>
	{
		public CarModelMap()
		{
			Table("car_model");
			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.IsArchive)              .Column ("is_archive");
			Map(x => x.TypeOfUse)           .Column ("type_of_cars").CustomType<CarTypeOfUseStringType>();
			Map(x => x.MaxVolume)              .Column ("max_volume");
			Map(x => x.MaxWeight)              .Column ("max_weight");

			References(x => x.ManufacturerCars).Column("manufacturer_id");
		}
	}
}
