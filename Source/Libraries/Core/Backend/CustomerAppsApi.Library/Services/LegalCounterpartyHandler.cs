using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;

namespace CustomerAppsApi.Library.Services
{
	public class LegalCounterpartyHandler
	{
		private readonly IEmailRepository _emailRepository;

		public LegalCounterpartyHandler(
			IEmailRepository emailRepository)
		{
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
		}
		
		public Email GetEmailForLink(IUnitOfWork uow, int counterpartyId, string email)
		{
			return _emailRepository.GetEmailCounterparty(uow, counterpartyId, email);
		}
	}
}
