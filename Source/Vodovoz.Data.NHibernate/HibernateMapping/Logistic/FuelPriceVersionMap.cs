using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class FuelPriceVersionMap : ClassMap<FuelPriceVersion>
	{
		public FuelPriceVersionMap()
		{
			Table("fuel_price_version");
			
			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.FuelPrice).Column("fuel_price");

			References(x => x.FuelType).Column("fuel_type_id");
		}
	}
}
