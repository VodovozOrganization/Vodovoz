using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class GuidChildMap : ClassMap<GuidChild>
	{
		public GuidChildMap()
		{
			Table("bulk_guid_children");
			Id(x => x.Id).Column("id").GeneratedBy.Guid();
			Map(x => x.Name).Column("name");
		}
	}
}
