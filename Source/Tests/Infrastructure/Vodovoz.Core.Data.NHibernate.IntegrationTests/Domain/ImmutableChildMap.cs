using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class ImmutableChildMap : ClassMap<ImmutableChild>
	{
		public ImmutableChildMap()
		{
			Table("bulk_immutable_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
