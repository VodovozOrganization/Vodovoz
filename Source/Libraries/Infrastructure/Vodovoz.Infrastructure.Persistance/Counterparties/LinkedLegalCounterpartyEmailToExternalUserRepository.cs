using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Contacts;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	public class LinkedLegalCounterpartyEmailToExternalUserRepository : ILinkedLegalCounterpartyEmailToExternalUserRepository
	{
		public IEnumerable<LinkedLegalCounterpartyEmailToExternalUser> GetLinkedLegalCounterpartyEmails(IUnitOfWork uow, string emailAddress)
		{
			return (
				from linkedData in uow.Session.Query<LinkedLegalCounterpartyEmailToExternalUser>()
				join email in uow.Session.Query<Email>()
					on linkedData.LegalCounterpartyEmailId equals email.Id
				where email.Address == emailAddress
				select linkedData
				)
				.ToList();
		}
	}
}
