using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarFileInformationMap : ClassMap<CarFileInformation>
	{
		public CarFileInformationMap()
		{
			Table("car_fuel_versions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CarId).Column("car_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
