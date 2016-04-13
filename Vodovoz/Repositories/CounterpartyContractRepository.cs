using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository
{
	public class CounterpartyContractRepository
	{
		public static CounterpartyContract GetCounterpartyContractByPaymentType (IUnitOfWork uow, Counterparty counterparty, PaymentType paymentType)
		{
			Organization organization = OrganizationRepository.GetOrganizationByPaymentType (uow,paymentType);

			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;

			return uow.Session.QueryOver<CounterpartyContract> ()
				.JoinAlias (co => co.Counterparty, () => counterpartyAlias)
				.JoinAlias (co => co.Organization, () => organizationAlias)
				.Where (co => (counterpartyAlias.Id == counterparty.Id &&
			!co.IsArchive &&
			!co.OnCancellation &&
			organizationAlias.Id == organization.Id))
				.SingleOrDefault ();
		}
	}
}

