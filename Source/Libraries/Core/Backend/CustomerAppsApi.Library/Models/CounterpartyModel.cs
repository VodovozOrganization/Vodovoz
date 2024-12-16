using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Validators;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Roboats;

namespace CustomerAppsApi.Library.Models
{
	public class CounterpartyModel : ICounterpartyModel
	{
		private readonly ILogger<CounterpartyModel> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly ICameFromConverter _cameFromConverter;
		private readonly ICounterpartyModelFactory _counterpartyModelFactory;
		private readonly ICounterpartyModelValidator _counterpartyModelValidator;
		private readonly IContactManagerForExternalCounterparty _contactManagerForExternalCounterparty;
		private readonly ICounterpartyFactory _counterpartyFactory;
		private readonly ICounterpartyServiceDataHandler _counterpartyServiceDataHandler;
		private readonly IEmailRepository _emailRepository;

		public CounterpartyModel(
			ILogger<CounterpartyModel> logger,
			IUnitOfWork uow,
			IRoboatsSettings roboatsSettings,
			ICameFromConverter cameFromConverter,
			ICounterpartyModelFactory counterpartyModelFactory,
			ICounterpartyModelValidator counterpartyModelValidator,
			IContactManagerForExternalCounterparty contactManagerForExternalCounterparty,
			ICounterpartyFactory counterpartyFactory,
			ICounterpartyServiceDataHandler counterpartyServiceDataHandler,
			IEmailRepository emailRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_cameFromConverter = cameFromConverter ?? throw new ArgumentNullException(nameof(cameFromConverter));
			_counterpartyModelFactory = counterpartyModelFactory ?? throw new ArgumentNullException(nameof(counterpartyModelFactory));
			_counterpartyModelValidator = counterpartyModelValidator ?? throw new ArgumentNullException(nameof(counterpartyModelValidator));
			_contactManagerForExternalCounterparty =
				contactManagerForExternalCounterparty ?? throw new ArgumentNullException(nameof(contactManagerForExternalCounterparty));
			_counterpartyFactory = counterpartyFactory ?? throw new ArgumentNullException(nameof(counterpartyFactory));
			_counterpartyServiceDataHandler =
				counterpartyServiceDataHandler ?? throw new ArgumentNullException(nameof(counterpartyServiceDataHandler));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
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
				_counterpartyServiceDataHandler.GetExternalCounterparty(
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
				_counterpartyServiceDataHandler.GetExternalCounterparty(
					_uow, counterpartyContactInfoDto.ExternalCounterpartyId, counterpartyFrom);

			if(externalCounterparty != null)
			{
				_logger.LogInformation(
					"Нашли соответствие по ExternalId {ExternalCounterpartyId} отправляем на ручное сопоставление",
					counterpartyContactInfoDto.ExternalCounterpartyId);
				return SendToManualHandling(counterpartyContactInfoDto, counterpartyFrom);
			}

			_logger.LogInformation("Теперь ищем зарегистрированного клиента по телефону {PhoneNumber}", phoneNumber);
			
			externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(_uow, phoneNumber, counterpartyFrom);

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
			
			externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(_uow, phoneNumber, registeredFromOtherPlatform);

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
			_logger.LogInformation("Запрос на регистрацию {ExternalId}", counterpartyDto.ExternalCounterpartyId);
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
			counterparty.CameFrom = _uow.GetById<ClientCameFrom>(counterpartyDto.CameFromId);

			//Создаем новый контакт для клиента
			var phone = new Phone
			{
				Counterparty = counterparty,
				Number = counterpartyDto.PhoneNumber,
			};

			FillCounterpartyContact(phone, counterpartyDto.FirstName, counterpartyDto.Patronymic);

			_uow.Save(counterparty);
			
			//Создаем новую почту
			var email = CreateNewEmail(counterpartyDto.Email, counterparty);
			
			//Делаем привязку нового клиента и покупателя
			var externalCounterparty = _counterpartyModelFactory.CreateExternalCounterparty(counterpartyFrom);
			externalCounterparty.Email = email;
			externalCounterparty.ExternalCounterpartyId = counterpartyDto.ExternalCounterpartyId;
			externalCounterparty.Phone = phone;
			
			_uow.Save(phone);
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
			_logger.LogInformation("Запрос на обновление данных {ExternalId}", counterpartyDto.ExternalCounterpartyId);
			var validationResult = _counterpartyModelValidator.CounterpartyDtoValidate(counterpartyDto);
			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_logger.LogInformation("Не прошли валидацию при обновлении {ValidationResult}", validationResult);
				return _counterpartyModelFactory.CreateErrorCounterpartyUpdateDto(validationResult);
			}
			
			var externalCounterparty = _counterpartyServiceDataHandler
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
					counterparty.FillLegalProperties(counterpartyDto);
					break;
				case PersonType.natural:
					counterparty.FirstName = counterpartyDto.FirstName;
					counterparty.Surname = counterpartyDto.Surname;
					counterparty.Patronymic = counterpartyDto.Patronymic;
					counterparty.Name = $"{counterparty.Surname} {counterparty.FirstName} {counterparty.Patronymic}";
					break;
			}

