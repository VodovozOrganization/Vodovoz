using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class CarModelMap : ClassMap<CarModel>
	{
		public CarModelMap()
		{
			Table("car_models");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.CarTypeOfUse).Column("car_type_of_use").CustomType<CarTypeOfUseStringType>();
			Map(x => x.MaxVolume).Column("max_volume");
			Map(x => x.MaxWeight).Column("max_weight");

			References(x => x.CarManufacturer).Column("manufacturer_id");
		}
	}
}
