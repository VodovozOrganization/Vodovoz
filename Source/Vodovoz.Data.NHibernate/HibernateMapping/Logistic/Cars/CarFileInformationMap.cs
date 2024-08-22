using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	// TODO: Отключено до реализации 4963, мешает сборке
	//public class CarFileInformationMap : ClassMap<CarFileInformation>
	//{
	//	public CarFileInformationMap()
	//	{
	//		Table("car_file_informations");

	//		Id(x => x.Id).Column("id").GeneratedBy.Native();

	//		Map(x => x.CarId).Column("car_id");
	//		Map(x => x.FileName).Column("file_name");
	//	}
	//}
}
