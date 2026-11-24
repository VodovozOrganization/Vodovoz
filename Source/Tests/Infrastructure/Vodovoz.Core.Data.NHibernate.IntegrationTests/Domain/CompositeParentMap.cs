using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class CompositeParentMap : ClassMap<CompositeParent>
	{
		public CompositeParentMap()
		{
			Table("bulk_composite_parents");
			CompositeId()
				.KeyProperty(x => x.KeyA, "key_a")
				.KeyProperty(x => x.KeyB, "key_b");
			HasMany(x => x.Items).KeyColumns.Add("key_a", "key_b").Inverse();
		}
	}
}
