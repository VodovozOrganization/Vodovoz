using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;

namespace VodovozBusiness.EntityRepositories.Counterparties
{
	public interface IExternalLegalCounterpartyAccountRepository
	{
		IEnumerable<ExternalLegalCounterpartyAccount> GetLinkedLegalCounterpartyEmails(
			IUnitOfWork uow, int legalCounterpartyId, string emailAddress);

		IEnumerable<ExternalLegalCounterpartyAccountActivation> GetExternalLegalCounterpartyAccountsActivations(
			IUnitOfWork uow, Source source, Guid externalUserId, int legalCounterpartyId);
	}
}
