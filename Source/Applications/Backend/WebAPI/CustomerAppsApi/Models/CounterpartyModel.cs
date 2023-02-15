using System;
using CustomerAppsApi.Controllers;
using CustomerAppsApi.Dto;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Validators;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Parameters;

namespace CustomerAppsApi.Models
{
	public class CounterpartyModel : ICounterpartyModel
	{
		private readonly IUnitOfWork _uow;
		private readonly IPhoneRepository _phoneRepository;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly RoboatsSettings _roboatsSettings;
		private readonly CounterpartyModelFactory _counterpartyModelFactory;
		private readonly CounterpartyModelValidator _counterpartyModelValidator;
		private readonly IContactManagerForExternalCounterparty _contactManagerForExternalCounterparty;

		public CounterpartyModel(
			IUnitOfWork uow,
			IPhoneRepository phoneRepository,
			IRoboatsRepository roboatsRepository,
			IEmailRepository emailRepository,
			RoboatsSettings roboatsSettings,
			CounterpartyModelFactory counterpartyModelFactory,
			CounterpartyModelValidator counterpartyModelValidator,
			IContactManagerForExternalCounterparty contactManagerForExternalCounterparty)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_counterpartyModelFactory = counterpartyModelFactory ?? throw new ArgumentNullException(nameof(counterpartyModelFactory));
			_counterpartyModelValidator = counterpartyModelValidator ?? throw new ArgumentNullException(nameof(counterpartyModelValidator));
			_contactManagerForExternalCounterparty =
				contactManagerForExternalCounterparty ?? throw new ArgumentNullException(nameof(contactManagerForExternalCounterparty));
		}

		public CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			var validationResult = _counterpartyModelValidator.CounterpartyContactInfoDtoValidate(counterpartyContactInfoDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				return _counterpartyModelFactory.CreateErrorCounterpartyIdentificationDto(validationResult);
			}
			
			//Ищем зарегистрированного клиента по ExternalId
			var externalCounterparty =
				_phoneRepository.GetExternalCounterparty(
					_uow, counterpartyContactInfoDto.ExternalCounterpartyId, counterpartyContactInfoDto.CounterpartyFrom);

			if(externalCounterparty != null)
			{
				return _counterpartyModelFactory.CreateSuccessCounterpartyIdentificationDto(externalCounterparty);
			}

			/*
			 * Ищем зарегистрированного клиента с другой площадки
			 * если запрос пришел от мобилки, то смотрим клиента с таким номером телефона, зарегистрированного через сайт
			 * и наоборот, если запрос с сайта - ищем зарегистрированного через мобилку
			 */
			var phoneNumber = new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(counterpartyContactInfoDto.PhoneNumber);
			var counterpartyFrom =
				counterpartyContactInfoDto.CounterpartyFrom == CounterpartyFrom.MobileApp
				? CounterpartyFrom.WebSite
				: CounterpartyFrom.MobileApp;
			
			externalCounterparty = _phoneRepository.GetExternalCounterparty(_uow, phoneNumber, counterpartyFrom);

			if(externalCounterparty != null)
			{
				//Создаем нужный контакт и отправляем данные в ИПЗ
				var copiedExternalCounterparty =
					_counterpartyModelFactory.CopyToOtherExternalCounterparty(externalCounterparty,
						counterpartyContactInfoDto.ExternalCounterpartyId);

				_uow.Save(copiedExternalCounterparty);
				_uow.Commit();
				
				return _counterpartyModelFactory.CreateRegisteredCounterpartyIdentificationDto(externalCounterparty);
			}

			var contact = _contactManagerForExternalCounterparty.FindContactForRegisterExternalCounterparty(_uow, phoneNumber);

