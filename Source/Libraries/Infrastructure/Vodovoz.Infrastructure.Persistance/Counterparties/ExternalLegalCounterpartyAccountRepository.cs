using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Contacts;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	public class ExternalLegalCounterpartyAccountRepository : IExternalLegalCounterpartyAccountRepository
	{
		public IEnumerable<ExternalLegalCounterpartyAccount> GetLinkedLegalCounterpartyEmails(
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
		
		public IEnumerable<ExternalLegalCounterpartyAccountActivation> GetExternalLegalCounterpartyAccountsActivations(
			IUnitOfWork uow, Source source, Guid externalUserId, int legalCounterpartyId)
		{
			return (
					from accountActivation in uow.Session.Query<ExternalLegalCounterpartyAccountActivation>()
					join account in uow.Session.Query<ExternalLegalCounterpartyAccount>()
						on accountActivation.ExternalAccount.Id equals account.Id
					where account.Source == source
						&& account.ExternalUserId == externalUserId
						&& account.LegalCounterpartyId == legalCounterpartyId 
					select accountActivation
				)
				.ToList();
		}
	}
}
