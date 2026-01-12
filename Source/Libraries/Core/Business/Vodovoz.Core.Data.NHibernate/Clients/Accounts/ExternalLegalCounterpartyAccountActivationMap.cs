using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients.Accounts;

namespace Vodovoz.Core.Data.NHibernate.Clients.Accounts
{
	public class ExternalLegalCounterpartyAccountActivationMap : ClassMap<ExternalLegalCounterpartyAccountActivation>
	{
		public ExternalLegalCounterpartyAccountActivationMap()
		{
			Table("external_legal_counterparties_accounts_activations");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.AddingPhoneNumberState).Column("adding_phone_number_state");
			Map(x => x.AddingReasonForLeavingState).Column("adding_reason_for_leaving_state");
			Map(x => x.AddingEdoAccountState).Column("adding_edo_account_state");
			Map(x => x.TaxServiceCheckState).Column("tax_service_check_state");
			Map(x => x.TrueMarkCheckState).Column("true_mark_check_state");
			
			References(x => x.ExternalAccount).Column("account_id");
		}
	}
}
