using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class ManufacturerCarsMap: ClassMap<ManufacturerCars>
	{
		public ManufacturerCarsMap()
		{
			Table("manufacturer_of_cars");
			
			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
		}
	}
}
