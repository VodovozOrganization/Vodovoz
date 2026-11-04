using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class JoinedChildMap : SubclassMap<JoinedChild>
	{
		public JoinedChildMap()
		{
			Table("bulk_joined_children");
			KeyColumn("id");
			Map(x => x.Extra).Column("extra");
		}
	}
}
