using FluentNHibernate.Mapping;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Data.NHibernate.HibernateMapping.StoredEmails
{
	public class AuthorizationCodeEmailMap : SubclassMap<AuthorizationCodeEmail>
	{
		public AuthorizationCodeEmailMap()
		{
			DiscriminatorValue(nameof(CounterpartyEmailType.AuthorizationCode));
			
			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_id");
		}
	}
}
