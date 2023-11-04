using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Controllers
{
	public interface IExternalCounterpartyController
	{
		bool ArchiveExternalCounterparties(IUnitOfWork uow, int phoneId);
		void ArchiveExternalCounterparties(IEnumerable<ExternalCounterparty> externalCounterparties);
		void UndoArchiveExternalCounterparties(IEnumerable<ExternalCounterparty> externalCounterparties);
		bool HasActiveExternalCounterparties(IUnitOfWork uow, int phoneId, out IList<ExternalCounterparty> externalCounterparties);
	}
}
