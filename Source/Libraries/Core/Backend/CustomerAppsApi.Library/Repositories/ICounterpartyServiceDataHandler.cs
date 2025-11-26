using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Counterparties;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICounterpartyServiceDataHandler
	{
		ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			string phoneNumber,
			CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			CounterpartyFrom counterpartyFrom);
		ExternalCounterparty GetExternalCounterparty(IUnitOfWork uow, string phoneNumber, CounterpartyFrom counterpartyFrom);
		bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyId, string phoneNumber);
		IEnumerable<LegalCounterpartyInfo> GetNaturalCounterpartyLegalCustomers(IUnitOfWork uow, int counterpartyId, string phone);
		RoboAtsCounterpartyName GetRoboatsCounterpartyName(IUnitOfWork uow, string counterpartyName);
		RoboAtsCounterpartyPatronymic GetRoboatsCounterpartyPatronymic(IUnitOfWork uow, string counterpartyPatronymic);
		Task<int> GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId);
		Email GetEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId);
		EmailType GetEmailTypeForReceipts(IUnitOfWork uow);
		OrganizationOwnershipType GetOrganizationOwnershipTypeByCode(IUnitOfWork uow, string code);
		bool CounterpartyExists(IUnitOfWork uow, int counterpartyId);
		ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId, string phone);
		ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int phoneId);
		bool CounterpartyExists(IUnitOfWork uow, string inn);
		IEnumerable<PhoneInfo> GetConnectedCustomerPhones(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId);
		IEnumerable<Email> GetEmailForLinking(IUnitOfWork uow, int legalCounterpartyId, string dtoEmail);
	}
}
