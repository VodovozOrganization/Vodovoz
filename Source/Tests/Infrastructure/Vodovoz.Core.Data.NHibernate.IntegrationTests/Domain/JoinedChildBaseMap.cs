using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class JoinedChildBaseMap : ClassMap<JoinedChildBase>
	{
		public JoinedChildBaseMap()
		{
			Table("bulk_joined_base");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
