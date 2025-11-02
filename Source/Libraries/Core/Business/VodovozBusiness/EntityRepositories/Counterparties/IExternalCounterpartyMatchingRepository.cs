using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyMatchingRepository
	{
		bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber);
		IEnumerable<ExternalCounterpartyMatching> GetExternalCounterpartyMatching(
			IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber);
	}
}
