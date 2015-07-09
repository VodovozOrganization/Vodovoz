using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Operations;
using NHibernate;


namespace Vodovoz.Repository
{
	public class CounterpartyContractRepository
	{
		public static CounterpartyContract GetCounterpartyContractByPaymentType (IUnitOfWork uow, Counterparty counterparty, Payment paymentType)
		{
			Organization organization = 
				(paymentType == Payment.cash 
				? OrganizationRepository.GetCashOrganization (uow)
				: OrganizationRepository.GetCashlessOrganization (uow));

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

