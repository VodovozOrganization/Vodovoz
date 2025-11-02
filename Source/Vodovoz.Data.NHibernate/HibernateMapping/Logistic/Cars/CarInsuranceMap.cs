using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarInsuranceMap : ClassMap<CarInsurance>
	{
		public CarInsuranceMap()
		{
			Table("car_insurances");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.InsuranceNumber).Column("insurance_number");
			Map(x => x.InsuranceType).Column("insurance_type");

			References(x => x.Car).Column("car_id");
			References(x => x.Insurer).Column("insurer_id");
		}
	}
}
