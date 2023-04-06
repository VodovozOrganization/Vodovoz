using System;
using CustomerAppsApi.Converters;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Validators;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Parameters;

namespace CustomerAppsApi.Models
{
	public class CounterpartyModel : ICounterpartyModel
	{
		private readonly ILogger<CounterpartyModel> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly ICameFromConverter _cameFromConverter;
		private readonly CounterpartyModelFactory _counterpartyModelFactory;
		private readonly CounterpartyModelValidator _counterpartyModelValidator;
		private readonly IContactManagerForExternalCounterparty _contactManagerForExternalCounterparty;

		public CounterpartyModel(
			ILogger<CounterpartyModel> logger,
			IUnitOfWork uow,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IRoboatsRepository roboatsRepository,
			IEmailRepository emailRepository,
			IRoboatsSettings roboatsSettings,
			ICameFromConverter cameFromConverter,
			CounterpartyModelFactory counterpartyModelFactory,
			CounterpartyModelValidator counterpartyModelValidator,
			IContactManagerForExternalCounterparty contactManagerForExternalCounterparty)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_cameFromConverter = cameFromConverter ?? throw new ArgumentNullException(nameof(cameFromConverter));
			_counterpartyModelFactory = counterpartyModelFactory ?? throw new ArgumentNullException(nameof(counterpartyModelFactory));
			_counterpartyModelValidator = counterpartyModelValidator ?? throw new ArgumentNullException(nameof(counterpartyModelValidator));
			_contactManagerForExternalCounterparty =
				contactManagerForExternalCounterparty ?? throw new ArgumentNullException(nameof(contactManagerForExternalCounterparty));
		}

		public CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			_logger.LogInformation(
				"Запрос на авторизацию с номером {PhoneNumber} и Guid {ExternalCounterpartyId}",
				counterpartyContactInfoDto.PhoneNumber,
				counterpartyContactInfoDto.ExternalCounterpartyId);
			
			var validationResult = _counterpartyModelValidator.CounterpartyContactInfoDtoValidate(counterpartyContactInfoDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				return _counterpartyModelFactory.CreateErrorCounterpartyIdentificationDto(validationResult);
			}

			var counterpartyFrom = _cameFromConverter.ConvertCameFromToCounterpartyFrom(counterpartyContactInfoDto.CameFromId);
			var phoneNumber = new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(counterpartyContactInfoDto.PhoneNumber);

			
			_logger.LogInformation(
				"Ищем зарегистрированного клиента по ExternalId {ExternalCounterpartyId} и по телефону {PhoneNumber}",
				counterpartyContactInfoDto.ExternalCounterpartyId,
				phoneNumber);
			
			var externalCounterparty =
				_externalCounterpartyRepository.GetExternalCounterparty(
					_uow, counterpartyContactInfoDto.ExternalCounterpartyId, phoneNumber, counterpartyFrom);

			if(externalCounterparty != null)
			{
				_logger.LogInformation("Нашли соответствие по телефону и ExternalId");
				return _counterpartyModelFactory.CreateSuccessCounterpartyIdentificationDto(externalCounterparty);
			}
			
			_logger.LogInformation(
				"Теперь ищем зарегистрированного клиента по ExternalId {ExternalCounterpartyId}",
				counterpartyContactInfoDto.ExternalCounterpartyId);
			
			externalCounterparty =
				_externalCounterpartyRepository.GetExternalCounterparty(
					_uow, counterpartyContactInfoDto.ExternalCounterpartyId, counterpartyFrom);

			if(externalCounterparty != null)
			{
				_logger.LogInformation(
					"Нашли соответствие по ExternalId {ExternalCounterpartyId} отправляем на ручное сопоставление",
					counterpartyContactInfoDto.ExternalCounterpartyId);
				return SendToManualHandling(counterpartyContactInfoDto, counterpartyFrom);
			}

			_logger.LogInformation("Теперь ищем зарегистрированного клиента по телефону {PhoneNumber}", phoneNumber);
			
			externalCounterparty = _externalCounterpartyRepository.GetExternalCounterparty(_uow, phoneNumber, counterpartyFrom);

			if(externalCounterparty != null)
			{
				_logger.LogInformation("Нашли соответствие по телефону {PhoneNumber} отправляем на ручное сопоставление", phoneNumber);
				return SendToManualHandling(counterpartyContactInfoDto, counterpartyFrom);
			}

			_logger.LogInformation("Ищем зарегистрированног пользователя, но на другой площадке");
			/*
			 * Ищем зарегистрированного клиента с другой площадки
			 * если запрос пришел от мобилки, то смотрим клиента с таким номером телефона, зарегистрированного через сайт
			 * и наоборот, если запрос с сайта - ищем зарегистрированного через мобилку
			 */
			var registeredFromOtherPlatform = counterpartyFrom == CounterpartyFrom.MobileApp
				? CounterpartyFrom.WebSite
				: CounterpartyFrom.MobileApp;
			
			externalCounterparty = _externalCounterpartyRepository.GetExternalCounterparty(_uow, phoneNumber, registeredFromOtherPlatform);

			if(externalCounterparty != null)
			{
				_logger.LogInformation("Нашли, создаем нового для {CounterpartyFrom}", counterpartyFrom);
				//Создаем нужный контакт и отправляем данные в ИПЗ
				var copiedExternalCounterparty =
					_counterpartyModelFactory.CopyToOtherExternalCounterparty(externalCounterparty,
						counterpartyContactInfoDto.ExternalCounterpartyId);

				_uow.Save(copiedExternalCounterparty);
				_uow.Commit();
				
				return _counterpartyModelFactory.CreateRegisteredCounterpartyIdentificationDto(externalCounterparty);
			}

