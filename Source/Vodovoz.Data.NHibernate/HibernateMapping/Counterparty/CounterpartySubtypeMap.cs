using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class CounterpartySubtypeMap : ClassMap<CounterpartySubtype>
	{
		public CounterpartySubtypeMap()
		{
			Table("counterparty_subtypes");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
