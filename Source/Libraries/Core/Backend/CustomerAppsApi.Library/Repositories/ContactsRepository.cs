using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Contacts;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

namespace CustomerAppsApi.Library.Repositories
{
	public class ContactsRepository : IContactsRepository
	{
		public IEnumerable<PhoneDto> GetLegalCounterpartyPhones(IUnitOfWork uow, int counterpartyId)
		{
			return(
				from phone in uow.Session.Query<Phone>()
				where phone.Counterparty.Id == counterpartyId
				select new PhoneDto
				{
					Id = phone.Id,
					Number = phone.DigitsNumber,
				}
				).ToList();
		}
		
		public IEnumerable<EmailDto> GetLegalCounterpartyEmails(IUnitOfWork uow, int counterpartyId)
		{
			return(
				from email in uow.Session.Query<Email>()
				where email.Counterparty.Id == counterpartyId
				select new EmailDto
				{
					Id = email.Id,
					Address = email.Address,
				}
			).ToList();
		}
	}
}
