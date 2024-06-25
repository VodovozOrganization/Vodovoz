using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class ExternalCounterpartyMatchingRepository : IExternalCounterpartyMatchingRepository
	{
		public bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber)
		{
			return GetExternalCounterpartyMatching(uow, externalCounterpartyGuid, phoneNumber).Any();
		}

		public IEnumerable<ExternalCounterpartyMatching> GetExternalCounterpartyMatching(
			IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber)
		{
			return uow.Session.QueryOver<ExternalCounterpartyMatching>()
				.Where(ecm => ecm.ExternalCounterpartyGuid == externalCounterpartyGuid)
				.And(ecm => ecm.PhoneNumber == phoneNumber)
				.List();
		}
	}
}
