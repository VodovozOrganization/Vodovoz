using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto.Contacts;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Errors;
using CustomerAppsApi.Library.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Library.Services
{
	public class LegalCounterpartyService
	{
		private readonly ILogger<LegalCounterpartyService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICameFromConverter _cameFromConverter;
		private readonly ICounterpartyServiceDataHandler _counterpartyServiceDataHandler;
		private readonly IEmailRepository _emailRepository;
		private readonly ILinkedLegalCounterpartyEmailToExternalUserRepository _linkedLegalCounterpartyEmailsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IContactsRepository _contactsRepository;
		
		public LegalCounterpartyService(
			ILogger<LegalCounterpartyService> logger,
			IUnitOfWork uow,
			ICameFromConverter cameFromConverter,
			ICounterpartyServiceDataHandler counterpartyServiceDataHandler,
			IEmailRepository emailRepository,
			ILinkedLegalCounterpartyEmailToExternalUserRepository linkedLegalCounterpartyEmailsRepository,
			ICounterpartyRepository counterpartyRepository,
			IContactsRepository contactsRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_cameFromConverter = cameFromConverter ?? throw new ArgumentNullException(nameof(cameFromConverter));
			_counterpartyServiceDataHandler =
				counterpartyServiceDataHandler ?? throw new ArgumentNullException(nameof(counterpartyServiceDataHandler));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_linkedLegalCounterpartyEmailsRepository =
				linkedLegalCounterpartyEmailsRepository ?? throw new ArgumentNullException(nameof(linkedLegalCounterpartyEmailsRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_contactsRepository = contactsRepository ?? throw new ArgumentNullException(nameof(contactsRepository));
		}
		
		public Result<IEnumerable<LegalCustomersByInnResponse>> GetLegalCustomersByInn(LegalCustomersByInnRequest dto)
		{
			var counterpartyFrom = _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source);

			if(counterpartyFrom.IsFailure)
			{
				return Result.Failure<IEnumerable<LegalCustomersByInnResponse>>(counterpartyFrom.Errors.First());
			}
			
			var counterparties = _counterpartyRepository.GetLegalCustomersByInn(_uow, dto.Inn, dto.Email);

			if(counterparties.Any() && counterparties.Count() > 1)
			{
				return Result.Failure<IEnumerable<LegalCustomersByInnResponse>>(CounterpartyError.MoreThanOneCounterpartyWithInn());
			}

			if(!counterparties.Any())
			{
				return Result.Success<IEnumerable<LegalCustomersByInnResponse>>(new []{ LegalCustomersByInnResponse.CreateEmpty() });
			}

			counterparties.First().UpdateNextStep();
			
			return Result.Success(counterparties);
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
						"Нашли в БД архивную ОПФ с кодом {Code} и аббревиатурой {ShortTypeOwnership}, разархивируем...",
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

			//TODO 5417: Согласовать формат
			var registered = RegisteredLegalCustomerDto.Create(
				newLegalCounterparty.Id,
				email.Address,
				newLegalCounterparty.Name,
				newLegalCounterparty.INN,
				newLegalCounterparty.KPP,
				newLegalCounterparty.JurAddress,
				newLegalCounterparty.TypeOfOwnership);
			
			return (null, registered);
		}
		
		public Result<CompanyWithActiveEmailResponse> GetCompanyWithActiveEmail(CompanyWithActiveEmailRequest dto)
		{
			var linkedEmails = _linkedLegalCounterpartyEmailsRepository.GetLinkedLegalCounterpartyEmails(_uow, dto.Email);

			if(!linkedEmails.Any())
			{
				return Result.Failure<CompanyWithActiveEmailResponse>(LegalCounterpartyControllerError.NotExistsActiveEmail());
			}

			var emailsCount = linkedEmails.Count();

			if(emailsCount > 1)
			{
				return Result.Failure<CompanyWithActiveEmailResponse>(
					LegalCounterpartyControllerError.ActiveEmailCountGreater1($"Найдено {emailsCount} активных почт. Обратитесь в тех поддержку"));
			}

			return Result.Success(CompanyWithActiveEmailResponse.Create(linkedEmails.First().LegalCounterpartyId));
		}
		
		public Result CheckPassword(CheckPasswordRequest dto)
		{
			var linkedEmails = _linkedLegalCounterpartyEmailsRepository.GetLinkedLegalCounterpartyEmails(_uow, dto.Email);

			if(!linkedEmails.Any())
			{
				return Result.Failure(LegalCounterpartyControllerError.NotExistsActiveEmail());
			}

			var emailsCount = linkedEmails.Count();
			
			if(emailsCount > 1)
			{
				return Result.Failure(
					LegalCounterpartyControllerError.ActiveEmailCountGreater1($"Найдено {emailsCount} активных почт. Обратитесь в тех поддержку"));
			}
			
			var linkedEmail = linkedEmails.First();

			return linkedEmail.AccountPassword != dto.Password
				? Result.Failure(LegalCounterpartyControllerError.WrongAccountPassword())
				: Result.Success();
		}

		public Result<string> LinkLegalCounterpartyEmailToExternalUser(LinkingLegalCounterpartyEmailToExternalUser dto)
		{
			var externalCounterparty = _counterpartyServiceDataHandler.GetExternalCounterparty(
				_uow, dto.ExternalCounterpartyId, _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source));

			var legalCounterpartyId = dto.ErpCounterpartyId;

			if(externalCounterparty is null)
			{
				return Result.Failure<string>(
					new Error(nameof(LinkLegalCounterpartyEmailToExternalUser),"Не найден зарегистрированный пользователь"));
			}

			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, legalCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return Result.Failure<string>(
					new Error(nameof(LinkLegalCounterpartyEmailToExternalUser),"Не найдено юридическое лицо с таким Id"));
			}
			
			//Проверка пароля
			
			//TODO 5417: проверка без регистра?
			var emailsForLinking = _counterpartyServiceDataHandler.GetEmailForLinking(_uow, legalCounterpartyId, dto.Email);
			
			Email emailForLinking = null;

			if(!emailsForLinking.Any())
			{
				emailForLinking = CreateNewEmail(dto.Email, dto.ErpCounterpartyId);
				_uow.Save(emailForLinking); 
			}
			else if(emailsForLinking.Count() > 1)
			{
				return Result.Failure<string>(new Error(
					nameof(LinkLegalCounterpartyEmailToExternalUser),
					"Найдено несколько почт с таким адресом у этого клиента. Обратитесь в техподдержку"));
			}
			
			emailForLinking = emailsForLinking.First();

			var link = LinkedLegalCounterpartyEmailToExternalUser.Create(
				dto.ErpCounterpartyId,
				emailForLinking.Id,
				externalCounterparty.Id,
				dto.Password);

			_uow.Save(link);
			_uow.Commit();

			return Result.Success(emailForLinking.Address);
		}

		public Result<CompanyInfoResponse> GetCompanyInfo(CompanyInfoRequest dto)
		{
			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return Result.Failure<CompanyInfoResponse>(CounterpartyError.CounterpartyNotExists());
			}
			
			//TODO 5417: решить, как будет выглядеть система аккаунтов
			var company = _counterpartyRepository.GetLinkedCompany(_uow, externalCounterparty.Id);

			if(company is null)
			{
				return Result.Failure<CompanyInfoResponse>(CounterpartyError.CounterpartyNotExists());
			}

			return company;
		}

		public Result<LegalCounterpartyContacts> GetLegalCustomerContacts(LegalCounterpartyContactListRequest dto)
		{
			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return Result.Failure<LegalCounterpartyContacts>(CounterpartyError.CounterpartyNotExists());
			}

			var phones = _contactsRepository.GetLegalCounterpartyPhones(_uow, dto.ErpCounterpartyId);
			var emails = _contactsRepository.GetLegalCounterpartyEmails(_uow, dto.ErpCounterpartyId);

			return LegalCounterpartyContacts.Create(phones, emails);
		}

		private Email CreateNewEmail(string emailAddress, int legalCounterpartyId)
		{
			//TODO 5417: какой тип почты по умолчанию для связки
			var emailType = _emailRepository.GetEmailTypeForReceipts(_uow);

			var email = Email.Create(
				emailAddress,
				new Counterparty
				{
					Id = legalCounterpartyId
				},
				emailType);

			_uow.Save(email);

			return email;
		}
		
		private Email CreateNewEmail(string emailAddress, Counterparty legalCounterparty)
		{
			//TODO 5417: какой тип почты по умолчанию для связки
			var emailType = _emailRepository.GetEmailTypeForReceipts(_uow);

			var email = Email.Create(
				emailAddress,
				legalCounterparty,
				emailType);

			_uow.Save(email);

			return email;
		}
	}
}
