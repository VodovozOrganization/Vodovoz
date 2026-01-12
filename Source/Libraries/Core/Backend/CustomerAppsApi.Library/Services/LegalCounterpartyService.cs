using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto.Contacts;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Errors;
using CustomerAppsApi.Library.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Application.TrueMark;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.Security;
using Vodovoz.Services;
using VodovozBusiness.EntityRepositories.Counterparties;
using VodovozBusiness.Errors.TrueMark;

namespace CustomerAppsApi.Library.Services
{
	public class LegalCounterpartyService
	{
		private readonly ILogger<LegalCounterpartyService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICameFromConverter _cameFromConverter;
		private readonly ICounterpartyServiceDataHandler _counterpartyServiceDataHandler;
		private readonly IEmailRepository _emailRepository;
		private readonly IExternalLegalCounterpartyAccountRepository _externalLegalCounterpartyEmailsRepository;
		private readonly ICustomerAppCounterpartyRepository _customerAppCounterpartyRepository;
		private readonly IContactsRepository _contactsRepository;
		private readonly ICounterpartyService _counterpartyService;
		private readonly TrueMarkRegistrationCheckService _trueMarkRegistrationCheckService;
		private readonly IPasswordHasher _passwordHasher;

		public LegalCounterpartyService(
			ILogger<LegalCounterpartyService> logger,
			IUnitOfWork uow,
			ICameFromConverter cameFromConverter,
			ICounterpartyServiceDataHandler counterpartyServiceDataHandler,
			IEmailRepository emailRepository,
			IExternalLegalCounterpartyAccountRepository externalLegalCounterpartyEmailsRepository,
			ICustomerAppCounterpartyRepository customerAppCounterpartyRepository,
			IContactsRepository contactsRepository,
			ICounterpartyService counterpartyService,
			TrueMarkRegistrationCheckService trueMarkRegistrationCheckService,
			IPasswordHasher passwordHasher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_cameFromConverter = cameFromConverter ?? throw new ArgumentNullException(nameof(cameFromConverter));
			_counterpartyServiceDataHandler =
				counterpartyServiceDataHandler ?? throw new ArgumentNullException(nameof(counterpartyServiceDataHandler));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_externalLegalCounterpartyEmailsRepository =
				externalLegalCounterpartyEmailsRepository ?? throw new ArgumentNullException(nameof(externalLegalCounterpartyEmailsRepository));
			_customerAppCounterpartyRepository =
				customerAppCounterpartyRepository ?? throw new ArgumentNullException(nameof(customerAppCounterpartyRepository));
			_contactsRepository = contactsRepository ?? throw new ArgumentNullException(nameof(contactsRepository));
			_counterpartyService = counterpartyService ?? throw new ArgumentNullException(nameof(counterpartyService));
			_trueMarkRegistrationCheckService =
				trueMarkRegistrationCheckService ?? throw new ArgumentNullException(nameof(trueMarkRegistrationCheckService));
			_passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
		}
		
		public Result<IEnumerable<LegalCustomersByInnResponse>> GetLegalCustomersByInn(LegalCustomersByInnRequest dto)
		{
			var counterpartyFrom = _cameFromConverter.ConvertSourceToCounterpartyFrom(dto.Source);

			if(counterpartyFrom.IsFailure)
			{
				_logger.LogWarning("Ошибка при получении данных откуда клиент");
				return Result.Failure<IEnumerable<LegalCustomersByInnResponse>>(counterpartyFrom.Errors.First());
			}

			var counterparties = _customerAppCounterpartyRepository.GetLegalCustomersByInn(_uow, dto.Inn, dto.Email);

			if(counterparties.Any() && counterparties.Count() > 1)
			{
				_logger.LogWarning("Найдены несколько клиентов с ИНН {INN}", dto.Inn);
				return Result.Failure<IEnumerable<LegalCustomersByInnResponse>>(CounterpartyErrors.MoreThanOneCounterpartyWithInn());
			}

			if(!counterparties.Any())
			{
				_logger.LogInformation("Не нашли клиентов с ИНН {INN}, должен прийти запрос на регистрацию", dto.Inn);
				return Result.Success<IEnumerable<LegalCustomersByInnResponse>>(new []{ LegalCustomersByInnResponse.CreateEmpty() });
			}

			counterparties.First().UpdateNextStep();

			return Result.Success(counterparties);
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

			//TODO 5606: Согласовать формат
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
			var linkedEmails =
				_externalLegalCounterpartyEmailsRepository.GetLinkedLegalCounterpartyEmails(_uow, dto.Email);

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
			var legalCounterpartyId = dto.ErpCounterpartyId;
			var linkedEmails =
				_externalLegalCounterpartyEmailsRepository.GetLinkedLegalCounterpartyEmails(_uow, legalCounterpartyId, dto.Email);

			if(!linkedEmails.Any())
			{
				_logger.LogWarning("Нет активных почт у {LegalCounterpartyId}", legalCounterpartyId);
				return Result.Failure(LegalCounterpartyControllerError.NotExistsActiveEmail());
			}

			var emailsCount = linkedEmails.Count();

			if(emailsCount > 1)
			{
				_logger.LogWarning("У {LegalCounterpartyId} найдено больше одной активной почты", legalCounterpartyId);
				return Result.Failure(
					LegalCounterpartyControllerError.ActiveEmailCountGreater1($"Найдено {emailsCount} активных почт. Обратитесь в тех поддержку"));
			}

			var linkedEmail = linkedEmails.First();

			return !_passwordHasher.VerifyHashedPassword(linkedEmail.AccountPasswordSalt, linkedEmail.AccountPasswordHash, dto.Password)
				? Result.Failure(LegalCounterpartyControllerError.WrongAccountPassword())
				: Result.Success();
		}