			_logger.LogInformation("Ведем поиск телефона по всему списку контактов");
			var contact = _contactManagerForExternalCounterparty.FindContactForRegisterExternalCounterparty(_uow, phoneNumber);

			switch(contact.FoundContactStatus)
			{
				case FoundContactStatus.Success:
					
					_logger.LogInformation("Нашли правильное соответствие, создаем нового пользователя");
					externalCounterparty = _counterpartyModelFactory.CreateExternalCounterparty(counterpartyFrom);
					externalCounterparty.ExternalCounterpartyId = counterpartyContactInfoDto.ExternalCounterpartyId;
					externalCounterparty.Phone = contact.Phone;
					externalCounterparty.Email = GetCounterpartyEmailForExternalCounterparty(contact.Phone.Counterparty.Id);
					
					_uow.Save(externalCounterparty);
					_uow.Commit();
					
					return _counterpartyModelFactory.CreateRegisteredCounterpartyIdentificationDto(externalCounterparty);
				case FoundContactStatus.ContactNotFound:
					_logger.LogInformation(
						"Контрагент не найден с телефоном {PhoneNumber}, должен прийти запрос на регистрацию", phoneNumber);
					return _counterpartyModelFactory.CreateNotFoundCounterpartyIdentificationDto();
				default:
					_logger.LogInformation("Отправляем на ручное сопоставление {PhoneNumber}", phoneNumber);
					return SendToManualHandling(counterpartyContactInfoDto, counterpartyFrom);
			}
		}

		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto)
		{
			_logger.LogInformation("Запрос на регистрацию");
			var validationResult = _counterpartyModelValidator.CounterpartyDtoValidate(counterpartyDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation("Не прошли валидацию при регистрации {ValidationResult}", validationResult);
				return _counterpartyModelFactory.CreateErrorCounterpartyRegistrationDto(validationResult);
			}

			var counterpartyFrom = _cameFromConverter.ConvertCameFromToCounterpartyFrom(counterpartyDto.CameFromId);
			
			var counterpartyRegistrationDto = CheckExternalCounterpartyWithSameExternalId(counterpartyDto, counterpartyFrom);

			if(counterpartyRegistrationDto != null)
			{
				return counterpartyRegistrationDto;
			}

			counterpartyRegistrationDto = CheckExternalCounterpartyWithSamePhoneNumber(counterpartyDto, counterpartyFrom);
			
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
					counterparty.Name = $"{counterparty.Surname} {counterparty.FirstName} {counterparty.Patronymic}";
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
			var externalCounterparty = _counterpartyModelFactory.CreateExternalCounterparty(counterpartyFrom);
			externalCounterparty.Email = email;
			externalCounterparty.ExternalCounterpartyId = counterpartyDto.ExternalCounterpartyId;
			externalCounterparty.Phone = phone;
			
			_uow.Save(counterparty);
			_uow.Save(externalCounterparty);
			_uow.Commit();

			_logger.LogInformation(
				"Создали нового пользователя {ExternalId} {PhoneNumber}, код пользователя {ErpCounterpartyId}",
				counterpartyDto.ExternalCounterpartyId,
				counterpartyDto.PhoneNumber,
				counterparty.Id);
			return _counterpartyModelFactory.CreateRegisteredCounterpartyRegistrationDto(counterparty.Id);
		}

		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			_logger.LogInformation("Запрос на обновление данных");
			var validationResult = _counterpartyModelValidator.CounterpartyDtoValidate(counterpartyDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation("Не прошли валидацию при обновлении {ValidationResult}", validationResult);
				return _counterpartyModelFactory.CreateErrorCounterpartyUpdateDto(validationResult);
			}
			
			var externalCounterparty = _externalCounterpartyRepository
				.GetExternalCounterparty(_uow, counterpartyDto.ExternalCounterpartyId,
					_cameFromConverter.ConvertCameFromToCounterpartyFrom(counterpartyDto.CameFromId));

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

		private CounterpartyRegistrationDto CheckExternalCounterpartyWithSameExternalId(
			CounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom)
		{
			var externalCounterparty =
				_externalCounterpartyRepository.GetExternalCounterparty(_uow, counterpartyDto.ExternalCounterpartyId, counterpartyFrom);

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
		
		private CounterpartyRegistrationDto CheckExternalCounterpartyWithSamePhoneNumber(
			CounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom)
		{
			var phoneNumber = new PhoneFormatter(PhoneFormat.RussiaOnlyShort).FormatString(counterpartyDto.PhoneNumber);
			var externalCounterparty = 
				_externalCounterpartyRepository.GetExternalCounterparty(_uow, phoneNumber, counterpartyFrom);

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

		private CounterpartyIdentificationDto SendToManualHandling(
			CounterpartyContactInfoDto counterpartyContactInfoDto, CounterpartyFrom counterpartyFrom)
		{
			var counterpartyManualHandlingDto = _counterpartyModelFactory.CreateNeedManualHandlingCounterpartyDto(
				counterpartyContactInfoDto, counterpartyFrom);
			
			_uow.Save(counterpartyManualHandlingDto.ExternalCounterpartyMatching);
			_uow.Commit();

			return counterpartyManualHandlingDto.CounterpartyIdentificationDto;
		}
	}
}
