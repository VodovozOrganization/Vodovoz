using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Nodes;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.EntityRepositories.Counterparties
{
	public interface IConnectedCustomerRepository
	{
		IEnumerable<ConnectedCustomerInfoNode> GetConnectedCustomersInfo(IUnitOfWork uow, int counterpartyId, PersonType personType);
		IEnumerable<LegalCounterpartyInfo> GetConnectedCustomers(IUnitOfWork uow, int counterpartyId);
		ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId);
	}
}
