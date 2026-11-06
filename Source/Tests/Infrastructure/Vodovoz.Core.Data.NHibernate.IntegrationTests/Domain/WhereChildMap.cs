using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class WhereChildMap : ClassMap<WhereChild>
	{
		public WhereChildMap()
		{
			Table("bulk_where_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
