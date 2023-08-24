using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class CounterpartyEdoOperatorMap : ClassMap<CounterpartyEdoOperator>
	{
		public CounterpartyEdoOperatorMap()
		{
			Table("counterparty_edo_operators");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PersonalAccountIdInEdo).Column("personal_account_id_in_edo");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.EdoOperator).Column("edo_operator_id");
		}
	}
}
