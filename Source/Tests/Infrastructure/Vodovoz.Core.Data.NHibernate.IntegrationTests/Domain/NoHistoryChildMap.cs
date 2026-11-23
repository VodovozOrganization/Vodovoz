using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class NoHistoryChildMap : ClassMap<NoHistoryChild>
	{
		public NoHistoryChildMap()
		{
			Table("history_test_no_history_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Parent).Column("parent_id");
			Map(x => x.Name).Column("name");
		}
	}
}
