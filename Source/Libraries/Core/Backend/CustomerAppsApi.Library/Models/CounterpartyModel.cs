using System;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Settings.Roboats;
using VodovozBusiness.Controllers;

namespace CustomerAppsApi.Library.Models
{
	public class CounterpartyModel : ICounterpartyModel
	{
		private readonly ILogger<CounterpartyModel> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IExternalCounterpartyMatchingRepository _externalCounterpartyMatchingRepository;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly ICachedBottlesDebtRepository _cachedBottlesDebtRepository;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly ICameFromConverter _cameFromConverter;
		private readonly ICounterpartyModelFactory _counterpartyModelFactory;
		private readonly ICounterpartyRequestDataValidator _counterpartyRequestDataValidator;
		private readonly IContactManagerForExternalCounterparty _contactManagerForExternalCounterparty;
		private readonly ICounterpartyFactory _counterpartyFactory;
		private readonly ICounterpartyEdoAccountController _counterpartyEdoAccountController;
		private readonly IConfigurationSection _cacheExpirationSection;

		public CounterpartyModel(
			ILogger<CounterpartyModel> logger,
			IUnitOfWork uow,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IExternalCounterpartyMatchingRepository externalCounterpartyMatchingRepository,
			IRoboatsRepository roboatsRepository,
			IEmailRepository emailRepository,
			ICachedBottlesDebtRepository cachedBottlesDebtRepository,
			IRoboatsSettings roboatsSettings,
			ICameFromConverter cameFromConverter,
			ICounterpartyModelFactory counterpartyModelFactory,
			ICounterpartyRequestDataValidator counterpartyRequestDataValidator,
			IContactManagerForExternalCounterparty contactManagerForExternalCounterparty,
			ICounterpartyFactory counterpartyFactory,
			ICounterpartyEdoAccountController counterpartyEdoAccountController,
			IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_externalCounterpartyMatchingRepository =
				externalCounterpartyMatchingRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyMatchingRepository));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_cachedBottlesDebtRepository = cachedBottlesDebtRepository ?? throw new ArgumentNullException(nameof(cachedBottlesDebtRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_cameFromConverter = cameFromConverter ?? throw new ArgumentNullException(nameof(cameFromConverter));
			_counterpartyModelFactory = counterpartyModelFactory ?? throw new ArgumentNullException(nameof(counterpartyModelFactory));
			_counterpartyRequestDataValidator = counterpartyRequestDataValidator ?? throw new ArgumentNullException(nameof(counterpartyRequestDataValidator));
			_contactManagerForExternalCounterparty =
				contactManagerForExternalCounterparty ?? throw new ArgumentNullException(nameof(contactManagerForExternalCounterparty));
			_counterpartyFactory = counterpartyFactory ?? throw new ArgumentNullException(nameof(counterpartyFactory));
			_counterpartyEdoAccountController =
				counterpartyEdoAccountController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountController));
			_cacheExpirationSection =
				(configuration ?? throw new ArgumentNullException(nameof(configuration)))
				.GetSection("CacheExpirationTime");
		}

		public CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			_logger.LogInformation(
				"Запрос на авторизацию с номером {PhoneNumber} и Guid {ExternalCounterpartyId}",
				counterpartyContactInfoDto.PhoneNumber,
				counterpartyContactInfoDto.ExternalCounterpartyId);
			
			var validationResult = _counterpartyRequestDataValidator.CounterpartyContactInfoDtoValidate(counterpartyContactInfoDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation("Не прошли валидацию при авторизации {ValidationResult}", validationResult);
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
				_logger.LogInformation("Нашли соответствие по телефону {PhoneNumber} и {ExternalId}",
					phoneNumber,
					counterpartyContactInfoDto.ExternalCounterpartyId);
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

			_logger.LogInformation("Ищем зарегистрированного пользователя, но на другой площадке");
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
					_logger.LogInformation(
						"Отправляем на ручное сопоставление {PhoneNumber}, найденный контакт {Contact}",
						phoneNumber,
						contact.Phone?.Id);
					return SendToManualHandling(counterpartyContactInfoDto, counterpartyFrom);
			}
		}

		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto, bool isDryRun = false)
		{
			_logger.LogInformation("Запрос на регистрацию {ExternalId}", counterpartyDto.ExternalCounterpartyId);
			var validationResult = _counterpartyRequestDataValidator.CounterpartyDtoValidate(counterpartyDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation("Не прошли валидацию при регистрации {ValidationResult}", validationResult);
				return _counterpartyModelFactory.CreateErrorCounterpartyRegistrationDto(validationResult);
			}

			var counterpartyFrom = _cameFromConverter.ConvertCameFromToCounterpartyFrom(counterpartyDto.CameFromId);
			
			var counterpartyRegistrationDto = CheckExternalCounterpartyWithSameExternalId(counterpartyDto, counterpartyFrom);

			if(counterpartyRegistrationDto != null)
			{
				_logger.LogInformation("Нашли другого контрагента {CounterpartyId} при регистрации с таким же {ExternalId}",
					counterpartyRegistrationDto.ErpCounterpartyId,
					counterpartyDto.ExternalCounterpartyId);
				return counterpartyRegistrationDto;
			}

			counterpartyRegistrationDto = CheckExternalCounterpartyWithSamePhoneNumber(counterpartyDto, counterpartyFrom);
			
			if(counterpartyRegistrationDto != null)
			{
				_logger.LogInformation("Нашли другого контрагента {CounterpartyId} при регистрации с таким же {PhoneNumber}",
					counterpartyRegistrationDto.ErpCounterpartyId,
					counterpartyDto.PhoneNumber);
				return counterpartyRegistrationDto;
			}
			
			//Создаем нового контрагента и валидируем полученную сущность
			var counterparty = _counterpartyFactory.CreateCounterpartyFromExternalSource(counterpartyDto);
			_counterpartyEdoAccountController.AddDefaultEdoAccountsToCounterparty(counterparty);
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

			if(!isDryRun)
			{
				_uow.Save(counterparty);
				_uow.Save(externalCounterparty);
				_uow.Commit();
			}

			_logger.LogInformation(
				"Создали нового пользователя {ExternalId} {PhoneNumber}, код пользователя {ErpCounterpartyId}",
				counterpartyDto.ExternalCounterpartyId,
				counterpartyDto.PhoneNumber,
				counterparty.Id);
			
			return _counterpartyModelFactory.CreateRegisteredCounterpartyRegistrationDto(counterparty.Id);
		}

		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto, bool isDryRun = false)
		{
			_logger.LogInformation("Запрос на обновление данных {ExternalId}", counterpartyDto.ExternalCounterpartyId);
			var validationResult = _counterpartyRequestDataValidator.CounterpartyDtoValidate(counterpartyDto);
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
				_logger.LogInformation("При запросе обновления данных {ExternalId} контрагент не найден",
					counterpartyDto.ExternalCounterpartyId);
				return _counterpartyModelFactory.CreateNotFoundCounterpartyUpdateDto();
			}

			var counterparty = externalCounterparty.Phone.Counterparty;
			counterparty.CameFrom ??= _uow.GetById<ClientCameFrom>(counterpartyDto.CameFromId);

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

			if(externalCounterparty.Email?.Address != counterpartyDto.Email)
			{
				externalCounterparty.Email = CreateNewEmail(counterpartyDto.Email, counterparty);
			}

			if(!isDryRun)
			{
				_uow.Save(counterparty);
				_uow.Save(externalCounterparty);
				_uow.Commit();
			}

			_logger.LogInformation("Успешно обновили данные {ExternalId}", counterpartyDto.ExternalCounterpartyId);
			return new CounterpartyUpdateDto
			{
				CounterpartyUpdateStatus = CounterpartyUpdateStatus.CounterpartyUpdated
			};
		}

		public CounterpartyBottlesDebtDto GetCounterpartyBottlesDebt(int counterpartyId)
		{
			_logger.LogInformation("Поступил запрос на выборку долга по бутылям клиента {CounterpartyId}", counterpartyId);
			var debt = _cachedBottlesDebtRepository.GetCounterpartyBottlesDebt(
				_uow, counterpartyId, _cacheExpirationSection.GetValue<int>("CounterpartyDebtCacheMinutes"));
			return _counterpartyFactory.CounterpartyBottlesDebtDto(counterpartyId, debt);
		}
		
		private Email GetCounterpartyEmailForExternalCounterparty(int counterpartyId)
		{
			return _emailRepository.GetEmailForExternalCounterparty(_uow, counterpartyId);
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
			if(_externalCounterpartyMatchingRepository.ExternalCounterpartyMatchingExists(
					_uow, counterpartyContactInfoDto.ExternalCounterpartyId, counterpartyContactInfoDto.PhoneNumber))
			{
				return _counterpartyModelFactory.CreateNeedManualHandlingCounterpartyIdentificationDto();
			}
			
			var counterpartyManualHandlingDto = _counterpartyModelFactory.CreateNeedManualHandlingCounterpartyDto(
				counterpartyContactInfoDto, counterpartyFrom);
			
			_uow.Save(counterpartyManualHandlingDto.ExternalCounterpartyMatching);
			_uow.Commit();

			return counterpartyManualHandlingDto.CounterpartyIdentificationDto;
		}
	}
}
