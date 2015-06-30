using QSOrmProject;
using Vodovoz.Domain;


namespace Vodovoz.Repository
{
	public class CounterpartyContractRepository
	{
		public static CounterpartyContract GetCounterpartyContract (IUnitOfWorkGeneric<Order> uow)
		{
			Order order = uow.RootObject as Order;
			Organization organization = 
				(order.PaymentType == Payment.cash 
				? OrganizationRepository.GetCashOrganization (uow)
				: OrganizationRepository.GetCashlessOrganization (uow));

			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;

			return uow.Session.QueryOver<CounterpartyContract> ()
				.JoinAlias (co => co.Counterparty, () => counterpartyAlias)
				.JoinAlias (co => co.Organization, () => organizationAlias)
				.Where (co => (counterpartyAlias.Id == order.Client.Id &&
			!co.IsArchive &&
			!co.OnCancellation &&
			organizationAlias.Id == organization.Id))
				.SingleOrDefault ();
		}
	}
}