			_uow.Save(counterparty);

			if(externalCounterparty.Email?.Address != counterpartyDto.Email)
			{
				var email = CreateNewEmail(counterpartyDto.Email, counterparty);
				externalCounterparty.Email = email;
			}

			_uow.Save(externalCounterparty);
			_uow.Commit();
			
			_logger.LogInformation("Успешно обновили данные {ExternalId}", counterpartyDto.ExternalCounterpartyId);
			return new CounterpartyUpdateDto
			{
				CounterpartyUpdateStatus = CounterpartyUpdateStatus.CounterpartyUpdated
			};
		}

		public async Task<CounterpartyBottlesDebtDto> GetCounterpartyBottlesDebt(int counterpartyId)
		{
			_logger.LogInformation("Поступил запрос на выборку долга по бутылям клиента {CounterpartyId}", counterpartyId);
			var debt = await _counterpartyServiceDataHandler.GetCounterpartyBottlesDebt(_uow, counterpartyId);
			return _counterpartyFactory.CounterpartyBottlesDebtDto(counterpartyId, debt);
		}
		
		public string GetLegalCustomersDtoByInnValidate(GetLegalCustomersByInnDto dto)
		{
			return _counterpartyModelValidator.GetLegalCustomersByInnDtoValidate(dto);
		}

		public (string Message, IEnumerable<LegalCounterpartyInfo> Data) GetLegalCustomersByInn(GetLegalCustomersByInnDto dto)
		{
			var externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(
				_uow, dto.ExternalCounterpartyId, _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source));

			if(externalCounterparty is null)
			{
				return ("Неизвестный пользователь", null);
			}
			
