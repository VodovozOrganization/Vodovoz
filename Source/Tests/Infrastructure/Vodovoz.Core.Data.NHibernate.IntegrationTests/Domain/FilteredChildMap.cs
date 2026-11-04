using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class FilteredChildMap : ClassMap<FilteredChild>
	{
		public FilteredChildMap()
		{
			Table("bulk_filtered_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
