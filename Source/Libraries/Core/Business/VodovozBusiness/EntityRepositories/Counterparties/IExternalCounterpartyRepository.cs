using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Nodes;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyRepository
	{
		ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, Guid externalCounterpartyId, CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow, Guid externalCounterpartyId, string phoneNumber, CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, string phoneNumber, CounterpartyFrom counterpartyFrom);
		IEnumerable<ExternalCounterparty> GetActiveExternalCounterpartiesByPhone(IUnitOfWork uow, int phoneId);
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(IUnitOfWork uow, int counterpartyId);
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(IUnitOfWork uow, IEnumerable<int> phoneId);
		IList<ExternalCounterparty> GetExternalCounterpartyByEmail(IUnitOfWork uow, int emailId);
	}
}
