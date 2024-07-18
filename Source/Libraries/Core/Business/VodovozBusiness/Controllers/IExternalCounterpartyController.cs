using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Nodes;

namespace Vodovoz.Controllers
{
	public interface IExternalCounterpartyController
	{
		string PhoneAssignedExternalCounterpartyMessage { get; }
		bool TryDeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<int> externalCounterpartiesIds, bool ask = false);
		bool HasActiveExternalCounterparties(IEnumerable<int> externalCounterpartiesIds);
		bool CanArchiveOrDeletePhone(IEnumerable<int> externalCounterpartiesIds);
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByCounterparty(IUnitOfWork uow, int counterpartyId);
		IEnumerable<ExternalCounterpartyNode> GetActiveExternalCounterpartiesByPhones(IUnitOfWork uow, IEnumerable<int> phonesIds);
		void TryCreateNotifications(IUnitOfWork uow);
	}
}