			var naturalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!naturalCounterpartyExists)
			{
				return ("Не найден клиент с таким Id", null);
			}

			var registeredCounterpartyId = externalCounterparty.Phone.Counterparty.Id;
			
			if(registeredCounterpartyId != dto.ErpCounterpartyId)
			{
				return (
					$"Переданный Id клиента {dto.ErpCounterpartyId} не совпадает с зарегистрированным {registeredCounterpartyId}",
					null);
			}
			
			if(externalCounterparty.Phone.DigitsNumber != dto.PhoneNumber)
			{
				return ("Не совпадает номер телефона у пользователя и который пришел в запросе", null);
			}
			
			var counterparties = _counterpartyServiceDataHandler.GetLegalCustomersByInn(_uow, dto);
			
			return (null, counterparties);
		}
		
		public (string Message, IEnumerable<LegalCounterpartyInfo> Data) GetNaturalCounterpartyLegalCustomers(
			GetNaturalCounterpartyLegalCustomersDto dto)
		{
			var externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(
				_uow, dto.ExternalCounterpartyId, _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source));

			if(externalCounterparty is null)
			{
				return ("Неизвестный пользователь", null);
			}
			
			var naturalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!naturalCounterpartyExists)
			{
				return ("Не найден клиент с таким Id", null);
			}
			
			if(externalCounterparty.Phone.DigitsNumber != dto.PhoneNumber)
			{
				return ("Не совпадает номер телефона у пользователя и который пришел в запросе", null);
			}
			
			var counterparties =
				_counterpartyServiceDataHandler.GetNaturalCounterpartyLegalCustomers(_uow, dto.ErpCounterpartyId, dto.PhoneNumber);
			
			return (null, counterparties);
		}
		
		public (string Message, RegisteredLegalCustomerDto Data) RegisterLegalCustomer(RegisteringLegalCustomerDto dto)
		{
			var naturalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!naturalCounterpartyExists)
			{
				return ("Не найден клиент с таким Id", null);
			}

			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.Inn);

			if(legalCounterpartyExists)
			{
				return ("Юр лицо с таким ИНН уже существует", null);
			}

			_logger.LogInformation(
				"Проверяем наличие в БД ОПФ с кодом {Code} и аббревиатурой {ShortTypeOwnership}",
				dto.CodeTypeOfOwnership,
				dto.ShortTypeOfOwnership);
			
			var typeOwnership = _counterpartyServiceDataHandler.GetOrganizationOwnershipTypeByCode(_uow, dto.CodeTypeOfOwnership);

			if(typeOwnership is null)
			{
				_logger.LogInformation(
					"Не нашли в БД ОПФ с кодом {Code} и аббревиатурой {ShortTypeOwnership}, создаем...",
					dto.CodeTypeOfOwnership,
					dto.ShortTypeOfOwnership);
				
				typeOwnership = new OrganizationOwnershipType
				{
					Code = dto.CodeTypeOfOwnership,
					Abbreviation = dto.ShortTypeOfOwnership,
					FullName = dto.FullTypeOfOwnership
				};
			}
			else
			{
				if(typeOwnership.IsArchive)
				{
					_logger.LogInformation(
						"Нашли в БД ОПФ с кодом {Code} и аббревиатурой {ShortTypeOwnership}, на архивную, разархивируем...",
						dto.CodeTypeOfOwnership,
						dto.ShortTypeOfOwnership);
					
					typeOwnership.IsArchive = false;
				}
			}
			
			_uow.Save(typeOwnership);

			var newLegalCounterparty = new Counterparty
			{
				PersonType = PersonType.legal
			};
			newLegalCounterparty.FillLegalProperties(dto);
			newLegalCounterparty.CameFrom = _uow.GetById<ClientCameFrom>((int)dto.Source);

			var email = CreateNewEmail(dto.Email, newLegalCounterparty);
			newLegalCounterparty.Emails.Add(email);
			
			_uow.Save(newLegalCounterparty);
			_uow.Commit();

			var registered = RegisteredLegalCustomerDto.Create(
				newLegalCounterparty.Id,
				newLegalCounterparty.Name,
				newLegalCounterparty.INN,
				newLegalCounterparty.KPP,
				newLegalCounterparty.JurAddress,
				newLegalCounterparty.TypeOfOwnership);
			
			return (null, registered);
		}

		public string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto)
		{
			return _counterpartyModelValidator.RegisteringLegalCustomerValidate(dto);
		}

		public string ConnectingLegalCustomerValidate(ConnectingLegalCustomerDto dto)
		{
			return _counterpartyModelValidator.ConnectingLegalCustomerValidate(dto);
		}

		public string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto)
		{
			return _counterpartyModelValidator.GetPhonesConnectedToLegalCustomerValidate(dto);
		}

		public (string Message, PhonesConnectedToLegalCustomerDto Data) GetPhonesConnectedToLegalCustomer(
			GetPhonesConnectedToLegalCustomerDto dto)
		{
			var externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(
				_uow, dto.ExternalCounterpartyId, _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source));

			var legalCounterpartyId = dto.ErpLegalCounterpartyId;
			var naturalCounterpartyId = dto.ErpNaturalCounterpartyId;

			if(externalCounterparty is null)
			{
				return ("Не найден зарегистрированный пользователь", null);
			}
			
			var naturalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, naturalCounterpartyId);

			if(!naturalCounterpartyExists)
			{
				return ("Не найдено физическое лицо с таким Id", null);
			}
			
			var registeredCounterpartyId = externalCounterparty.Phone.Counterparty.Id;
			
			if(registeredCounterpartyId != dto.ErpNaturalCounterpartyId)
			{
				return (
					$"Переданный Id клиента {dto.ErpNaturalCounterpartyId} не совпадает с зарегистрированным {registeredCounterpartyId}",
					null);
			}

			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, legalCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return ("Не найдено юридическое лицо с таким Id", null);
			}
			
			var connectedCustomerPhones =
				_counterpartyServiceDataHandler.GetConnectedCustomerPhones(_uow, legalCounterpartyId, naturalCounterpartyId);

			return (null, PhonesConnectedToLegalCustomerDto.Create(connectedCustomerPhones));
		}

		public string UpdateConnectToLegalCustomerByPhone(UpdateConnectToLegalCustomerByPhoneDto dto)
		{
			var externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(
				_uow, dto.ExternalCounterpartyId, _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source));

			var legalCounterpartyId = dto.ErpLegalCounterpartyId;
			var naturalCounterpartyId = dto.ErpNaturalCounterpartyId;

			if(externalCounterparty is null)
			{
				return "Не найден зарегистрированный пользователь";
			}
			
			var naturalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, naturalCounterpartyId);

			if(!naturalCounterpartyExists)
			{
				return "Не найдено физическое лицо с таким Id";
			}

			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, legalCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return "Не найдено юридическое лицо с таким Id";
			}

			var connectedCustomer = _counterpartyServiceDataHandler.GetConnectedCustomer(
				_uow, dto.ErpLegalCounterpartyId, dto.ErpPhoneId);

			if(connectedCustomer is null)
			{
				return "Не найдена связка номера и юр лица";
			}
			
			var currentConnectState = Enum.Parse<ConnectedCustomerConnectState>(dto.ConnectState);

			switch(currentConnectState)
			{
				case ConnectedCustomerConnectState.Active:
					connectedCustomer.ConnectState = ConnectedCustomerConnectState.Blocked;
					break;
				case ConnectedCustomerConnectState.Blocked:
					connectedCustomer.ConnectState = ConnectedCustomerConnectState.Active;
					break;
			}
			
			_uow.Save(connectedCustomer);
			_uow.Commit();

			return null;
		}

		public string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto)
		{
			return _counterpartyModelValidator.GetNaturalCounterpartyLegalCustomersDtoValidate(dto);
		}

		public string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto)
		{
			return _counterpartyModelValidator.UpdateConnectToLegalCustomerByPhoneValidate(dto);
		}

		public string ConnectingNewPhoneToLegalCustomerValidate(ConnectingNewPhoneToLegalCustomerDto dto)
		{
			return _counterpartyModelValidator.ConnectingNewPhoneToLegalCustomerValidate(dto);
		}

		public string ConnectNewPhoneToLegalCustomer(ConnectingNewPhoneToLegalCustomerDto dto)
		{
			var externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(
				_uow, dto.ExternalCounterpartyId, _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source));

			var legalCounterpartyId = dto.ErpLegalCounterpartyId;
			var naturalCounterpartyId = dto.ErpNaturalCounterpartyId;

			if(externalCounterparty is null)
			{
				return "Не найден зарегистрированный пользователь";
			}
			
			var naturalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, naturalCounterpartyId);

			if(!naturalCounterpartyExists)
			{
				return "Не найдено физическое лицо с таким Id";
			}

			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, legalCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return "Не найдено юридическое лицо с таким Id";
			}
			
			if(externalCounterparty.Phone.DigitsNumber != dto.PhoneNumber)
			{
				return "Не совпадает номер телефона у пользователя и который пришел в запросе";
			}

			var connectedCustomer = _counterpartyServiceDataHandler.GetConnectedCustomer(
				_uow, dto.ErpLegalCounterpartyId, dto.ErpNaturalCounterpartyId, dto.PhoneNumber);

			if(connectedCustomer != null)
			{
				return "Эта связка уже существует";
			}
			
			var newConnectedCustomer = ConnectedCustomer.Create(dto.ErpLegalCounterpartyId, externalCounterparty.Phone.Id);
			_uow.Save(newConnectedCustomer);
			_uow.Commit();

			return null;
		}

		private Email GetCounterpartyEmailForExternalCounterparty(int counterpartyId)
		{
			return _counterpartyServiceDataHandler.GetEmailForExternalCounterparty(_uow, counterpartyId);
		}

		private Email CreateNewEmail(string emailAddress, Counterparty counterparty)
		{
			var emailType = _emailRepository.GetEmailTypeForReceipts(_uow);
			var email = new Email
			{
				Address = emailAddress,
				Counterparty = counterparty,
				EmailType = emailType
			};
			_uow.Save(email);
			
			return email;
		}

		private CounterpartyRegistrationDto CheckExternalCounterpartyWithSameExternalId(
			CounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom)
		{
			var externalCounterparty =
				_counterpartyServiceDataHandler.GetExternalCounterparty(_uow, counterpartyDto.ExternalCounterpartyId, counterpartyFrom);

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
				_counterpartyServiceDataHandler.GetExternalCounterparty(_uow, phoneNumber, counterpartyFrom);

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
			var roboatsName = _counterpartyServiceDataHandler.GetRoboatsCounterpartyName(_uow, counterpartyName);

			if(roboatsName is null)
			{
				FillCounterpartyContactByDefault(phone);
				return;
			}

			var roboatsPatronymic = _counterpartyServiceDataHandler.GetRoboatsCounterpartyPatronymic(_uow, counterpartyPatronymic);

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
			if(_counterpartyServiceDataHandler.ExternalCounterpartyMatchingExists(
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