			switch(contact.FoundContactStatus)
			{
				case FoundContactStatus.Success:
					externalCounterparty = _counterpartyModelFactory.CreateExternalCounterparty(counterpartyContactInfoDto.CounterpartyFrom);
					externalCounterparty.ExternalCounterpartyId = counterpartyContactInfoDto.ExternalCounterpartyId;
					externalCounterparty.Phone = contact.Phone;
					externalCounterparty.Email = GetCounterpartyEmailForExternalCounterparty(contact.Phone.Counterparty.Id);
					
					_uow.Save(externalCounterparty);
					_uow.Commit();
					
					return _counterpartyModelFactory.CreateRegisteredCounterpartyIdentificationDto(externalCounterparty);
				case FoundContactStatus.ContactNotFound:
					return _counterpartyModelFactory.CreateNotFoundCounterpartyIdentificationDto();
				default:
					return _counterpartyModelFactory.CreateNeedManualHandlingCounterpartyIdentificationDto();
			}
		}

		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto)
		{
			var validationResult = _counterpartyModelValidator.CounterpartyDtoValidate(counterpartyDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				return _counterpartyModelFactory.CreateErrorCounterpartyRegistrationDto(validationResult);
			}

			var counterpartyRegistrationDto = CheckExternalCounterpartyWithSameExternalId(counterpartyDto);

			if(counterpartyRegistrationDto != null)
			{
				return counterpartyRegistrationDto;
			}

			counterpartyRegistrationDto = CheckExternalCounterpartyWithSamePhoneNumber(counterpartyDto);
			
			if(counterpartyRegistrationDto != null)
			{
				return counterpartyRegistrationDto;
			}
			
			//Создаем нового контрагента и валидируем полученную сущность
			var counterparty = new Counterparty();
			
			switch(counterpartyDto.PersonType)
			{
				case PersonType.legal:
					counterparty.Name = counterpartyDto.Name;
					counterparty.FullName = counterpartyDto.FullName ?? counterpartyDto.Name;
					counterparty.TypeOfOwnership = counterpartyDto.TypeOfOwnership;
					counterparty.TaxType = counterpartyDto.TaxType.Value;
					counterparty.INN = counterpartyDto.Inn;
					counterparty.KPP = counterpartyDto.Kpp;
					counterparty.JurAddress = counterpartyDto.JurAddress;
					break;
				case PersonType.natural:
					counterparty.FirstName = counterpartyDto.FirstName;
					counterparty.Surname = counterpartyDto.Surname;
					counterparty.Patronymic = counterpartyDto.Patronymic;
					counterparty.FullName = $"{counterparty.Surname} {counterparty.FirstName} {counterparty.Patronymic}";
					break;
			}

			counterparty.CameFrom = _uow.GetById<ClientCameFrom>(counterpartyDto.CameFromId);

			//Создаем новый контакт для клиента
			var phone = new Phone
			{
				Counterparty = counterparty,
				Number = counterpartyDto.PhoneNumber,
			};

			FillCounterpartyContact(phone, counterpartyDto.FirstName, counterpartyDto.Patronymic);

			//Создаем новую почту
			var email = CreateNewEmail(counterpartyDto.Email, counterparty);

			//Делаем привязку нового клиента и покупателя
			var externalCounterparty = _counterpartyModelFactory.CreateExternalCounterparty(counterpartyDto.CounterpartyFrom);
			externalCounterparty.Email = email;
			externalCounterparty.ExternalCounterpartyId = counterpartyDto.ExternalCounterpartyId;
			externalCounterparty.Phone = phone;
			
			_uow.Save(counterparty);
			_uow.Save(externalCounterparty);
			_uow.Commit();

			return _counterpartyModelFactory.CreateRegisteredCounterpartyRegistrationDto(counterparty.Id);
		}

		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			var validationResult = _counterpartyModelValidator.CounterpartyDtoValidate(counterpartyDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				return _counterpartyModelFactory.CreateErrorCounterpartyUpdateDto(validationResult);
			}
			
			var externalCounterparty = _phoneRepository
				.GetExternalCounterparty(_uow, counterpartyDto.ExternalCounterpartyId, counterpartyDto.CounterpartyFrom);

			if(externalCounterparty is null)
			{
				return _counterpartyModelFactory.CreateNotFoundCounterpartyUpdateDto();
			}

			//Обновляем сущность и валидируем ее, затем сохраняем изменения
			var counterparty = externalCounterparty.Phone.Counterparty;
			if(counterparty.CameFrom.Id != counterpartyDto.CameFromId)
			{
				counterparty.CameFrom = _uow.GetById<ClientCameFrom>(counterpartyDto.CameFromId);
			}

			switch(counterpartyDto.PersonType)
			{
				case PersonType.legal:
					counterparty.Name = counterpartyDto.Name;
					counterparty.FullName = counterpartyDto.FullName ?? counterpartyDto.Name;
					counterparty.TypeOfOwnership = counterpartyDto.TypeOfOwnership;
					counterparty.TaxType = counterpartyDto.TaxType.Value;
					counterparty.INN = counterpartyDto.Inn;
					counterparty.KPP = counterpartyDto.Kpp;
					counterparty.JurAddress = counterpartyDto.JurAddress;
					break;
				case PersonType.natural:
					counterparty.FirstName = counterpartyDto.FirstName;
					counterparty.Surname = counterpartyDto.Surname;
					counterparty.Patronymic = counterpartyDto.Patronymic;
					counterparty.Name = $"{counterparty.Surname} {counterparty.FirstName} {counterparty.Patronymic}";
					break;
			}

			if(externalCounterparty.Email.Address != counterpartyDto.Email)
			{
				externalCounterparty.Email = CreateNewEmail(counterpartyDto.Email, counterparty);
			}
			
			//валидация?

			_uow.Save(counterparty);
			_uow.Save(externalCounterparty);
			_uow.Commit();

			return new CounterpartyUpdateDto
			{
				CounterpartyUpdateStatus = CounterpartyUpdateStatus.CounterpartyUpdated
			};
		}
		
		private Email GetCounterpartyEmailForExternalCounterparty(int counterpartyId)
		{
			return _emailRepository.GetEmailForExternalCounterparty(_uow, counterpartyId);
		}

		private bool ValidateCounterpartyDto(CounterpartyDto counterpartyDto)
		{
			throw new NotImplementedException();
		}

		private Email CreateNewEmail(string emailAddress, Counterparty counterparty)
		{
			var emailType = _emailRepository.GetEmailTypeForReceipts(_uow);
			return new Email
			{
				Address = emailAddress,
				Counterparty = counterparty,
				EmailType = emailType
			};
		}

		private CounterpartyRegistrationDto CheckExternalCounterpartyWithSameExternalId(CounterpartyDto counterpartyDto)
		{
			var externalCounterparty =
				_phoneRepository.GetExternalCounterparty(_uow, counterpartyDto.ExternalCounterpartyId, counterpartyDto.CounterpartyFrom);

			if(externalCounterparty != null)
			{
				return new CounterpartyRegistrationDto
				{
					ErpCounterpartyId = externalCounterparty.Phone.Counterparty.Id,
					//ErrorDescription = "Контрагент с таким внешним номером уже зарегистрирован",
					CounterpartyRegistrationStatus = CounterpartyRegistrationStatus.CounterpartyWithSameExternalIdExists
				};
			}

			return null;
		}
		
		private CounterpartyRegistrationDto CheckExternalCounterpartyWithSamePhoneNumber(CounterpartyDto counterpartyDto)
		{
			var phoneNumber = new PhoneFormatter(PhoneFormat.RussiaOnlyShort).FormatString(counterpartyDto.PhoneNumber);
			var externalCounterparty = 
				_phoneRepository.GetExternalCounterparty(_uow, phoneNumber, counterpartyDto.CounterpartyFrom);

			if(externalCounterparty != null)
			{
				return new CounterpartyRegistrationDto
				{
					ErpCounterpartyId = externalCounterparty.Phone.Counterparty.Id,
					//ErrorDescription = "Контрагент с таким номером телефона уже зарегистрирован",
					CounterpartyRegistrationStatus = CounterpartyRegistrationStatus.CounterpartyWithSamePhoneNumberExists
				};
			}

			return null;
		}
		
		private void FillCounterpartyContact(Phone phone, string counterpartyName, string counterpartyPatronymic)
		{
			var roboatsName = _roboatsRepository.GetCounterpartyName(_uow, counterpartyName);

			if(roboatsName is null)
			{
				FillCounterpartyContactByDefault(phone);
				return;
			}

			var roboatsPatronymic = _roboatsRepository.GetCounterpartyPatronymic(_uow, counterpartyPatronymic);

			if(roboatsPatronymic is null)
			{
				FillCounterpartyContactByDefault(phone);
				return;
			}
			
			phone.RoboAtsCounterpartyName = roboatsName;
			phone.RoboAtsCounterpartyPatronymic = roboatsPatronymic;
		}

		private void FillCounterpartyContactByDefault(Phone phone)
		{
			phone.RoboAtsCounterpartyName =
				_uow.GetById<RoboAtsCounterpartyName>(_roboatsSettings.DefaultCounterpartyNameId);
			phone.RoboAtsCounterpartyPatronymic =
				_uow.GetById<RoboAtsCounterpartyPatronymic>(_roboatsSettings.DefaultCounterpartyPatronymicId);
		}
	}
}
