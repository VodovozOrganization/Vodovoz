using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class ExternalCounterpartyMatchingRepository : IExternalCounterpartyMatchingRepository
	{
		public bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber)
		{
			return uow.Session.QueryOver<ExternalCounterpartyMatching>()
				.Where(ecm => ecm.ExternalCounterpartyGuid == externalCounterpartyGuid)
				.And(ecm => ecm.PhoneNumber == phoneNumber)
				.List()
				.Any();
		}
	}
}
