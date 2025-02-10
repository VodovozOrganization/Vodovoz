using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public interface IExternalCounterpartyController
	{
		string PhoneAssignedExternalCounterpartyMessage { get; }
		bool DeleteExternalCounterparties(IUnitOfWork uow, int phoneId);
		void DeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<ExternalCounterparty> externalCounterparties);
		bool HasActiveExternalCounterparties(IUnitOfWork uow, int phoneId, out IEnumerable<ExternalCounterparty> externalCounterparties);
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(IUnitOfWork uow, int counterpartyId);
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(IUnitOfWork uow, IEnumerable<int> phonesIds);
	}
}
