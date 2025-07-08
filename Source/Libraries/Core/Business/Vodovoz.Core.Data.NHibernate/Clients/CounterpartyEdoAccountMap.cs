using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class CounterpartyEdoAccountMap : ClassMap<CounterpartyEdoAccountEntity>
	{
		public CounterpartyEdoAccountMap()
		{
			Table(CounterpartyEdoAccountEntity.TableName);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.IsDefault).Column("is_default");
			Map(x => x.PersonalAccountIdInEdo).Column("personal_account_id_in_edo");
			Map(x => x.OrganizationId).Column("organization_id");
			Map(x => x.ConsentForEdoStatus).Column("consent_for_edo_status");

			References(x => x.EdoOperator).Column("edo_operator_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}
