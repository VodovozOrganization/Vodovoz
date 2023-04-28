using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Controllers
{
	public class ContactManagerForExternalCounterparty : IContactManagerForExternalCounterparty
	{
		private readonly IPhoneRepository _phoneRepository;
		private readonly IContactFinderForExternalCounterparty _contactFinderForExternalCounterpartyFromOne;
		private readonly IContactFinderForExternalCounterparty _contactFinderForExternalCounterpartyFromTwo;
		private readonly IContactFinderForExternalCounterparty _contactFinderForExternalCounterpartyFromMany;

		public ContactManagerForExternalCounterparty(
			IPhoneRepository phoneRepository,
			ContactFinderForExternalCounterpartyFromOne contactFinderForExternalCounterpartyFromOne,
			ContactFinderForExternalCounterpartyFromTwo contactFinderForExternalCounterpartyFromTwo,
			ContactFinderForExternalCounterpartyFromMany contactFinderForExternalCounterpartyFromMany)
		{
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_contactFinderForExternalCounterpartyFromOne =
				contactFinderForExternalCounterpartyFromOne
					?? throw new ArgumentNullException(nameof(contactFinderForExternalCounterpartyFromOne));
			_contactFinderForExternalCounterpartyFromTwo =
				contactFinderForExternalCounterpartyFromTwo
					?? throw new ArgumentNullException(nameof(contactFinderForExternalCounterpartyFromTwo));
			_contactFinderForExternalCounterpartyFromMany =
				contactFinderForExternalCounterpartyFromMany
					?? throw new ArgumentNullException(nameof(contactFinderForExternalCounterpartyFromMany));

			SetSuccessorsForHandlers();
		}

		/// <summary>
		/// Ищем контакт клиента по номеру телефона
		/// Если контактов не найдено - отправляем <see cref="FoundContact"/> с соответствующим статусом,
		/// если контакт один и он принадлежит контрагенту физическому лицу, возвращаем этот контакт,
		/// если контакта два и один из них принадлежит контрагенту физическому лицу, а второй его точке доставки, возвращаем контакт клиента,
		/// если контактов больше 2 и один принадлежит контрагенту физическому лицу, а остальные его точкам доставки, возвращаем контакт клиента,
		/// иначе нужна ручнуя обработка
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phoneNumber">номер телефона</param>
		/// <returns>найденный контакт <see cref="FoundContact"/></returns>
		public FoundContact FindContactForRegisterExternalCounterparty(IUnitOfWork uow, string phoneNumber)
		{
			var contacts = GetPhonesByNumber(uow, phoneNumber);
			
			if(!contacts.Any())
			{
				return new FoundContact
				{
					FoundContactStatus = FoundContactStatus.ContactNotFound
				};
			}

			return _contactFinderForExternalCounterpartyFromOne.FindContact(contacts);
		}
		
		private IList<Phone> GetPhonesByNumber(IUnitOfWork uow, string phoneNumber)
		{
			return _phoneRepository.GetPhonesByNumber(uow, phoneNumber);
		}
		
		private void SetSuccessorsForHandlers()
		{
			_contactFinderForExternalCounterpartyFromOne.SetNextHandler(_contactFinderForExternalCounterpartyFromTwo);
			_contactFinderForExternalCounterpartyFromTwo.SetNextHandler(_contactFinderForExternalCounterpartyFromMany);
		}
	}
}
