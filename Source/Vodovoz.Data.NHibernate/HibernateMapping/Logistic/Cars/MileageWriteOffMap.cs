using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class MileageWriteOffMap : ClassMap<MileageWriteOff>
	{
		public MileageWriteOffMap()
		{
			Table("mileage_write_off");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.WriteOffDate).Column("write_off_date");
			Map(x => x.DistanceKm).Column("distance_km");
			Map(x => x.LitersOutlayed).Column("liters_outlayed");
			Map(x => x.Comment).Column("comment");

			References(x => x.Reason).Column("reason_id");
			References(x => x.Car).Column("car_id");
			References(x => x.Driver).Column("driver_id");
			References(x => x.Author).Column("author_id");
		}
	}
}
