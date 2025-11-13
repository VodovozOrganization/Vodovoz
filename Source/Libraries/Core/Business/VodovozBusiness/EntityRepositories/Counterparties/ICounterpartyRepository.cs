using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
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
		IQueryable<int> GetLastClassificationCalculationSettingsId(IUnitOfWork uow);
		IQueryable<CounterpartyClassification> GetLastExistingClassificationsForCounterparties(IUnitOfWork uow, int lastCalculationSettingsId);
		IQueryable<CounterpartyClassification> CalculateCounterpartyClassifications(IUnitOfWork uow, CounterpartyClassificationCalculationSettings calculationSettings);
		IQueryable<decimal> GetCounterpartyOrdersActuaSums(IUnitOfWork unitOfWork, int counterpartyId, OrderStatus[] orderStatuses, bool isExcludePaidOrders = false, DateTime maxDeliveryDate = default);
		IQueryable<CounterpartyCashlessBalanceNode> GetCounterpartiesCashlessBalance(IUnitOfWork unitOfWork, OrderStatus[] orderStatuses, int counterpartyId = default, DateTime maxDeliveryDate = default);
		IQueryable<CounterpartyInnName> GetCounterpartyNamesByInn(IUnitOfWork unitOfWork, IList<string> inns);
		/// <summary>
		/// Получение юр лиц по ИНН
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="inn"></param>
		/// <returns></returns>
		IEnumerable<LegalCounterpartyInfo> GetLegalCounterpartiesByInn(IUnitOfWork uow, string inn);
		bool CounterpartyByIdExists(IUnitOfWork uow, int counterpartyId);
		bool CounterpartyByInnExists(IUnitOfWork uow, string inn);
	}
}
