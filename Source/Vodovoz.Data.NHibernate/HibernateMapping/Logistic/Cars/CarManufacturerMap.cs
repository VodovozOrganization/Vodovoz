using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarManufacturerMap : ClassMap<CarManufacturer>
	{
		public CarManufacturerMap()
		{
			Table("car_manufacturers");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
