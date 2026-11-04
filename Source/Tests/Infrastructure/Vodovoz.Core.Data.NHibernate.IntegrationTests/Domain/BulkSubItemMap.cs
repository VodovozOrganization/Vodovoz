using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkSubItemMap : ClassMap<BulkSubItem>
	{
		public BulkSubItemMap()
		{
			Table("bulk_sub_items");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Child).Column("child_id");
		}
	}
}
