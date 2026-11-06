using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class IdentityChildMap : ClassMap<IdentityChild>
	{
		public IdentityChildMap()
		{
			Table("bulk_identity_children");
			Id(x => x.Id).Column("id").GeneratedBy.Identity();
			Map(x => x.Name).Column("name");
		}
	}
}
