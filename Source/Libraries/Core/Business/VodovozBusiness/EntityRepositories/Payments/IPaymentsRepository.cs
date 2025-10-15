using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;
using Vodovoz.Services;

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
	}
}
