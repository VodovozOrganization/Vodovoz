using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class ModelCarMap : ClassMap<ModelCar>
	{
		public ModelCarMap()
		{
			Table("car_model");
			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.IsArchive)              .Column ("is_archive");
			Map(x => x.CarTypeOfUse)           .Column ("type_of_cars").CustomType<CarTypeOfUseStringType>();
			Map(x => x.MaxVolume)              .Column ("max_volume");
			Map(x => x.MaxWeight)              .Column ("max_weight");

			References(x => x.ManufacturerCars).Column("manufacturer_id");
		}
	}
}
