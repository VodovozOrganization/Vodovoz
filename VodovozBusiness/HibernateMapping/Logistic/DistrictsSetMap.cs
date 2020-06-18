using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
    public class DistrictsSetMap : ClassMap<DistrictsSet>
    {
		public DistrictsSetMap()
		{
			Table("districts_set");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			
			References(x => x.Creator).Column("creator_id");

			HasMany(x => x.Districts).Cascade.AllDeleteOrphan().Inverse().KeyColumn("districts_set_id");
		}
    }
}