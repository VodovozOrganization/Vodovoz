using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Contacts;
using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.Repositories
{
	public interface IContactsRepository
	{
		IEnumerable<PhoneDto> GetLegalCounterpartyPhones(IUnitOfWork uow, int counterpartyId);
		IEnumerable<EmailDto> GetLegalCounterpartyEmails(IUnitOfWork uow, int counterpartyId);
	}
}
