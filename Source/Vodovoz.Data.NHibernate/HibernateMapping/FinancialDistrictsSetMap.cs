using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping
{
	public class FinancialDistrictsSetMap : ClassMap<FinancialDistrictsSet>
	{
		public FinancialDistrictsSetMap()
		{
			Table("financial_districts_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.DateCreated).Column("date_created");
			Map(x => x.DateActivated).Column("date_activated");
			Map(x => x.DateClosed).Column("date_closed");
			Map(x => x.Status).Column("status");

			References(x => x.Author).Column("author_id");

			HasMany(x => x.FinancialDistricts).Cascade.AllDeleteOrphan().Inverse().KeyColumn("financial_districts_set_id");
		}
	}
}