		public Result<string> LinkLegalCounterpartyEmailToExternalUser(LinkingLegalCounterpartyEmailToExternalUser dto)
		{
			var legalCounterpartyId = dto.ErpCounterpartyId;
			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, legalCounterpartyId);

			if(!legalCounterpartyExists)
			{
				_logger.LogWarning("Не нашли юр лица с таким Id {LegalCounterpartyId}", legalCounterpartyId);
				return Result.Failure<string>(
					new Error(nameof(LinkLegalCounterpartyEmailToExternalUser),"Не найдено юридическое лицо с таким Id"));
			}

			//Проверка пароля
			
			//TODO 5606: проверка без регистра?
			//TODO 5606: а что если уже есть такая или другая активные почты у этого клиента?
			var email = dto.Email;
			var emailsForLinking = _counterpartyServiceDataHandler.GetEmailForLinking(_uow, legalCounterpartyId, email);

			Email emailForLinking = null;

			if(!emailsForLinking.Any())
			{
				emailForLinking = CreateNewEmail(email, dto.ErpCounterpartyId);
				_uow.Save(emailForLinking);
			}
			else if(emailsForLinking.Count() > 1)
			{
				_logger.LogWarning("Найдено несколько почт с таким адресом {Email} у {LegalCounterpartyId}", email, legalCounterpartyId);
				return Result.Failure<string>(new Error(
					nameof(LinkLegalCounterpartyEmailToExternalUser),
					"Найдено несколько почт с таким адресом у этого клиента. Обратитесь в техподдержку"));
			}

			emailForLinking = emailsForLinking.First();
			var passwordData = _passwordHasher.HashPassword(dto.Password);

			var account = ExternalLegalCounterpartyAccount.Create(
				dto.Source,
				dto.ErpCounterpartyId,
				emailForLinking.Id,
				dto.ExternalCounterpartyId,
				passwordData);

			_uow.Save(account);
			_uow.Commit();

			return Result.Success(emailForLinking.Address);
		}

		public async Task<Result<CompanyInfoResponse>> GetCompanyInfo(CompanyInfoRequest dto, CancellationToken cancellationToken)
		{
			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return Result.Failure<CompanyInfoResponse>(CounterpartyErrors.CounterpartyNotExists());
			}

			var companyInfo = _customerAppCounterpartyRepository.GetLinkedCompany(_uow, dto.Source, dto.ExternalCounterpartyId, dto.ErpCounterpartyId);

			if(companyInfo is null)
			{
				return Result.Failure<CompanyInfoResponse>(LegalCounterpartyActivationErrors.ActivationNotExists());
			}

			if(companyInfo.ActivationCompanyAccountInfo.TaxServiceCheckState != nameof(TaxServiceCheckState.Done))
			{
				try
				{
					//TODO 5606: обговорить возвращаемые статусы и сделать обновление статуса в ФНС
					_counterpartyService.StopShipmentsIfNeeded();
				}
				catch(Exception e)
				{
					_logger.LogWarning(e, "Ошибка при получении статуса компании с ИНН {INN} в ФНС", companyInfo.Inn);
					companyInfo.ActivationCompanyAccountInfo.TaxServiceCheckState = nameof(TaxServiceCheckState.Error);
				}
			}
			
			if(companyInfo.ActivationCompanyAccountInfo.TrueMarkCheckState != nameof(TrueMarkCheckState.Done))
			{
				try
				{
					var trueMarkRegistrationResult = await _trueMarkRegistrationCheckService.CheckRegistrationFromTrueMarkAsync(
						companyInfo.Inn, cancellationToken);

					if(trueMarkRegistrationResult.IsFailure)
					{
						var error = trueMarkRegistrationResult.Errors.First();
						if(error.Code == nameof(TrueMarkServiceErrors.UnexpectedError))
						{
							companyInfo.ActivationCompanyAccountInfo.TrueMarkCheckState = nameof(TrueMarkCheckState.Error);
						}
						else if(error.Code == nameof(TrueMarkServiceErrors.UnknownRegistrationStatusError))
						{
							//TODO 5606: добавить еще статусов
							companyInfo.ActivationCompanyAccountInfo.TrueMarkCheckState = ;
						}
					}
					else
					{
						//TODO 5606: добавить еще статусов
						companyInfo.ActivationCompanyAccountInfo.TrueMarkCheckState = ;
					}
				}
				catch(Exception e)
				{
					_logger.LogWarning(e, "Ошибка при получении статуса компании с ИНН {INN} в ЧЗ", companyInfo.Inn);
					companyInfo.ActivationCompanyAccountInfo.TrueMarkCheckState = nameof(TrueMarkCheckState.Error);
				}
			}

			return companyInfo;
		}

		public Result<LegalCounterpartyContacts> GetLegalCustomerContacts(LegalCounterpartyContactListRequest dto)
		{
			//TODO 5606: как ищем контрагентов? Только не архивных, а если он заархивирован?
			var legalCounterpartyExists = _counterpartyServiceDataHandler.CounterpartyExists(_uow, dto.ErpCounterpartyId);

			if(!legalCounterpartyExists)
			{
				return Result.Failure<LegalCounterpartyContacts>(CounterpartyErrors.CounterpartyNotExists());
			}

			var phones = _contactsRepository.GetLegalCounterpartyPhones(_uow, dto.ErpCounterpartyId);
			var emails = _contactsRepository.GetLegalCounterpartyEmails(_uow, dto.ErpCounterpartyId);

			return LegalCounterpartyContacts.Create(phones, emails);
		}

		private Email CreateNewEmail(string emailAddress, int legalCounterpartyId)
		{
			//TODO 5606: какой тип почты по умолчанию для связки
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
			//TODO 5606: какой тип почты по умолчанию для связки
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
