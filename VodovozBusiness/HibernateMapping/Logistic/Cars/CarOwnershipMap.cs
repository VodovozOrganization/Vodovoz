using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class CarOwnershipMap : ClassMap<OwnershipModelCar>
	{
		public CarOwnershipMap()
		{
			Table("car_ownership");
			
			Id(x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.OwnershipCar).Column("ownership").CustomType<OwnershipCarStringType>();

			References(x => x.ModelCar).Column("car_model_id");
		}
	}
}
