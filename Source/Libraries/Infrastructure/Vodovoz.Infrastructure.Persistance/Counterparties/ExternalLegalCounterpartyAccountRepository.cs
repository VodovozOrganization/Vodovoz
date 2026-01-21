using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Contacts;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	public class ExternalLegalCounterpartyAccountRepository : IExternalLegalCounterpartyAccountRepository
	{
		public IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(
			IUnitOfWork uow, int legalCounterpartyId, string emailAddress)
		{
			return (
				from linkedData in uow.Session.Query<ExternalLegalCounterpartyAccount>()
				join email in uow.Session.Query<Email>()
					on linkedData.LegalCounterpartyEmailId equals email.Id
				where email.Address == emailAddress
					&& linkedData.LegalCounterpartyId == legalCounterpartyId 
				select linkedData
				)
				.ToList();
		}

		public IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(IUnitOfWork uow, string emailAddress)
		{
			return (
					from linkedData in uow.Session.Query<ExternalLegalCounterpartyAccount>()
					join email in uow.Session.Query<Email>()
						on linkedData.LegalCounterpartyEmailId equals email.Id
					where email.Address == emailAddress
					select linkedData
				)
				.ToList();
		}
		
		public IEnumerable<ExternalLegalCounterpartyAccount> GetExternalLegalCounterpartyAccounts(IUnitOfWork uow, int legalCounterpartyId)
		{
			return (
					from linkedData in uow.Session.Query<ExternalLegalCounterpartyAccount>()
					join email in uow.Session.Query<Email>()
						on linkedData.LegalCounterpartyEmailId equals email.Id
					where linkedData.LegalCounterpartyId == legalCounterpartyId 
					select linkedData
				)
				.ToList();
		}
		
		public IEnumerable<Email> GetExternalLegalCounterpartyAccountsEmails(IUnitOfWork uow, int legalCounterpartyId)
		{
			return (
					from linkedData in uow.Session.Query<ExternalLegalCounterpartyAccount>()
					join email in uow.Session.Query<Email>()
						on linkedData.LegalCounterpartyEmailId equals email.Id
					where linkedData.LegalCounterpartyId == legalCounterpartyId 
					select email
				)
				.ToList();
		}
	}
}
