using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Contacts;

namespace VodovozBusiness.EntityRepositories.Counterparties
{
	public interface IExternalLegalCounterpartyAccountRepository
	{
		IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(
			IUnitOfWork uow, int legalCounterpartyId, string emailAddress);
		
		IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(
			IUnitOfWork uow, Source source, Guid externalUserId, string emailAddress);

		IEnumerable<Email> GetExternalLegalCounterpartyAccountsEmails(IUnitOfWork uow, int legalCounterpartyId);

		IEnumerable<ExternalLegalCounterpartyAccountActivation> GetExternalLegalCounterpartyAccountsActivations(
			IUnitOfWork uow, Source source, Guid externalUserId, int legalCounterpartyId);
	}
}
