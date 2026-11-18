using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class HistorySubItemMap : ClassMap<HistorySubItem>
	{
		public HistorySubItemMap()
		{
			Table("history_test_sub_items");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Child).Column("child_id");
		}
	}
}
