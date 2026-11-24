using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkTagMap : ClassMap<BulkTag>
	{
		public BulkTagMap()
		{
			Table("bulk_tags");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name").Unique();
		}
	}
}
