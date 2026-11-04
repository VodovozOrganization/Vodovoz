using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class IncrementChildMap : ClassMap<IncrementChild>
	{
		public IncrementChildMap()
		{
			Table("bulk_increment_children");
			Id(x => x.Id).Column("id").GeneratedBy.Increment();
			Map(x => x.Name).Column("name");
		}
	}
}
