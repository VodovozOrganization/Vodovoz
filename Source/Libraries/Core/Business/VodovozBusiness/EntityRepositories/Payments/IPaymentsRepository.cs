using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;
using Vodovoz.Services;
using VodovozBusiness.EntityRepositories.Nodes;

namespace Vodovoz.EntityRepositories.Payments
{
	public interface IPaymentsRepository
	{
		IList<PaymentByCardOnlineNode> GetPaymentsByTwoMonths(IUnitOfWork uow, DateTime date);
		IEnumerable<string> GetAllShopsFromTinkoff(IUnitOfWork uow);
		bool NotManuallyPaymentFromBankClientExists(
			IUnitOfWork uow,
			DateTime date,
			int number,
			string organisationInn,
			string counterpartyInn,
			string accountNumber,
			decimal sum);
		decimal GetCounterpartyLastBalance(IUnitOfWork uow, int counterpartyId, int organizationId);
		int GetMaxPaymentNumFromManualPayments(IUnitOfWork uow, int counterpartyId, int organizationId);
		IEnumerable<Payment> GetAllUndistributedPayments(IUnitOfWork uow);
		IEnumerable<Payment> GetAllDistributedPayments(IUnitOfWork uow);
		IEnumerable<Payment> GetNotCancelledRefundedPayments(IUnitOfWork uow, int orderId);
		IList<NotFullyAllocatedPaymentNode> GetAllNotFullyAllocatedPaymentsByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId, bool allocateCompletedPayments);
		IQueryOver<Payment, Payment> GetAllUnallocatedBalances(IUnitOfWork uow, int closingDocumentDeliveryScheduleId);
		bool PaymentFromAvangardExists(IUnitOfWork uow, DateTime paidDate, int orderNum, decimal orderSum);
		IQueryable<PaymentNode> GetCounterpartyPaymentNodes(IUnitOfWork uow, int counterpartyId, string counterpartyInn);
		IQueryable<decimal> GetCounterpartyPaymentsSums(IUnitOfWork uow, int counterpartyId, string counterpartyInn);

		/// <summary>
		/// Возвращает данные по платежам контрагентов по организации
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="counterpartiesIds">Id контрагентов</param>
		/// <param name="organizationId">Id организации</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные платежей контрагентов</returns>
		Task<IDictionary<int, CounterpartyPaymentsDataNode[]>> GetCounterpatiesPaymentsData(IUnitOfWork uow, IEnumerable<int> counterpartiesIds, int organizationId, CancellationToken cancellationToken);
	}
}
