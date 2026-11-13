using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class HistoryIdTitleChildMap : ClassMap<HistoryIdTitleChild>
	{
		public HistoryIdTitleChildMap()
		{
			Table("history_test_id_title_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Parent).Column("parent_id");
			Map(x => x.Name).Column("name");
		}
	}
}
