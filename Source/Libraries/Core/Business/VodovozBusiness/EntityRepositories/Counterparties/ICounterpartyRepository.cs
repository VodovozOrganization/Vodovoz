using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
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
		/// <summary>
		/// Получение общей суммы задолженности контрагента по всем организациям
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="counterpartyId"></param>
		/// <returns></returns>
		decimal GetTotalDebt(IUnitOfWork unitOfWork, int counterpartyId);
		/// <summary>
		/// Получение суммы задолженности контрагента по конкретной организации
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="counterpartyId"></param>
		/// <param name="organizationId"></param>
		/// <returns></returns>
		decimal GetDebtByOrganization(IUnitOfWork unitOfWork, int counterpartyId, int organizationId);
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
		/// Возвращает email контрагентов по их Id
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterparties">Id контрагентов</param>
		/// <returns>Email адреса контрагента</returns>
		IDictionary<int, Email[]> GetCounterpartyEmails(IUnitOfWork uow, IEnumerable<int> counterparties);

		/// <summary>
		/// Возвращает телефоны контрагентов по их Id
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartiesIds">Id контрагентов</param>
		/// <returns>Телефоны контрагента</returns>
		IDictionary<int, Phone[]> GetCounterpartyPhones(IUnitOfWork uow, IEnumerable<int> counterpartiesIds);

		/// <summary>
		/// Возвращает телефоны для связи по заказам контрагентов по их Id
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartiesIds">Id контрагентов</param>
		/// <returns>Телефоны, указанные как контактные, в заказах контрагента</returns>
		IDictionary<int, Phone[]> GetCounterpartyOrdersContactPhones(IUnitOfWork uow, IEnumerable<int> counterpartiesIds);

		/// <summary>
		/// Изменения в контрагенте с определённой даты
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="fromDate">Дата начала для поиска изменений</param>
		/// <returns></returns>
		IList<CounterpartyChangesDto> GetCounterpartyChanges(IUnitOfWork unitOfWork, DateTime fromDate);
	}
}
