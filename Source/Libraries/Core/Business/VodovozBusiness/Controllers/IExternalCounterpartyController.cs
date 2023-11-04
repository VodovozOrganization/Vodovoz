using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Controllers
{
	public interface IExternalCounterpartyController
	{
		bool ArchiveExternalCounterparty(IUnitOfWork uow, int phoneId);
		void ArchiveExternalCounterparty(IList<ExternalCounterparty> externalCounterparties);
		bool HasActiveExternalCounterparties(IUnitOfWork uow, int phoneId, out IList<ExternalCounterparty> externalCounterparties);
	}
}
