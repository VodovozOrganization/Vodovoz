using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarFileInformationMap : ClassMap<CarFileInformation>
	{
		public CarFileInformationMap()
		{
			Table("car_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CarId).Column("car_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
