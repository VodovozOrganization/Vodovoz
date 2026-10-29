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

		/// <inheritdoc/>
		public IList<ExternalCounterpartyMatching> GetForExternalCounterparties(
			IUnitOfWork uow, IEnumerable<int> externalCounterpartyIds)
		{
			var ids = externalCounterpartyIds.ToArray();
			if(ids.Length == 0)
			{
				return new List<ExternalCounterpartyMatching>();
			}

			var externalCounterparties = uow.Session.Query<ExternalCounterparty>()
				.Where(ec => ids.Contains(ec.Id));

			return uow.Session.Query<ExternalCounterpartyMatching>()
				.Where(m => (m.AssignedExternalCounterparty != null
						&& ids.Contains(m.AssignedExternalCounterparty.Id))
					|| (m.AssignedExternalCounterparty == null
						&& externalCounterparties.Any(ec => ec.ExternalCounterpartyId == m.ExternalCounterpartyGuid
							&& ec.CounterpartyFrom == m.CounterpartyFrom)))
				.ToList();
		}
	}
}
