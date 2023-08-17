using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class OdometerReadingMap : ClassMap<OdometerReading>
	{
		public OdometerReadingMap()
		{
			Table("odometer_readings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.Odometer).Column("odometer");

			References(x => x.Car).Column("car_id");
		}
	}
}
