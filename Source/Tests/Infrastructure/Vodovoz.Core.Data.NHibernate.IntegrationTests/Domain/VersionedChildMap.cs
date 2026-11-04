using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class VersionedChildMap : ClassMap<VersionedChild>
	{
		public VersionedChildMap()
		{
			Table("bulk_versioned_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Version(x => x.Version).Column("version");
			Map(x => x.Name).Column("name");
		}
	}
}
