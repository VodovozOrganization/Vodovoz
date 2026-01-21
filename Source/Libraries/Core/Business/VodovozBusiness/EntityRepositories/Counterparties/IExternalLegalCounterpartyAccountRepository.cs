using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Contacts;

namespace VodovozBusiness.EntityRepositories.Counterparties
{
	public interface IExternalLegalCounterpartyAccountRepository
	{
		IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(
			IUnitOfWork uow, int legalCounterpartyId, string emailAddress);
		IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(IUnitOfWork uow, string emailAddress);
		IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(IUnitOfWork uow, int legalCounterpartyId);
		IEnumerable<Email> GetExternalLegalCounterpartyAccountsEmails(IUnitOfWork uow, int legalCounterpartyId);
	}
}
