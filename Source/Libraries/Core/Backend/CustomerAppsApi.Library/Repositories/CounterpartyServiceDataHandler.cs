using System;
using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Edo;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Roboats;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Library.Repositories
{
	public class CounterpartyServiceDataHandler : ICounterpartyServiceDataHandler
	{
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IExternalCounterpartyMatchingRepository _externalCounterpartyMatchingRepository;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IPhoneRepository _phoneRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly ICachedBottlesDebtRepository _cachedBottlesDebtRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IExternalLegalCounterpartyAccountRepository _legalCounterpartyAccountRepository;

		public CounterpartyServiceDataHandler(
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IExternalCounterpartyMatchingRepository externalCounterpartyMatchingRepository,
			IRoboatsRepository roboatsRepository,
			IPhoneRepository phoneRepository,
			IEmailRepository emailRepository,
			ICounterpartyRepository counterpartyRepository,
			ICachedBottlesDebtRepository cachedBottlesDebtRepository,
			IOrganizationRepository organizationRepository,
			IExternalLegalCounterpartyAccountRepository legalCounterpartyAccountRepository
			)
		{
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_externalCounterpartyMatchingRepository =
				externalCounterpartyMatchingRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyMatchingRepository));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_cachedBottlesDebtRepository =
				cachedBottlesDebtRepository ?? throw new ArgumentNullException(nameof(cachedBottlesDebtRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_legalCounterpartyAccountRepository =
				legalCounterpartyAccountRepository ?? throw new ArgumentNullException(nameof(legalCounterpartyAccountRepository));
		}

		public ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			string phoneNumber,
			CounterpartyFrom counterpartyFrom)
		{
			return _externalCounterpartyRepository.GetExternalCounterparty(uow, externalCounterpartyId, phoneNumber, counterpartyFrom);
		}
		
		public ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			CounterpartyFrom counterpartyFrom)
		{
			return _externalCounterpartyRepository.GetExternalCounterparty(uow, externalCounterpartyId, counterpartyFrom);
		}
		
		public ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			string phoneNumber,
			CounterpartyFrom counterpartyFrom)
		{
			return _externalCounterpartyRepository.GetExternalCounterparty(uow, phoneNumber, counterpartyFrom);
		}

		public bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyId, string phoneNumber)
		{
			return _externalCounterpartyMatchingRepository.ExternalCounterpartyMatchingExists(uow, externalCounterpartyId, phoneNumber);
		}
		
		public RoboAtsCounterpartyName GetRoboatsCounterpartyName(IUnitOfWork uow, string counterpartyName)
		{
			return _roboatsRepository.GetCounterpartyName(uow, counterpartyName);
		}
		
		public RoboAtsCounterpartyPatronymic GetRoboatsCounterpartyPatronymic(IUnitOfWork uow, string counterpartyPatronymic)
		{
			return _roboatsRepository.GetCounterpartyPatronymic(uow, counterpartyPatronymic);
		}
		
		public int GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, int counterpartyDebtCacheMinutes)
		{
			return _cachedBottlesDebtRepository.GetCounterpartyBottlesDebt(uow, counterpartyId, counterpartyDebtCacheMinutes);
		}
		
		public Email GetEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			return _emailRepository.GetEmailForExternalCounterparty(uow, counterpartyId);
		}
		
		public EmailType GetEmailTypeForReceipts(IUnitOfWork uow)
		{
			return _emailRepository.GetEmailTypeForReceipts(uow);
		}

		public OrganizationOwnershipType GetOrganizationOwnershipTypeByCode(IUnitOfWork uow, string code)
		{
			return _organizationRepository.GetOrganizationOwnershipTypeByCode(uow, code);
		}

		public bool CounterpartyExists(IUnitOfWork uow, int counterpartyId)
		{
			return _counterpartyRepository.CounterpartyByIdExists(uow, counterpartyId);
		}

		public bool CounterpartyExists(IUnitOfWork uow, string inn)
		{
			return _counterpartyRepository.CounterpartyByInnExists(uow, inn);
		}

		public IEnumerable<Email> GetEmailForLinking(IUnitOfWork uow, int legalCounterpartyId, string email)
		{
			return _emailRepository.GetEmailForLinkingLegalCounterparty(uow, legalCounterpartyId, email);
		}

		/// <inheritdoc/>
		public bool PhoneExists(IUnitOfWork unitOfWork, int counterpartyId, string phoneNumber)
		{
			return _phoneRepository.PhoneNumberExists(unitOfWork, phoneNumber, counterpartyId);
		}
	}
}
