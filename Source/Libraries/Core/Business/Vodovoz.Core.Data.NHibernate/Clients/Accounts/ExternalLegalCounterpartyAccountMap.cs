using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients.Accounts;

namespace Vodovoz.Core.Data.NHibernate.Clients.Accounts
{
	public class ExternalLegalCounterpartyAccountMap : ClassMap<ExternalLegalCounterpartyAccount>
	{
		public ExternalLegalCounterpartyAccountMap()
		{
			Table("external_legal_counterparties_accounts");
			
			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Source).Column("source");
			Map(x => x.ExternalUserId).Column("external_user_id");
			Map(x => x.LegalCounterpartyId).Column("legal_counterparty_id");
			Map(x => x.LegalCounterpartyEmailId).Column("legal_counterparty_email_id");
			Map(x => x.AccountPasswordSalt).Column("account_password_salt");
			Map(x => x.AccountPasswordHash).Column("account_password_hash");
			
			References(x => x.AccountActivation).Column("account_activation_id");
		}
	}
}
