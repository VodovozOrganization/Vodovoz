using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class FuelCardVersionMap : ClassMap<FuelCardVersion>
	{
		public FuelCardVersionMap()
		{
			Table("fuel_card_versions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");

			References(x => x.Car).Column("car_id");
			References(x => x.FuelCard).Column("fuel_card_id");
		}
	}
}
