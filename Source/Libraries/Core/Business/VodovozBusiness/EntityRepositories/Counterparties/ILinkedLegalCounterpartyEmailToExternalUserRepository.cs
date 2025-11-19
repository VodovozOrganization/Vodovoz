using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain;

namespace VodovozBusiness.EntityRepositories.Counterparties
{
	public interface ILinkedLegalCounterpartyEmailToExternalUserRepository
	{
		IEnumerable<LinkedLegalCounterpartyEmailToExternalUser> GetLinkedLegalCounterpartyEmails(IUnitOfWork uow, string emailAddress);
	}
}
