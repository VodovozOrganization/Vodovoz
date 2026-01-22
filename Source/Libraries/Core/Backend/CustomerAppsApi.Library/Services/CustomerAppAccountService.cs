using System;
using System.Linq;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Dto.Counterparties.Password;
using CustomerAppsApi.Library.Errors;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Security;
using VodovozBusiness.EntityRepositories.Counterparties;
using VodovozBusiness.Services.Clients;

namespace CustomerAppsApi.Library.Services
{
	public class CustomerAppAccountService
	{
		private readonly ILogger<CustomerAppAccountService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IExternalLegalCounterpartyAccountRepository _externalLegalCounterpartyEmailsRepository;
		private readonly ILegalCounterpartyAccountHandler _accountHandler;
		private readonly IPasswordHasher _passwordHasher;

		public CustomerAppAccountService(
			ILogger<CustomerAppAccountService> logger,
			IUnitOfWork uow,
			IExternalLegalCounterpartyAccountRepository externalLegalCounterpartyEmailsRepository,
			ILegalCounterpartyAccountHandler accountHandler,
			IPasswordHasher passwordHasher
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_externalLegalCounterpartyEmailsRepository =
				externalLegalCounterpartyEmailsRepository ?? throw new ArgumentNullException(nameof(externalLegalCounterpartyEmailsRepository));
			_accountHandler = accountHandler ?? throw new ArgumentNullException(nameof(accountHandler));
			_passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
		}
		
		public Result ChangePassword(ChangePasswordRequest dto)
		{
			var legalCounterpartyId = dto.ErpCounterpartyId;
			var externalAccounts =
				_externalLegalCounterpartyEmailsRepository.GetExternalLegalCounterpartyAccounts(_uow, legalCounterpartyId, dto.Email);

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

			var account = externalAccounts.First();
			var checkOldPassword =
				_passwordHasher.VerifyHashedPassword(account.AccountPasswordSalt, account.AccountPasswordHash, dto.OldPassword);

			if(!checkOldPassword)
			{
				Result.Failure(LegalCounterpartyControllerError.WrongOldAccountPassword());
			}
			
			var passwordData = _passwordHasher.HashPassword(dto.NewPassword);
			account.UpdatePasswordData(passwordData);
			
			_uow.Save(account);
			_uow.Commit();
			
			return Result.Success();
		}

		public Result DeleteLegalCounterpartyAccount(DeleteLegalCounterpartyAccountRequest dto)
		{
			var legalCounterpartyId = dto.ErpCounterpartyId;
			var externalAccounts =
				_externalLegalCounterpartyEmailsRepository.GetExternalLegalCounterpartyAccounts(_uow, legalCounterpartyId, dto.Email);

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

			var account = externalAccounts.First();
			var checkPassword =
				_passwordHasher.VerifyHashedPassword(account.AccountPasswordSalt, account.AccountPasswordHash, dto.Password);

			if(!checkPassword)
			{
				Result.Failure(LegalCounterpartyControllerError.WrongAccountPassword());
			}

			var result = _accountHandler.DeleteAccount(_uow, account);

			if(result.IsFailure)
			{
				return result;
			}
			
			_uow.Commit();

			return Result.Success();
		}
	}
}
