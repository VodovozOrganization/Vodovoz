using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Counterparties;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
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
		private readonly IConnectedCustomerRepository _connectedCustomerRepository;

		public CounterpartyServiceDataHandler(
			IExternalCounterpartyRepository externalCounterpartyRepository,
			IExternalCounterpartyMatchingRepository externalCounterpartyMatchingRepository,
			IRoboatsRepository roboatsRepository,
			IPhoneRepository phoneRepository,
			IEmailRepository emailRepository,
			ICounterpartyRepository counterpartyRepository,
			ICachedBottlesDebtRepository cachedBottlesDebtRepository,
			IOrganizationRepository organizationRepository,
			IConnectedCustomerRepository connectedCustomerRepository
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
			_connectedCustomerRepository =
				connectedCustomerRepository ?? throw new ArgumentNullException(nameof(connectedCustomerRepository));
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
		
		public IEnumerable<LegalCounterpartyInfo> GetLegalCustomersByInn(IUnitOfWork uow, GetLegalCustomersByInnDto dto)
		{
			var counterparties = _counterpartyRepository.GetLegalCounterpartiesByInn(uow, dto.Inn);

			var counterpartiesIds = counterparties
				.Select(x => x.ErpCounterpartyId)
				.Distinct()
				.ToArray();

			var phones =
				_phoneRepository
					.GetPhoneInfoByCounterpartiesIds(uow, counterpartiesIds)
					.ToLookup(x => x.ErpCounterpartyId);
			
			var emails =
				_emailRepository
					.GetEmailInfoByCounterpatiesIds(uow, counterpartiesIds)
					.ToLookup(x => x.ErpCounterpartyId);

			foreach(var counterpartyInfo in counterparties)
			{
				var counterpartyId = counterpartyInfo.ErpCounterpartyId;
				var counterpartyPhones = new List<PhoneInfo>();
				var counterpartyEmails = new List<EmailInfo>();

				if(phones.Contains(counterpartyId))
				{
					counterpartyPhones.AddRange(phones[counterpartyId]);
				}

				counterpartyInfo.Phones = counterpartyPhones;
				
				if(emails.Contains(counterpartyId))
				{
					counterpartyEmails.AddRange(emails[counterpartyId]);
				}
				
				counterpartyInfo.Emails = counterpartyEmails;
			}
			
			return counterparties;
		}
		
		public IEnumerable<LegalCounterpartyInfo> GetNaturalCounterpartyLegalCustomers(IUnitOfWork uow, int counterpartyId, string phone)
		{
			var counterparties = _connectedCustomerRepository.GetConnectedCustomers(uow, counterpartyId, phone);

			var counterpartiesIds = counterparties
				.Select(x => x.ErpCounterpartyId)
				.Distinct()
				.ToArray();

			var phones =
				_phoneRepository
					.GetPhoneInfoByCounterpartiesIds(uow, counterpartiesIds)
					.ToLookup(x => x.ErpCounterpartyId);
			
			var emails =
				_emailRepository
					.GetEmailInfoByCounterpatiesIds(uow, counterpartiesIds)
					.ToLookup(x => x.ErpCounterpartyId);

			foreach(var counterpartyInfo in counterparties)
			{
				var currentCounterpartyId = counterpartyInfo.ErpCounterpartyId;
				var counterpartyPhones = new List<PhoneInfo>();
				var counterpartyEmails = new List<EmailInfo>();

				if(phones.Contains(currentCounterpartyId))
				{
					counterpartyPhones.AddRange(phones[currentCounterpartyId]);
				}

				counterpartyInfo.Phones = counterpartyPhones;
				
				if(emails.Contains(currentCounterpartyId))
				{
					counterpartyEmails.AddRange(emails[currentCounterpartyId]);
				}
				
				counterpartyInfo.Emails = counterpartyEmails;
			}
			
			return counterparties;
		}
		
		public RoboAtsCounterpartyName GetRoboatsCounterpartyName(IUnitOfWork uow, string counterpartyName)
		{
			return _roboatsRepository.GetCounterpartyName(uow, counterpartyName);
		}
		
		public RoboAtsCounterpartyPatronymic GetRoboatsCounterpartyPatronymic(IUnitOfWork uow, string counterpartyPatronymic)
		{
			return _roboatsRepository.GetCounterpartyPatronymic(uow, counterpartyPatronymic);
		}
		
		public async Task<int> GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId)
		{
			return await _cachedBottlesDebtRepository.GetCounterpartyBottlesDebt(uow, counterpartyId);
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

		public ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId, string phone)
		{
			return _connectedCustomerRepository.GetConnectedCustomer(uow, legalCounterpartyId, naturalCounterpartyId, phone);
		}

		public ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int phoneId)
		{
			return _connectedCustomerRepository.GetConnectedCustomer(uow, legalCounterpartyId, phoneId);
		}

		public bool CounterpartyExists(IUnitOfWork uow, string inn)
		{
			return _counterpartyRepository.CounterpartyByInnExists(uow, inn);
		}

		public IEnumerable<PhoneInfo> GetConnectedCustomerPhones(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId)
		{			
			return _connectedCustomerRepository.GetConnectedCustomerPhones(uow, legalCounterpartyId, naturalCounterpartyId);
		}

		public IEnumerable<Email> GetEmailForLinking(IUnitOfWork uow, int legalCounterpartyId, string dtoEmail)
		{
			return _emailRepository.GetEmailForLinkingLegalCounterparty(uow, legalCounterpartyId, dtoEmail);
		}
	}
}
