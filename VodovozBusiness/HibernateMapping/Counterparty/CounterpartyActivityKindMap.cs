using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Hibernate.Counterparty
{
	public class CounterpartyActivityKindMap : ClassMap<CounterpartyActivityKind>
	{
		public CounterpartyActivityKindMap()
		{
			Table("counterparty_activity_kinds");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Substrings).Column("substrings");
		}
	}
}