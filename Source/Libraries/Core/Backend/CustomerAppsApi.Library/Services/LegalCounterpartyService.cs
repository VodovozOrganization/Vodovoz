using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Contacts;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Errors;
using CustomerAppsApi.Library.Extensions;
using CustomerAppsApi.Library.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RevenueService.Client;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.Security;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Library.Services
{
	public class LegalCounterpartyService
	{
		private readonly ILogger<LegalCounterpartyService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICounterpartyServiceDataHandler _counterpartyServiceDataHandler;
		private readonly IEmailRepository _emailRepository;
		private readonly IExternalLegalCounterpartyAccountRepository _externalLegalCounterpartyAccountRepository;
		private readonly ICustomerAppCounterpartyRepository _customerAppCounterpartyRepository;
		private readonly IContactsRepository _contactsRepository;
		private readonly IRevenueServiceClient _revenueServiceClient;
		private readonly IPasswordHasher _passwordHasher;

		public LegalCounterpartyService(
			ILogger<LegalCounterpartyService> logger,
			IUnitOfWork uow,
			ICounterpartyServiceDataHandler counterpartyServiceDataHandler,
			IEmailRepository emailRepository,
			IExternalLegalCounterpartyAccountRepository externalLegalCounterpartyAccountRepository,
			ICustomerAppCounterpartyRepository customerAppCounterpartyRepository,
			IContactsRepository contactsRepository,
			IRevenueServiceClient revenueServiceClient,
			IPasswordHasher passwordHasher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_counterpartyServiceDataHandler =
				counterpartyServiceDataHandler ?? throw new ArgumentNullException(nameof(counterpartyServiceDataHandler));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_externalLegalCounterpartyAccountRepository =
				externalLegalCounterpartyAccountRepository ?? throw new ArgumentNullException(nameof(externalLegalCounterpartyAccountRepository));
			_customerAppCounterpartyRepository =
				customerAppCounterpartyRepository ?? throw new ArgumentNullException(nameof(customerAppCounterpartyRepository));
			_contactsRepository = contactsRepository ?? throw new ArgumentNullException(nameof(contactsRepository));
			_revenueServiceClient = revenueServiceClient ?? throw new ArgumentNullException(nameof(revenueServiceClient));
			_passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
		}
		
		public Result<LegalCustomersByInnResponse> GetLegalCustomersByInn(LegalCustomersByInnRequest dto)
		{
			var counterparties = _customerAppCounterpartyRepository.GetLegalCustomersByInn(_uow, dto.Inn, dto.Email);

			if(counterparties.Any() && counterparties.Count() > 1)
			{
				_logger.LogWarning("Найдены несколько клиентов с ИНН {INN}", dto.Inn);
				return Result.Failure<LegalCustomersByInnResponse>(CounterpartyErrors.MoreThanOneCounterpartyWithInn());
			}

			if(!counterparties.Any())
			{
				_logger.LogInformation("Не нашли клиентов с ИНН {INN}, должен прийти запрос на регистрацию", dto.Inn);
				return Result.Success<LegalCustomersByInnResponse>(LegalCustomersByInnResponse.CreateEmpty());
			}

			var counterpartyInfo = counterparties.First();
			counterpartyInfo.UpdateNextStep();

			return Result.Success(counterpartyInfo);
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
			newLegalCounterparty.ReasonForLeaving = ReasonForLeaving.ForOwnNeeds;
			
			var email = CreateNewEmail(dto.Email, newLegalCounterparty);
			newLegalCounterparty.Emails.Add(email);
			
			var phone = Phone.Create(newLegalCounterparty, dto.PhoneNumber);
			newLegalCounterparty.Phones.Add(phone);
			
			_uow.Save(newLegalCounterparty);
			_uow.Commit();

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
			var externalAccounts =
				_externalLegalCounterpartyAccountRepository.GetExternalLegalCounterpartyAccounts(_uow, dto.Email);

			if(!externalAccounts.Any())
			{
				return Result.Failure<CompanyWithActiveEmailResponse>(LegalCounterpartyControllerError.NotExistsActiveEmail());
			}

			var emailsCount = externalAccounts.Count();

			if(emailsCount > 1)
			{
				return Result.Failure<CompanyWithActiveEmailResponse>(
					LegalCounterpartyControllerError.ActiveEmailCountGreater1($"Найдено {emailsCount} активных почт. Обратитесь в тех поддержку"));
			}

			return Result.Success(CompanyWithActiveEmailResponse.Create(externalAccounts.First().LegalCounterpartyId));
		}
		
		public Result CheckPassword(CheckPasswordRequest dto)
		{
			var legalCounterpartyId = dto.ErpCounterpartyId;
			var externalAccounts =
				_externalLegalCounterpartyAccountRepository.GetExternalLegalCounterpartyAccounts(_uow, legalCounterpartyId, dto.Email);

			if(!externalAccounts.Any())
			{
				_logger.LogWarning("Нет активной почты {Email} у {LegalCounterpartyId}", dto.Email, legalCounterpartyId);
				return Result.Failure(LegalCounterpartyControllerError.NotExistsActiveEmail());
			}

			var emailsCount = externalAccounts.Count();

			if(emailsCount > 1)
			{
				_logger.LogWarning("У {LegalCounterpartyId} найдено больше одной активной почты", legalCounterpartyId);
				return Result.Failure(
					LegalCounterpartyControllerError.ActiveEmailCountGreater1($"Найдено {emailsCount} активных почт. Обратитесь в тех поддержку"));
			}

			var linkedEmail = externalAccounts.First();

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

			var linkedEmails = _externalLegalCounterpartyAccountRepository.GetExternalLegalCounterpartyAccountsEmails(_uow, legalCounterpartyId);

			if(linkedEmails.Any(x => x.Address != dto.Email))
			{
				_logger.LogWarning("Найдена другая активная почта, вместо {Email} у {LegalCounterpartyId}", dto.Email, legalCounterpartyId);
				return Result.Failure<string>(
					new Error(nameof(LinkLegalCounterpartyEmailToExternalUser),"У клиента уже есть аккаунт с другой почтой"));
			}

			if(linkedEmails.Any(x => x.Address == dto.Email))
			{
				_logger.LogWarning("Почта {Email} уже активна у {LegalCounterpartyId}", dto.Email, legalCounterpartyId);
				return Result.Failure<string>(
					new Error(nameof(LinkLegalCounterpartyEmailToExternalUser),"У клиента уже есть аккаунт с данной почтой"));
			}

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
			else
			{
				emailForLinking = emailsForLinking.First();
			}

			var passwordData = _passwordHasher.HashPassword(dto.Password);

			var account = ExternalLegalCounterpartyAccount.Create(
				dto.ErpCounterpartyId,
				emailForLinking.Id,
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

			var companyInfo = _customerAppCounterpartyRepository.GetLinkedCompanyInfo(_uow, dto.ErpCounterpartyId);

			if(companyInfo is null)
			{
				return Result.Failure<CompanyInfoResponse>(LegalCounterpartyActivationErrors.ActivationNotExists());
			}

			if(companyInfo.TaxServiceCheckState is null)
			{
				var externalAccount = _externalLegalCounterpartyAccountRepository
					.GetExternalLegalCounterpartyAccounts(_uow, dto.ErpCounterpartyId)
					.FirstOrDefault();

				if(externalAccount is null)
				{
					return Result.Failure<CompanyInfoResponse>(LegalCounterpartyActivationErrors.ActivationNotExists());
				}

				TaxServiceCheckState? taxServiceCheckState = null;
				
				try
				{
					var taxServiceCheckStateFromDadata = await _revenueServiceClient
						.GetCounterpartyStatus(companyInfo.Inn, null, cancellationToken);
					taxServiceCheckState = taxServiceCheckStateFromDadata.ToTaxServiceCheckState();
					companyInfo.TaxServiceCheckState ??= taxServiceCheckState.ToString();
				}
				catch(Exception e)
				{
					_logger.LogWarning(e, "Ошибка при получении статуса компании с ИНН {INN} в ФНС", companyInfo.Inn);
					taxServiceCheckState = TaxServiceCheckState.Error;
					companyInfo.TaxServiceCheckState = nameof(TaxServiceCheckState.Error);
				}
				
				externalAccount.TaxServiceCheckState = taxServiceCheckState;
				await _uow.SaveAsync(externalAccount, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);
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

			return Result.Success(LegalCounterpartyContacts.Create(phones, emails));
		}

		private Email CreateNewEmail(string emailAddress, int legalCounterpartyId)
		{
			var emailType = _emailRepository.GetEmailTypeForExternalAccount(_uow);

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
			var emailType = _emailRepository.GetEmailTypeForExternalAccount(_uow);

			var email = Email.Create(
				emailAddress,
				legalCounterparty,
				emailType);

			_uow.Save(email);

			return email;
		}
	}
}
