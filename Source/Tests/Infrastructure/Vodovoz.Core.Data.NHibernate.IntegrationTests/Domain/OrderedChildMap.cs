using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class OrderedChildMap : ClassMap<OrderedChild>
	{
		public OrderedChildMap()
		{
			Table("bulk_ordered_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
