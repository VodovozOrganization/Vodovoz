using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface ICounterpartyRepository
	{
		QueryOver<Counterparty> ActiveClientsQuery();
		IList<Counterparty> GetCounterpartiesByCode1c(IUnitOfWork uow, string[] codes1c);
		IList<ClientCameFrom> GetPlacesClientCameFrom(IUnitOfWork uow, bool doNotShowArchive, bool orderByDescending = false);
		Counterparty GetCounterpartyByINN(IUnitOfWork uow, string inn);
		IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn);
		IList<Counterparty> GetNotArchivedCounterpartiesByPhoneNumber(IUnitOfWork uow, string phoneNumber);
		IList<string> GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(IUnitOfWork uow, string phoneNumber, int currentCounterpartyId);
		Dictionary<string, List<string>> GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(IUnitOfWork uow, List<Phone> phones, int currentCounterpartyId);
		Counterparty GetCounterpartyByAccount(IUnitOfWork uow, string accountNumber);
		IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow);
		IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow);
		PaymentType[] GetPaymentTypesForCash();
		PaymentType[] GetPaymentTypesForCashless();
		bool IsCashPayment(PaymentType payment);
		bool IsCashlessPayment(PaymentType payment);
		IList<CounterpartyTo1CNode> GetCounterpartiesWithInnAndAnyContact(IUnitOfWork uow);
		IList<Counterparty> GetDealers();
		Counterparty GetCounterpartyByPersonalAccountIdInEdo(IUnitOfWork uow, string edxClientId);
		EdoOperator GetEdoOperatorByCode(IUnitOfWork uow, string edoOperatorCode);
		IList<EdoContainer> GetEdoContainersByCounterpartyId(IUnitOfWork uow, int counterpartyId);
		IDictionary<int, string> GetAllCounterpartyIdsAndNames(IUnitOfWork unitOfWork);
		IQueryable<CounterpartyClassification> GetLastExistingClassificationsForCounterparties(IUnitOfWork unitOfWork);
		IQueryable<CounterpartyClassification> CalculateCounterpartyClassifications(IUnitOfWork unitOfWork, CounterpartyClassificationCalculationSettings calculationSettings);
	}
}
