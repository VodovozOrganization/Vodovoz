using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Core.Domain.Clients.Accounts.Events;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Contacts;
using VodovozBusiness.Errors.Clients;
using VodovozBusiness.Services.Clients;

namespace Vodovoz.Application.Clients
{
	public class LegalCounterpartyAccountHandler : ILegalCounterpartyAccountHandler
	{
		/// <inheritdoc/>
		public Result DeleteAccountFromDesktop(IUnitOfWork uow, ExternalLegalCounterpartyAccount account)
		{
			var resultCheckEmail = CheckEmailAndResetType(uow, account, out var email);
			
			if(resultCheckEmail.IsFailure)
			{
				return resultCheckEmail;
			}

			var @event = LogoutLegalAccountEvent.Create(account.LegalCounterpartyId, email.Address);
			
			uow.Save(@event);
			uow.Delete(account);
			return Result.Success();
		}

		/// <inheritdoc/>
		public Result DeleteAccount(IUnitOfWork uow, ExternalLegalCounterpartyAccount account)
		{
			var resultCheckEmail = CheckEmailAndResetType(uow, account, out var email);
			
			if(resultCheckEmail.IsFailure)
			{
				return resultCheckEmail;
			}
			
			uow.Delete(account);
			return Result.Success();
		}
		
		private Result CheckEmailAndResetType(IUnitOfWork uow, ExternalLegalCounterpartyAccount account, out Email accountEmail)
		{
			accountEmail = uow.GetById<Email>(account.LegalCounterpartyEmailId);

			if(accountEmail is null)
			{
				return Result.Failure(ExternalLegalCounterpartyAccountErrors.EmailNotFound);
			}

			if(accountEmail.EmailType != null && accountEmail.EmailType.EmailPurpose == EmailPurpose.ExternalAccount)
			{
				accountEmail.EmailType = null;
				uow.Save(accountEmail);
			}

			return Result.Success();
		}
	}
}
