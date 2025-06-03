using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class FuelTypeMap : ClassMap<FuelType>
	{
		public FuelTypeMap()
		{
			Table("fuel_types");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");

			HasMany(x => x.FuelPriceVersions).Cascade.AllDeleteOrphan().Inverse().KeyColumn("fuel_type_id")
				.OrderBy("start_date DESC");
		}
	}
}

