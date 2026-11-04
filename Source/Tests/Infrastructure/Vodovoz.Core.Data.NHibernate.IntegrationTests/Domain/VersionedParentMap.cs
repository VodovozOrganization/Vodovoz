using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class VersionedParentMap : ClassMap<VersionedParent>
	{
		public VersionedParentMap()
		{
			Table("bulk_versioned_parents");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Version(x => x.Version).Column("version");
			Map(x => x.Name).Column("name");
			HasMany(x => x.Items).KeyColumn("parent_id").Inverse();
		}
	}
}
