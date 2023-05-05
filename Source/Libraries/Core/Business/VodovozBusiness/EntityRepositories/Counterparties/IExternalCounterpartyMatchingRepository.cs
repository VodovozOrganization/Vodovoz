using System;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyMatchingRepository
	{
		bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber);
	}
}
