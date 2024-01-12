using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Controllers
{
	public interface IExternalCounterpartyController
	{
		bool DeleteExternalCounterparties(IUnitOfWork uow, int phoneId);
		void DeleteExternalCounterparties(IUnitOfWork uow, IEnumerable<ExternalCounterparty> externalCounterparties);
		bool HasActiveExternalCounterparties(IUnitOfWork uow, int phoneId, out IEnumerable<ExternalCounterparty> externalCounterparties);
	}
}
