using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class CompositeChildMap : ClassMap<CompositeChild>
	{
		public CompositeChildMap()
		{
			Table("bulk_composite_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.KeyA).Column("key_a");
		}
	}
}
