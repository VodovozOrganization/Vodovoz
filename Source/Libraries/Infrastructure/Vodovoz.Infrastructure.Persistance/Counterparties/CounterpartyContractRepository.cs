using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class CounterpartyContractRepository : ICounterpartyContractRepository
	{
		public IList<CounterpartyContract> GetActiveContractsWithOrganization(
			IUnitOfWork uow,
			Counterparty counterparty,
			Organization org,
			ContractType type,
			bool issueDateDesc = false)
		{
			var query =
				from contract in uow.Session.Query<CounterpartyContract>()
				where counterparty.Id == contract.Counterparty.Id
					&& !contract.IsArchive
					&& !contract.OnCancellation
					&& org.Id == contract.Organization.Id
					&& contract.ContractType == type
					select contract;

			if(issueDateDesc)
			{
				return query
					.OrderByDescending(x => x.IssueDate)
					.ToList();
			}
			
			return query.ToList();
		}
	}
}

