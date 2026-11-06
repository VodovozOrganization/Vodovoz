using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class VersionedParentChildMap : ClassMap<VersionedParentChild>
	{
		public VersionedParentChildMap()
		{
			Table("bulk_versioned_parent_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			References(x => x.Parent).Column("parent_id");
		}
	}
}
