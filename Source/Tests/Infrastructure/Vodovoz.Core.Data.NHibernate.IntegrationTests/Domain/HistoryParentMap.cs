using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class HistoryParentMap : ClassMap<HistoryParent>
	{
		public HistoryParentMap()
		{
			Table("history_test_parents");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			HasMany(x => x.Items).KeyColumn("parent_id").Inverse().Cascade.AllDeleteOrphan();
			HasMany(x => x.IdTitleItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.NoHistoryItems).KeyColumn("parent_id").Inverse();
		}
	}
}
