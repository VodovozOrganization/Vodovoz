using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface ICounterpartyContractRepository
    {
		IList<CounterpartyContract> GetActiveContractsWithOrganization(
			IUnitOfWork uow, Counterparty counterparty, Organization org, ContractType type, bool issueDateDesc = false);
	}
}
